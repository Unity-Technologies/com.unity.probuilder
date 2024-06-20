using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.ProBuilder
{
#if UNITY_EDITOR
    public sealed partial class ProBuilderMesh : ISerializationCallbackReceiver
#else
    public sealed partial class ProBuilderMesh
#endif
    {
        static HashSet<int> s_CachedHashSet = new HashSet<int>();

#if UNITY_EDITOR
        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            InvalidateCaches();
        }

#if ENABLE_DRIVEN_PROPERTIES
        // Using the internal callbacks here to avoid registering this component as "enable-able"
        void OnEnableINTERNAL()
        {
            ApplyDrivenProperties();
        }

        void OnDisableINTERNAL()
        {
            // Don't call DrivenPropertyManager.Unregister in OnDestroy. At that point GameObject::m_ActivationState is
            // already set to kDestroying, and DrivenPropertyManager.Unregister will try to revert the driven values to
            // their previous state (which will assert that the object is _not_ being destroyed)
            ClearDrivenProperties();
        }

        internal void ApplyDrivenProperties()
        {
            SerializationUtility.RegisterDrivenProperty(this, this, "m_Mesh");
            if(gameObject != null && gameObject.TryGetComponent(out MeshCollider meshCollider))
                SerializationUtility.RegisterDrivenProperty(this, meshCollider, "m_Mesh");
        }

        internal void ClearDrivenProperties()
        {
            SerializationUtility.UnregisterDrivenProperty(this, this, "m_Mesh");
            if(gameObject != null && gameObject.TryGetComponent(out MeshCollider meshCollider))
                SerializationUtility.UnregisterDrivenProperty(this, meshCollider, "m_Mesh");
        }
#endif
#endif

        void Awake()
        {
            EnsureMeshFilterIsAssigned();
            EnsureMeshColliderIsAssigned();
            //Ensure no element is selected at awake
            ClearSelection();

            if(vertexCount > 0
               && faceCount > 0
               && meshSyncState == MeshSyncState.Null)
            {
                using (new NonVersionedEditScope(this))
                {
                    Rebuild();
                    meshWasInitialized?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Rebuilds the mesh positions and submeshes, and then recalculates the normals, collisions,
        /// UVs, tangents, and colors.
        /// </summary>
        /// <seealso cref="ToMesh"/>
        /// <seealso cref="Refresh"/>
        void Reset()
        {
            if (meshSyncState != MeshSyncState.Null)
            {
                Rebuild();
                if (componentHasBeenReset != null)
                    componentHasBeenReset(this);
            }
        }

        /// <summary>
        /// Cleans up when the ProBuilderMesh component is removed (that is, when a ProBuilder mesh
        /// is converted to a standard Unity mesh).
        /// </summary>
        /// <seealso cref="meshWillBeDestroyed" />
        /// <seealso cref="preserveMeshAssetOnDestroy"/>
        void OnDestroy()
        {
            // Always re-enable the MeshFilter when the ProBuilderMesh component is removed
            if (m_MeshFilter != null || this.TryGetComponent(out m_MeshFilter))
                m_MeshFilter.hideFlags = HideFlags.None;

            if (componentWillBeDestroyed != null)
                componentWillBeDestroyed(this);

            // Time.frameCount is zero when loading scenes in the Editor. It's the only way I could figure to
            // differentiate between OnDestroy invoked from user delete & editor scene loading.
            if (!preserveMeshAssetOnDestroy &&
                Application.isEditor &&
                !Application.isPlaying &&
                Time.frameCount > 0)
            {
                DestroyUnityMesh();
            }
        }

        internal void DestroyUnityMesh()
        {
            if (meshWillBeDestroyed != null)
                meshWillBeDestroyed(this);
            else
                DestroyImmediate(gameObject.GetComponent<MeshFilter>().sharedMesh, true);
        }

        /// <summary>
        /// Increments the mesh version index. This helps ProBuilder track
        /// when the mesh changes.
        /// </summary>
        void IncrementVersionIndex()
        {
            // it doesn't matter if the version index wraps. the important thing is that it is changed.
            unchecked
            {
                if (++m_VersionIndex == 0)
                    m_VersionIndex = 1;
                m_InstanceVersionIndex = m_VersionIndex;
            }
        }

        /// <summary>
        /// Resets (empties) all the attribute arrays on this object and clears any selected elements.
        /// The attribute arrays include faces, positions, texture UVs, tangents, shared vertices,
        /// shared textures, and vertex colors.
        /// </summary>
        public void Clear()
        {
            // various editor tools expect faces & vertices to always be valid.
            // ideally we'd null everything here, but that would break a lot of existing code.
            m_Faces = new Face[0];
            m_Positions = new Vector3[0];
            m_Textures0 = new Vector2[0];
            m_Textures2 = null;
            m_Textures3 = null;
            m_Tangents = null;
            m_SharedVertices = new SharedVertex[0];
            m_SharedTextures = new SharedVertex[0];
            InvalidateSharedVertexLookup();
            InvalidateSharedTextureLookup();
            m_Colors = null;
            m_MeshFormatVersion = k_MeshFormatVersion;
            IncrementVersionIndex();
            ClearSelection();
        }

        internal void EnsureMeshFilterIsAssigned()
        {
            if (filter == null)
                m_MeshFilter = gameObject.AddComponent<MeshFilter>();

#if UNITY_EDITOR
            m_MeshFilter.hideFlags = k_MeshFilterHideFlags;
#endif

            if (!renderer.isPartOfStaticBatch && filter.sharedMesh != m_Mesh)
                filter.sharedMesh = m_Mesh;
        }

        internal static ProBuilderMesh CreateInstanceWithPoints(Vector3[] positions)
        {
            if (positions.Length % 4 != 0)
            {
                Log.Warning("Invalid Geometry. Make sure vertices in are pairs of 4 (faces).");
                return null;
            }

            GameObject go = new GameObject();
            go.name = "ProBuilder Mesh";
            ProBuilderMesh pb = go.AddComponent<ProBuilderMesh>();
            pb.m_MeshFormatVersion = k_MeshFormatVersion;
            pb.GeometryWithPoints(positions);

            return pb;
        }

        /// <summary>
        /// Creates a new GameObject with a ProBuilderMesh, <see cref="UnityEngine.MeshFilter" />,
        /// and <see cref="UnityEngine.MeshRenderer" /> component but leaves the position and face
        /// information empty.
        /// </summary>
        /// <returns>A reference to the new ProBuilderMesh component.</returns>
        public static ProBuilderMesh Create()
        {
            var go = new GameObject();
            var pb = go.AddComponent<ProBuilderMesh>();
            pb.m_MeshFormatVersion = k_MeshFormatVersion;
            pb.Clear();
            return pb;
        }

        /// <summary>
        /// Creates a new GameObject with a ProBuilderMesh, <see cref="UnityEngine.MeshFilter" />, and <see cref="UnityEngine.MeshRenderer" /> component.
        /// Then it initializes the ProBuilderMesh with the specified sets of positions and faces.
        /// </summary>
        /// <param name="positions">Vertex positions array.</param>
        /// <param name="faces">Faces array.</param>
        /// <returns>A reference to the new ProBuilderMesh component.</returns>
        public static ProBuilderMesh Create(IEnumerable<Vector3> positions, IEnumerable<Face> faces)
        {
            GameObject go = new GameObject();
            ProBuilderMesh pb = go.AddComponent<ProBuilderMesh>();
            go.name = "ProBuilder Mesh";
            pb.m_MeshFormatVersion = k_MeshFormatVersion;
            pb.RebuildWithPositionsAndFaces(positions, faces);
            return pb;
        }

        /// <summary>
        /// Creates a new GameObject with a ProBuilderMesh, <see cref="UnityEngine.MeshFilter" />, and
        /// <see cref="UnityEngine.MeshRenderer" /> component. Then it initializes the ProBuilderMesh
        /// with the specified sets of positions and faces, and if specified, coincident vertices,
        /// texture coordinates, and materials.
        /// </summary>
        /// <param name="vertices">Array of vertex positions to use.</param>
        /// <param name="faces">Array of faces to use.</param>
        /// <param name="sharedVertices">Optional <see cref="SharedVertex" /> array to define the coincident vertices.</param>
        /// <param name="sharedTextures">Optional <see cref="SharedVertex" /> array to define the coincident texture coordinates (UV0).</param>
        /// <param name="materials">Optional array of materials to be assigned to the <see cref="UnityEngine.MeshRenderer" />.</param>
        /// <returns>A reference to the new ProBuilderMesh component.</returns>
        public static ProBuilderMesh Create(
            IList<Vertex> vertices,
            IList<Face> faces,
            IList<SharedVertex> sharedVertices = null,
            IList<SharedVertex> sharedTextures = null,
            IList<Material> materials = null)
        {
            var go = new GameObject();
            go.name = "ProBuilder Mesh";
            var mesh = go.AddComponent<ProBuilderMesh>();
            if (materials != null)
                mesh.renderer.sharedMaterials = materials.ToArray();
            mesh.m_MeshFormatVersion = k_MeshFormatVersion;
            mesh.SetVertices(vertices);
            mesh.faces = faces;
            mesh.sharedVertices = sharedVertices;
            mesh.sharedTextures = sharedTextures != null ? sharedTextures.ToArray() : null;
            mesh.ToMesh();
            mesh.Refresh();
            return mesh;
        }

        internal void GeometryWithPoints(Vector3[] points)
        {
            // Wrap in faces
            Face[] f = new Face[points.Length / 4];

            for (int i = 0; i < points.Length; i += 4)
            {
                f[i / 4] = new Face(new int[6]
                {
                    i + 0, i + 1, i + 2,
                    i + 1, i + 3, i + 2
                },
                        0,
                        AutoUnwrapSettings.tile,
                        0,
                        -1,
                        -1,
                        false);
            }

            Clear();
            positions = points;
            m_Faces = f;
            m_SharedVertices = SharedVertex.GetSharedVerticesWithPositions(points);
            InvalidateCaches();
            ToMesh();
            Refresh();
        }

        /// <summary>
        /// Clears all mesh attributes and reinitializes the mesh with new positions and face collections.
        /// </summary>
        /// <param name="vertices">New vertex positions array to use.</param>
        /// <param name="faces">New faces array to use.</param>
        public void RebuildWithPositionsAndFaces(IEnumerable<Vector3> vertices, IEnumerable<Face> faces)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            Clear();
            m_Positions = vertices.ToArray();
            m_Faces = faces.ToArray();
            m_SharedVertices = SharedVertex.GetSharedVerticesWithPositions(m_Positions);
            InvalidateSharedVertexLookup();
            InvalidateSharedTextureLookup();
            ToMesh();
            Refresh();
        }

        /// <summary>
        /// Wraps <see cref="ToMesh"/> and <see cref="Refresh"/>.
        /// </summary>
        internal void Rebuild()
        {
            ToMesh();
            Refresh();
        }

        /// <summary>
        /// Rebuilds the mesh positions and submeshes.
        ///
        /// If the vertex count matches the new positions array, the existing attributes are kept
        /// (except for UV2s, which are always cleared). Otherwise, the mesh is cleared.
        /// </summary>
        /// <param name="preferredTopology">You can specify MeshTopology.Quads if you don't want to use the default MeshTopology.Triangles. </param>
        public void ToMesh(MeshTopology preferredTopology = MeshTopology.Triangles)
        {
            bool usedInParticleSystem = false;

            // if the mesh vertex count hasn't been modified, we can keep most of the mesh elements around
            if (mesh == null)
            {
#if ENABLE_DRIVEN_PROPERTIES
                SerializationUtility.RegisterDrivenProperty(this, this, "m_Mesh");
#endif
                mesh = new Mesh() { name = $"pb_Mesh{GetInstanceID()}" };
            }
            else if (mesh.vertexCount != vertexCount)
            {
                usedInParticleSystem = MeshUtility.IsUsedInParticleSystem(this);
                mesh.Clear();
            }

            mesh.indexFormat = vertexCount > ushort.MaxValue ? Rendering.IndexFormat.UInt32 : Rendering.IndexFormat.UInt16;
            mesh.vertices = m_Positions;
            mesh.uv2 = null;

            if (m_MeshFormatVersion < k_MeshFormatVersion)
            {
                if (m_MeshFormatVersion < k_MeshFormatVersionSubmeshMaterialRefactor)
                    Submesh.MapFaceMaterialsToSubmeshIndex(this);
                if (m_MeshFormatVersion < k_MeshFormatVersionAutoUVScaleOffset)
                    UvUnwrapping.UpgradeAutoUVScaleOffset(this);
                m_MeshFormatVersion = k_MeshFormatVersion;
            }

            m_MeshFormatVersion = k_MeshFormatVersion;

            int materialCount = MaterialUtility.GetMaterialCount(renderer);

            Submesh[] submeshes = Submesh.GetSubmeshes(facesInternal, materialCount, preferredTopology);

            mesh.subMeshCount = submeshes.Length;

            // If the mesh does not have any submeshes, we don't want to do
            // any manipulation on the mesh's materials. We skip to the end
            // of the method and return.
            if (mesh.subMeshCount == 0)
            {
                FinalizeToMesh(usedInParticleSystem);
                return;
            }

            var currentSubmeshIndex = 0;
            var shouldReassignMaterials = false;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
#if DEVELOPER_MODE
                if (i >= materialCount)
                    Log.Warning("Submesh index " + i + " is out of bounds of the MeshRenderer materials array.");
                if (submeshes[i] == null)
                    throw new Exception("Attempting to assign a null submesh. " + i + "/" + materialCount);
#endif
                if (submeshes[i].m_Indexes.Length == 0)
                {
                    if (!shouldReassignMaterials)
                    {
                        MaterialUtility.s_MaterialArray.Clear();
                        renderer.GetSharedMaterials(MaterialUtility.s_MaterialArray);
                        shouldReassignMaterials = true;
                    }

                    submeshes[i].submeshIndex = -1;
                    MaterialUtility.s_MaterialArray.RemoveAt(currentSubmeshIndex);

                    foreach (var face in facesInternal)
                    {
                        if (currentSubmeshIndex < face.submeshIndex)
                            face.submeshIndex -= 1;
                    }

                    continue;
                }

                submeshes[i].submeshIndex = currentSubmeshIndex;
                mesh.SetIndices(submeshes[i].m_Indexes, submeshes[i].m_Topology, submeshes[i].submeshIndex, false);
                currentSubmeshIndex++;
            }

            if (mesh.subMeshCount < materialCount)
            {
                var delta = materialCount - mesh.subMeshCount;
                var start = MaterialUtility.s_MaterialArray.Count - delta;
                MaterialUtility.s_MaterialArray.RemoveRange(start, delta);

                shouldReassignMaterials = true;
            }

            if (shouldReassignMaterials)
                renderer.sharedMaterials = MaterialUtility.s_MaterialArray.ToArray();

            FinalizeToMesh(usedInParticleSystem);
        }

        private void FinalizeToMesh(bool usedInParticleSystem)
        {
            EnsureMeshFilterIsAssigned();

            if (usedInParticleSystem)
                MeshUtility.RestoreParticleSystem(this);

            IncrementVersionIndex();
        }

        /// <summary>
        /// Ensures that the UnityEngine.Mesh associated with this object is unique. When instantiating a ProBuilderMesh,
        /// the mesh asset will reference the original instance. If you are making a copy to edit, you must call
        /// MakeUnique to avoid modifying a shared mesh asset.
        /// </summary>
        public void MakeUnique()
        {
            mesh = mesh != null
                ? Instantiate(mesh)
                : new Mesh() { name = $"pb_Mesh{GetInstanceID()}" };

            if (meshSyncState == MeshSyncState.InSync)
            {
                filter.mesh = mesh;
                return;
            }

            ToMesh();
            Refresh();
        }

        /// <summary>
        /// Copies the mesh data from another mesh to this one.
        /// </summary>
        /// <param name="other">The mesh to copy from.</param>
        public void CopyFrom(ProBuilderMesh other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            Clear();
            positions = other.positions;
            sharedVertices = other.sharedVerticesInternal;
            SetSharedTextures(other.sharedTextureLookup);
            facesInternal = other.faces.Select(x => new Face(x)).ToArray();

            List<Vector4> uvs = new List<Vector4>();

            for (var i = 0; i < k_UVChannelCount; i++)
            {
                other.GetUVs(i, uvs);
                SetUVs(i, uvs);
            }

            tangents = other.tangents;
            colors = other.colors;
            userCollisions = other.userCollisions;
            selectable = other.selectable;
            unwrapParameters = new UnwrapParameters(other.unwrapParameters);
        }

        /// <summary>
        /// Recalculates mesh attributes: normals, collisions, UVs, tangents, and colors.
        /// </summary>
        /// <param name="mask">
        /// Optional. Specify a RefreshMask to indicate which components to update. Use this when you want to
        /// wait until later to rebuild some components in order to save processing power, since UVs and
        /// collisions are expensive to rebuild and can usually be deferred until the task finishes.
        /// </param>
        public void Refresh(RefreshMask mask = RefreshMask.All)
        {
            // Mesh
            if ((mask & RefreshMask.UV) > 0)
                RefreshUV(facesInternal);

            if ((mask & RefreshMask.Colors) > 0)
                RefreshColors();

            if ((mask & RefreshMask.Normals) > 0)
                RefreshNormals();

            if ((mask & RefreshMask.Tangents) > 0)
                RefreshTangents();

            if ((mask & RefreshMask.Collisions) > 0)
                EnsureMeshColliderIsAssigned();

            if ((mask & RefreshMask.Bounds) > 0 && mesh != null)
                mesh.RecalculateBounds();

            IncrementVersionIndex();
        }

        internal void EnsureMeshColliderIsAssigned()
        {
            if(gameObject.TryGetComponent<MeshCollider>(out MeshCollider collider))
            {
#if ENABLE_DRIVEN_PROPERTIES
                SerializationUtility.RegisterDrivenProperty(this, collider, "m_Mesh");
#endif
                collider.sharedMesh = (mesh != null && mesh.vertexCount > 0) ? mesh : null;
            }
        }

        /// <summary>
        /// Returns a new unused texture group ID.
        /// </summary>
        /// <param name="i">Optional value specifying the 'last' used ID. Defaults to 1.</param>
        /// <returns>
        /// An integer greater than or equal to the specified value `i`.
        /// </returns>
        internal int GetUnusedTextureGroup(int i = 1)
        {
            while (Array.Exists(facesInternal, element => element.textureGroup == i))
                i++;

            return i;
        }

        /// <summary>
        /// Tests whether the specified texture group ID is valid.
        /// </summary>
        /// <param name="group">ID of the texture group to check.</param>
        /// <returns>
        /// True if the specified group is greater than 0; false otherwise.
        /// </returns>
        static bool IsValidTextureGroup(int group)
        {
            return group > 0;
        }

        /// <summary>
        /// Returns a new unused element group.
        /// </summary>
        /// <param name="i">Optional value specifying the 'last' used group. Defaults to 1.</param>
        /// <returns>
        /// An integer greater than or equal to the specified value `i`.
        /// </returns>
        internal int UnusedElementGroup(int i = 1)
        {
            while (Array.Exists(facesInternal, element => element.elementGroup == i))
                i++;

            return i;
        }

        /// <summary>
        /// Rebuilds the UV arrays on the specified faces.
        ///
        /// This usually applies only to faces set to use Auto UVs. However, if ProBuilder can't detect
        /// any valid UV arrays, it resets the faces from Manual to Auto before rebuilding them.
        /// </summary>
        /// <param name="facesToRefresh">The set of faces to process.</param>
        /// <seealso cref="Refresh" />
        public void RefreshUV(IEnumerable<Face> facesToRefresh)
        {
            // If the UV array has gone out of sync with the positions array, reset all faces to Auto UV so that we can
            // correct the texture array.
            if (!HasArrays(MeshArrays.Texture0))
            {
                m_Textures0 = new Vector2[vertexCount];
                foreach (Face f in facesInternal)
                    f.manualUV = false;
                facesToRefresh = facesInternal;
            }

            s_CachedHashSet.Clear();

            foreach (var face in facesToRefresh)
            {
                if (face.manualUV || face.indexesInternal?.Length < 3)
                    continue;

                int textureGroup = face.textureGroup;

                if (!IsValidTextureGroup(textureGroup))
                    UvUnwrapping.Unwrap(this, face);
                else if (s_CachedHashSet.Add(textureGroup))
                    UvUnwrapping.ProjectTextureGroup(this, textureGroup, face.uv);
            }

            mesh.uv = m_Textures0;

            if (HasArrays(MeshArrays.Texture2))
                mesh.SetUVs(2, m_Textures2);
            if (HasArrays(MeshArrays.Texture3))
                mesh.SetUVs(3, m_Textures3);

            IncrementVersionIndex();
        }

        internal void SetGroupUV(AutoUnwrapSettings settings, int group)
        {
            if (!IsValidTextureGroup(group))
                return;

            foreach (var face in facesInternal)
            {
                if (face.textureGroup != group)
                    continue;

                face.uv = settings;
            }
        }

        /// <summary>
        /// Reapplies the vertex colors for this mesh.
        /// </summary>
        /// <seealso cref="Refresh" />
        void RefreshColors()
        {
            Mesh m = filter.sharedMesh;
            m.colors = m_Colors;
        }

        /// <summary>
        /// Applies a [vertex color](../manual/workflow-vertexcolors.html) to the specified <see cref="Face" />.
        /// </summary>
        /// <param name="face">The target face to apply the colors to.</param>
        /// <param name="color">The color to apply to this face's referenced vertices.</param>
        public void SetFaceColor(Face face, Color color)
        {
            if (face == null)
                throw new ArgumentNullException("face");

            if (!HasArrays(MeshArrays.Color))
                m_Colors = ArrayUtility.Fill(Color.white, vertexCount);

            foreach (int i in face.distinctIndexes)
                m_Colors[i] = color;
        }

        /// <summary>
        /// Sets a specific material on a collection of faces.
        /// </summary>
        /// <remarks>
        /// To apply the changes to the <see cref="UnityEngine.Mesh" />, call
        /// <see cref="ToMesh" /> and <see cref="Refresh" />.
        /// </remarks>
        /// <param name="faces">The faces to apply the material to.</param>
        /// <param name="material">The material to apply.</param>
        public void SetMaterial(IEnumerable<Face> faces, Material material)
        {
            var materials = renderer.sharedMaterials;
            var submeshCount = materials.Length;
            var index = -1;

            for (int i = 0; i < submeshCount && index < 0; i++)
            {
                if (materials[i] == material)
                    index = i;
            }

            if (index < 0)
            {
                // Material doesn't exist in MeshRenderer.sharedMaterials, now check if there is an unused
                // submeshIndex that we can replace with this value instead of creating a new entry.
                var submeshIndexes = new bool[submeshCount];

                foreach (var face in m_Faces)
                    submeshIndexes[Math.Clamp(face.submeshIndex, 0, submeshCount - 1)] = true;

                index = Array.IndexOf(submeshIndexes, false);

                // Found an unused submeshIndex, replace it with the material.
                if (index > -1)
                {
                    materials[index] = material;
                    renderer.sharedMaterials = materials;
                }
                else
                {
                    // There were no unused submesh indices, append another submesh and material.
                    index = materials.Length;
                    var copy = new Material[index + 1];
                    Array.Copy(materials, copy, index);
                    copy[index] = material;
                    renderer.sharedMaterials = copy;
                }
            }

            foreach (var face in faces)
                face.submeshIndex = index;

            IncrementVersionIndex();
        }

        /// <summary>
        /// Recalculates the normals for this mesh.
        /// </summary>
        /// <seealso cref="Refresh" />
        void RefreshNormals()
        {
            Normals.CalculateNormals(this);
            mesh.normals = m_Normals;
        }

        /// <summary>
        /// Recalculates the tangents on this mesh.
        /// </summary>
        /// <seealso cref="Refresh" />
        void RefreshTangents()
        {
            Normals.CalculateTangents(this);
            mesh.tangents = m_Tangents;
        }

        /// <summary>
        /// Finds the index of a vertex index (triangle) in an array of vertices.
        /// The index returned is called the common index, or shared index.
        /// </summary>
        /// <remarks>Aids in removing duplicate vertex indexes.</remarks>
        /// <param name="vertex">The vertex to find.</param>
        /// <returns>The common (or shared) index.</returns>
        internal int GetSharedVertexHandle(int vertex)
        {
            int res;

            if (m_SharedVertexLookup.TryGetValue(vertex, out res))
                return res;

            for (int i = 0; i < m_SharedVertices.Length; i++)
            {
                for (int n = 0, c = m_SharedVertices[i].Count; n < c; n++)
                    if (m_SharedVertices[i][n] == vertex)
                        return i;
            }

            throw new ArgumentOutOfRangeException("vertex");
        }

        internal HashSet<int> GetSharedVertexHandles(IEnumerable<int> vertices)
        {
            var lookup = sharedVertexLookup;
            HashSet<int> common = new HashSet<int>();
            foreach (var i in vertices)
                common.Add(lookup[i]);
            return common;
        }

        /// <summary>
        /// Returns a list of vertices that are coincident to any of the specified vertices.
        /// </summary>
        /// <param name="vertices">A collection of indices relative to the mesh positions.</param>
        /// <returns>A list of all vertices that share a position with any of the specified vertices.</returns>
        /// <exception cref="ArgumentNullException">The vertices parameter may not be null.</exception>
        public List<int> GetCoincidentVertices(IEnumerable<int> vertices)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            List<int> shared = new List<int>();
            GetCoincidentVertices(vertices, shared);
            return shared;
        }

        /// <summary>
        /// Populates a list of vertices that are coincident to any of the specified vertices.
        /// </summary>
        /// <param name="faces">A collection of faces to gather vertices from.</param>
        /// <param name="coincident">The list to clear and populate with any vertices that are coincident.</param>
        /// <exception cref="ArgumentNullException">The vertices and coincident parameters may not be null.</exception>
        public void GetCoincidentVertices(IEnumerable<Face> faces, List<int> coincident)
        {
            if (faces == null)
                throw new ArgumentNullException("faces");

            if (coincident == null)
                throw new ArgumentNullException("coincident");

            coincident.Clear();
            s_CachedHashSet.Clear();
            var lookup = sharedVertexLookup;

            foreach (var face in faces)
            {
                foreach (var v in face.distinctIndexesInternal)
                {
                    var common = lookup[v];

                    if (s_CachedHashSet.Add(common))
                    {
                        var indices = m_SharedVertices[common];

                        for (int i = 0, c = indices.Count; i < c; i++)
                            coincident.Add(indices[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Populates a list of vertices that are coincident to any of the specified vertices.
        /// </summary>
        /// <param name="edges">A collection of edges to gather vertices from.</param>
        /// <param name="coincident">The list to clear and populate with any vertices that are coincident.</param>
        /// <exception cref="ArgumentNullException">The vertices and coincident parameters may not be null.</exception>
        public void GetCoincidentVertices(IEnumerable<Edge> edges, List<int> coincident)
        {
            if (faces == null)
                throw new ArgumentNullException("edges");

            if (coincident == null)
                throw new ArgumentNullException("coincident");

            coincident.Clear();
            s_CachedHashSet.Clear();
            var lookup = sharedVertexLookup;

            foreach (var edge in edges)
            {
                var common = lookup[edge.a];

                if (s_CachedHashSet.Add(common))
                {
                    var indices = m_SharedVertices[common];

                    for (int i = 0, c = indices.Count; i < c; i++)
                        coincident.Add(indices[i]);
                }

                common = lookup[edge.b];

                if (s_CachedHashSet.Add(common))
                {
                    var indices = m_SharedVertices[common];

                    for (int i = 0, c = indices.Count; i < c; i++)
                        coincident.Add(indices[i]);
                }
            }
        }

        /// <summary>
        /// Populates a list of vertices that are coincident to any of the specified vertices.
        /// </summary>
        /// <param name="vertices">A collection of indices relative to the mesh positions.</param>
        /// <param name="coincident">The list to clear and populate with any vertices that are coincident.</param>
        /// <exception cref="ArgumentNullException">The vertices and coincident parameters may not be null.</exception>
        public void GetCoincidentVertices(IEnumerable<int> vertices, List<int> coincident)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            if (coincident == null)
                throw new ArgumentNullException("coincident");

            coincident.Clear();
            s_CachedHashSet.Clear();
            var lookup = sharedVertexLookup;

            foreach (var v in vertices)
            {
                var common = lookup[v];

                if (s_CachedHashSet.Add(common))
                {
                    var indices = m_SharedVertices[common];

                    for (int i = 0, c = indices.Count; i < c; i++)
                        coincident.Add(indices[i]);
                }
            }
        }

        /// <summary>
        /// Populates a list with all the vertices that are coincident to the specified vertex.
        /// </summary>
        /// <param name="vertex">An index relative to a positions array.</param>
        /// <param name="coincident">The list to clear and populate with any vertices that are coincident.</param>
        /// <exception cref="ArgumentNullException">The coincident list may not be null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The SharedVertex[] does not contain an entry for the requested vertex.</exception>
        public void GetCoincidentVertices(int vertex, List<int> coincident)
        {
            if (coincident == null)
                throw new ArgumentNullException("coincident");

            int common;

            if (!sharedVertexLookup.TryGetValue(vertex, out common))
                throw new ArgumentOutOfRangeException("vertex");

            var indices = m_SharedVertices[common];

            for (int i = 0, c = indices.Count; i < c; i++)
                coincident.Add(indices[i]);
        }

        /// <summary>
        /// Marks the specified vertices as coincident on this mesh.
        /// </summary>
        /// <remarks>
        /// Note that it is up to the caller to ensure that the specified vertices are indeed sharing a position.
        /// </remarks>
        /// <param name="vertices">The list of vertices to be marked as coincident.</param>
        public void SetVerticesCoincident(IEnumerable<int> vertices)
        {
            var lookup = sharedVertexLookup;
            List<int> coincident = new List<int>();
            GetCoincidentVertices(vertices, coincident);
            SharedVertex.SetCoincident(ref lookup, coincident);
            SetSharedVertices(lookup);
        }

        internal void SetTexturesCoincident(IEnumerable<int> vertices)
        {
            var lookup = sharedTextureLookup;
            SharedVertex.SetCoincident(ref lookup, vertices);
            SetSharedTextures(lookup);
        }

        internal void AddToSharedVertex(int sharedVertexHandle, int vertex)
        {
            if (sharedVertexHandle < 0 || sharedVertexHandle >= m_SharedVertices.Length)
                throw new ArgumentOutOfRangeException("sharedVertexHandle");

            m_SharedVertices[sharedVertexHandle].Add(vertex);
            InvalidateSharedVertexLookup();
        }

        internal void AddSharedVertex(SharedVertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException("vertex");

            m_SharedVertices = m_SharedVertices.Add(vertex);
            InvalidateSharedVertexLookup();
        }
    }
}
