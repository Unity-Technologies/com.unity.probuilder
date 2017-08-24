using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FbxExporters
{
    /// <summary>
    /// This component is applied to a prefab. It keeps the prefab sync'd up
    /// with an FBX file.
    ///
    /// Other parts of the ecosystem:
    ///         FbxPrefabInspector
    ///         FbxPrefabAutoUpdater
    /// </summary>
    public class FbxPrefab : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////////
        // TODO: Fields included in editor must be included in player, or it doesn't
        // build.

        /// <summary>
        /// Representation of the FBX file as it was when the prefab was
        /// last saved. This lets us update the prefab when the FBX changes.
        /// </summary>
        [SerializeField] // [HideInInspector]
        string m_fbxHistory;

        /// <summary>
        /// Which FBX file does this refer to?
        /// </summary>
        [SerializeField]
        [Tooltip("Which FBX file does this refer to?")]
        GameObject m_fbxModel;

        /// <summary>
        /// Should we auto-update this prefab when the FBX file is updated?
        /// <summary>
        [Tooltip("Should we auto-update this prefab when the FBX file is updated?")]
        [SerializeField]
        bool m_autoUpdate = true;

        //////////////////////////////////////////////////////////////////////
        // None of the code should be included in the build, because this
        // component is really only about the editor.
#if UNITY_EDITOR
        public class FbxRepresentation
        {
            public Dictionary<string, FbxRepresentation> c;

            public static FbxRepresentation FromTransform(Transform xfo) {
                var c = new Dictionary<string, FbxRepresentation>();
                foreach(Transform child in xfo) {
                    c.Add(child.name, FromTransform(child));
                }
                var fbxrep = new FbxRepresentation();
                fbxrep.c = c;
                return fbxrep;
            }

            // todo: use a real json parser
            static bool Consume(char expected, string json, ref int index, bool required = true) {
                while (true) { // loop breaks if index == json.Length
                    switch(json[index]) {
                        case ' ':
                        case '\t':
                        case '\n':
                            index++;
                            continue;
                    }
                    if (json[index] == expected) {
                        index++;
                        return true;
                    } else if (required) {
                        throw new System.Exception("expected " + expected + " at index " + index);
                    } else {
                        return false;
                    }
                }
            }

            static FbxRepresentation FromJsonHelper(string json, ref int index) {
                Consume('{', json, ref index);
                var fbxrep = new FbxRepresentation();
                if (Consume('}', json, ref index, required: false)) {
                    // this is a leaf; we're done.
                } else {
                    // this is a parent of one or more objects; store them.
                    fbxrep.c = new Dictionary<string, FbxRepresentation>();
                    do {
                        Consume('"', json, ref index);
                        int nameStart = index;
                        while (json[index] != '"') { index++; }
                        string name = json.Substring(nameStart, index - nameStart);
                        index++;
                        Consume(':', json, ref index);
                        var subrep = FromJsonHelper(json, ref index);
                        fbxrep.c.Add(name, subrep);
                    } while(Consume(',', json, ref index, required: false));
                    Consume('}', json, ref index);
                }
                return fbxrep;
            }

            public static FbxRepresentation FromJson(string json) {
                if (json.Length == 0) { return null; }
                int index = 0;
                return FromJsonHelper(json, ref index);
            }

            void ToJsonHelper(System.Text.StringBuilder builder) {
                builder.Append("{");
                if (c != null) {
                    bool first = true;
                    foreach(var kvp in c.OrderBy(kvp => kvp.Key)) {
                        if (!first) { builder.Append(','); }
                        else { first = false; }

                        builder.Append('"');
                        builder.Append(kvp.Key); // the name
                        builder.Append('"');
                        builder.Append(':');
                        kvp.Value.ToJsonHelper(builder);
                    }
                }
                builder.Append("}");
            }

            public string ToJson() {
                var builder = new System.Text.StringBuilder();
                ToJsonHelper(builder);
                return builder.ToString();
            }

            public static bool IsLeaf(FbxRepresentation rep) {
                return rep == null || rep.c == null;
            }

            public static HashSet<string> CopyKeySet(FbxRepresentation rep) {
                if (IsLeaf(rep)) {
                    return new HashSet<string>();
                } else {
                    return new HashSet<string>(rep.c.Keys);
                }
            }

            public static FbxRepresentation Find(FbxRepresentation rep, string key) {
                if (IsLeaf(rep)) { return null; }

                FbxRepresentation child;
                if (rep.c.TryGetValue(key, out child)) {
                    return child;
                }
                return null;
            }
        }

        /// <summary>
        /// Recursively perform the update.
        ///
        /// In terms of a 3-way merge, "oldFbx" is the "base".
        /// "newFbx" and "prefabInstance" are "theirs" and "ours".
        ///
        /// Whether the fbx or the prefab counts as "ours" depends on who the
        /// user is: a 3d modeler that updated the FBX, or the game programmer
        /// who's updating the prefab.
        /// So we don't use 3-way merge terminology.
        /// </summary>
        public static void TreeDiff(FbxRepresentation oldFbx,
                Transform newFbx,
                Transform prefabInstance,
                string indent = "")
        {
            // TODO: a better tree diff algorithm, e.g. Demaine et al 2009.
            //
            // For now we half-ass it: there's no possibility of handling
            //   renaming other than delete-and-add. Demaine handles it.
            // There's also no possibility of nicely handling hierarchy
            //   changes, but that's essentially impossible (NP-hard, no PTAS),
            //   so it's not likely worth even trying unless we feel like doing
            //   serious algorithmics.
            // 1. For each child in the union of the old, new, and prefab:
            //    7- old and new and prefab   => recur.
            //    6- old and new and !prefab  => skip (deleted by user)
            //    5- old and !new and prefab  => delete from prefab
            //    4- old and !new and !prefab => skip, we happen to match
            //    3- !old and new and prefab  => recur, we happen to match
            //    2- !old and new and !prefab => instantiate the new into the prefab
            //    1- !old and !new and prefab => skip (added by user)
            //    0- !old and !new and !prefab  => not possible given the loop
            var names = FbxRepresentation.CopyKeySet(oldFbx);
            foreach(Transform child in newFbx) { names.Add(child.name); }
            foreach(Transform child in prefabInstance) { names.Add(child.name); }

            indent += "  ";
            foreach(var name in names) {
                var oldChild = FbxRepresentation.Find(oldFbx, name);
                var newChild = newFbx.Find(name);
                var prefabChild = prefabInstance.Find(name);
                int index = ((oldChild == null) ? 0 : 4)
                    | ((newChild == null) ? 0 : 2)
                    | ((prefabChild == null) ? 0 : 1);
                switch(index) {
                    case 7:
                        //Debug.Log(indent + "recur into " + name);
                        TreeDiff(oldChild, newChild, prefabChild, indent);
                        break;
                    case 6:
                        //Debug.Log(indent + "skip user-deleted " + name);
                        break;
                    case 5:
                        //Debug.Log(indent + "delete " + name);
                        GameObject.DestroyImmediate(prefabChild.gameObject);
                        break;
                    case 4:
                        //Debug.Log(indent + "skip deleted in both new and prefabInstance " + name);
                        break;
                    case 3:
                        //Debug.Log(indent + "accidental match; recur into " + name);
                        TreeDiff(oldChild, newChild, prefabChild, indent);
                        break;
                    case 2:
                        //Debug.Log(indent + "instantiate into prefabInstance " + name);
                        prefabChild = GameObject.Instantiate(newChild);
                        prefabChild.parent = prefabInstance;
                        prefabChild.name = newChild.name;
                        prefabChild.localPosition = newChild.localPosition;
                        prefabChild.localRotation = newChild.localRotation;
                        prefabChild.localScale = newChild.localScale;
                        break;
                    case 1:
                        //Debug.Log(indent + "skip user-added node " + name);
                        break;
                    default:
                        // This shouldn't happen.
                        throw new System.NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Return whether this FbxPrefab component requests automatic updates.
        /// </summary>
        public bool WantsAutoUpdate() {
            return m_autoUpdate;
        }

        /// <summary>
        /// Set whether this FbxPrefab component requests automatic updates.
        /// </summary>
        public void SetAutoUpdate(bool autoUpdate) {
            if (!m_autoUpdate && autoUpdate) {
                // We just turned autoupdate on, so update now!
                CompareAndUpdate();
            }
            m_autoUpdate = autoUpdate;
        }

        /// <summary>
        /// Compare the old and new, and update the old according to the rules.
        /// </summary>
        void CompareAndUpdate()
        {
            // If we're not tracking anything, stop updating now.
            // (Typically this is due to a manual update.)
            if (!m_fbxModel) {
                return;
            }

            // todo: only instantiate if there's a change
            var oldRep = GetFbxHistory();
            if (oldRep == null || oldRep.c == null) { oldRep = null; }

            // Instantiate the prefab and compare & update.
            var prefabInstance = UnityEditor.PrefabUtility.InstantiatePrefab(this.gameObject) as GameObject;
            if (!prefabInstance) {
                throw new System.Exception(string.Format("Failed to instantiate {0}; is it really a prefab?",
                            this.gameObject));
            }
            TreeDiff(oldRep, m_fbxModel.transform, prefabInstance.transform);

            // Update the representation of the history to match the new fbx.
            var fbxPrefab = prefabInstance.GetComponent<FbxPrefab>();
            var newFbxRep = FbxRepresentation.FromTransform(m_fbxModel.transform);
            var newFbxRepString = newFbxRep.ToJson();
            fbxPrefab.m_fbxHistory = newFbxRepString;

            // Save the changes back to the prefab.
            UnityEditor.PrefabUtility.ReplacePrefab(prefabInstance.gameObject, this.transform);

            // Destroy the prefabInstance.
            GameObject.DestroyImmediate(prefabInstance.gameObject);
        }

        /// <summary>
        /// Returns the fbx model we're tracking.
        /// </summary>
        public GameObject GetFbxAsset()
        {
            return m_fbxModel;
        }

        /// <summary>
        /// Returns the asset path of the fbx model we're tracking.
        /// </summary>
        public string GetFbxAssetPath()
        {
            if (!m_fbxModel) { return ""; }
            return UnityEditor.AssetDatabase.GetAssetPath(m_fbxModel);
        }

        /// <summary>
        /// Returns the tree representation of the fbx file as it was last time we sync'd.
        /// </summary>
        public FbxRepresentation GetFbxHistory()
        {
            return FbxRepresentation.FromJson(m_fbxHistory);
        }

        /// <summary>
        /// Sync the prefab to match the FBX file.
        /// </summary>
        public void SyncPrefab()
        {
            CompareAndUpdate();
        }

        /// <summary>
        /// Set up the FBX file that this prefab should track.
        ///
        /// Set to null to stop tracking in a way that we can
        /// still restart tracking later.
        /// </summary>
        public void SetSourceModel(GameObject fbxModel) {
            // Null is OK. But otherwise, fbxModel must be an fbx.
            if (fbxModel && !UnityEditor.AssetDatabase.GetAssetPath(fbxModel).EndsWith(".fbx")) {
                throw new System.ArgumentException("FbxPrefab source model must be an fbx asset");
            }

            m_fbxModel = fbxModel;

            // Case 0: fbxModel is null and we have no history
            //          => not normal data flow, but doing nothing is
            //             non-surprising
            // Case 1: fbxModel is null and we have history
            //          => user wants to stop auto-update. Remember history for
            //             when they want to reconnect.
            // Case 2: fbxModel is not null and we have no history
            //          => normal case in ConvertToModel when we just added the
            //             component
            // Case 3: fbxModel is not null and we have history
            //          => normal case when user wants to reconnect or change
            //             the model. Keep the old history and update
            //             immediately.
            if (!m_fbxModel) {
                // Case 0 or 1
                return;
            }

            if (string.IsNullOrEmpty(m_fbxHistory)) {
                // Case 2.
                // This is the first time we've seen the FBX file. Assume that
                // it's the original FBX. Further assume that the user is happy
                // with the prefab as it is now, so don't update it to match the FBX.
                m_fbxHistory = FbxRepresentation.FromTransform(m_fbxModel.transform).ToJson();
            } else {
                // Case 3.
                // User wants to reconnect or change the connection.
                // Update immediately.
                CompareAndUpdate();
            }
        }
#endif
    }
}
