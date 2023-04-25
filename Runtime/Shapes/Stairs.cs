using UnityEditor;

namespace UnityEngine.ProBuilder.Shapes
{
    /// <summary>
    /// Describes how ProBuilder will construct the <see cref="Stairs" /> mesh.
    /// </summary>
    enum StepGenerationType
    {
        /// <summary>
        /// Instructs ProBuilder to generate a predictable height for each step in the staircase.
        /// This means that if you increase the height of the overall size of the staircase, the number of steps increases.
        /// </summary>
        Height,
        /// <summary>
        /// ProBuilder to generate a specific number of steps, regardless of any changes in the size of the staircase.
        /// This means that if you increase the height of the overall size of the stairs, each step becomes higher.
        /// </summary>
        Count
    }

    /// <summary>
    /// Represents a basic [stairs](../manual/Stairs.html) shape.
    /// </summary>
    [Shape("Stairs")]
    public class Stairs : Shape
    {
        /// <summary>
        /// Determines whether you want ProBuilder to build the same number of steps regardless of how the size of the stairs
        /// changes (the default) or make each step the same height and automatically adapt the number of steps to match the stairs size.
        ///
        /// The default value is to build the same number of steps.
        /// </summary>
        [SerializeField]
        StepGenerationType m_StepGenerationType = StepGenerationType.Count;

        /// <summary>
        /// Sets the fixed height of each step on the stairs.
        /// The default value is 0.2.
        /// </summary>
        /// <seealso cref="StepGenerationType.Count" />
        [Min(0.01f)]
        [SerializeField]
        float m_StepsHeight = .2f;

        /// <summary>
        /// Sets the fixed number of steps that the stairs always has.
        /// The default value is 10. Valid values range from 1 to 256.
        /// </summary>
        /// <seealso cref="StepGenerationType.Height" />
        [Range(1, 256)]
        [SerializeField]
        int m_StepsCount = 10;

        /// <summary>
        /// Determines whether to force every step to be the exactly the same height. If disabled,
        /// the height of the last step is smaller than the others depending on the remaining height.
        /// This is enabled by default.
        /// </summary>
        /// <seealso cref="StepGenerationType.Height" />
        [SerializeField]
        bool m_HomogeneousSteps = true;

        /// <summary>
        /// Sets the degree of curvature on the stairs in degrees, where 0 makes straight stairs, 360 makes stairs
        /// in a complete circle, and negative angles makes the stairs curve to the left while positive angles make
        /// turns to the right. Remember that you might need to increase the number of stairs to compensate as you
        /// increase this value.
        ///
        /// The default value is 0. Valid values range from -360 to 360.
        /// </summary>
        [Range(-360, 360)]
        [SerializeField]
        float m_Circumference = 0f;

        /// <summary>
        /// Determines whether to draw polygons on the sides of the stairs.
        /// This is enabled by default. You can disable this option if the sides of your stairs
        /// are not visible to the camera (for example, if your stairs are built into a wall).
        /// </summary>
        [SerializeField]
        bool m_Sides = true;

        /// <summary>
        /// Gets or sets whether to draw polygons on the sides of the stairs.
        /// </summary>
        public bool sides
        {
            get => m_Sides;
            set => m_Sides = value;
        }

        [SerializeField, Min(0f)]
        float m_InnerRadius;

        /// <inheritdoc/>
        public override void CopyShape(Shape shape)
        {
            if(shape is Stairs)
            {
                Stairs stairs = (Stairs) shape;
                m_StepGenerationType = stairs.m_StepGenerationType;
                m_StepsHeight = stairs.m_StepsHeight;
                m_StepsCount = stairs.m_StepsCount;
                m_HomogeneousSteps = stairs.m_HomogeneousSteps;
                m_Circumference = stairs.m_Circumference;
                m_Sides = stairs.m_Sides;
                m_InnerRadius = stairs.m_InnerRadius;
            }
        }

        /// <inheritdoc/>
        public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            if (Mathf.Abs(m_Circumference) > 0)
                return BuildCurvedStairs(mesh, size, rotation);
            else
                return BuildStairs(mesh, size, rotation);
        }

        /// <inheritdoc/>
        public override Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
        {
            if (Mathf.Abs(m_Circumference) > 0)
            {
                bounds.center = mesh.mesh.bounds.center;
                bounds.size = Vector3.Scale(Math.Sign(size),mesh.mesh.bounds.size);
            }
            else
            {
                bounds = mesh.mesh.bounds;
                bounds.size = size;
            }

            return bounds;
        }

        Bounds BuildStairs(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            var upDir = Vector3.Scale(rotation * Vector3.up, size) ;
            var rightDir = Vector3.Scale(rotation * Vector3.right, size) ;
            var forwardDir = Vector3.Scale(rotation * Vector3.forward, size) ;

            var meshSize = new Vector3(rightDir.magnitude, upDir.magnitude, forwardDir.magnitude);

            var useStepHeight = m_StepGenerationType == StepGenerationType.Height;
            var stairsHeight = meshSize.y;
            var stepsHeight = Mathf.Min(m_StepsHeight, stairsHeight);

            var steps = m_StepsCount;
            if(useStepHeight)
            {
                if(stairsHeight > 0)
                {
                    steps = (int) ( stairsHeight / stepsHeight );
                    if(m_HomogeneousSteps)
                        stepsHeight = stairsHeight / steps;
                    else
                        steps += ( ( stairsHeight / stepsHeight ) - steps ) > 0.001f ? 1 : 0;
                }
                else
                    steps = 1;
            }

            //Clamping max steps number
            if(steps > 256)
            {
                steps = 256;
                stepsHeight = stairsHeight / steps;
            }

            // 4 vertices per quad, 2 quads per step.
            var vertices = new Vector3[4 * steps * 2];
            var faces = new Face[steps * 2];
            Vector3 extents = meshSize * .5f;

            // vertex index, face index
            int v = 0, t = 0;

            float heightInc0, heightInc1, inc0, inc1;
            float x0, x1, y0, y1, z0, z1;
            for (int i = 0; i < steps; i++)
            {
                heightInc0 = i * stepsHeight;
                heightInc1 = i != steps -1 ? (i + 1) * stepsHeight : meshSize.y;
                inc0 = i / (float)steps;
                inc1 = (i + 1) / (float)steps;

                x0 = meshSize.x - extents.x;
                x1 = 0 - extents.x;
                y0 = (useStepHeight ? heightInc0 : meshSize.y * inc0) - extents.y;
                y1 = (useStepHeight ? heightInc1 : meshSize.y * inc1) - extents.y;
                z0 = meshSize.z * inc0 - extents.z;
                z1 = meshSize.z * inc1 - extents.z;

                vertices[v + 0] = new Vector3(x0, y0, z0);
                vertices[v + 1] = new Vector3(x1, y0, z0);
                vertices[v + 2] = new Vector3(x0, y1, z0);
                vertices[v + 3] = new Vector3(x1, y1, z0);

                vertices[v + 4] = new Vector3(x0, y1, z0);
                vertices[v + 5] = new Vector3(x1, y1, z0);
                vertices[v + 6] = new Vector3(x0, y1, z1);
                vertices[v + 7] = new Vector3(x1, y1, z1);

                faces[t + 0] = new Face(new int[] {  v + 0,
                                                     v + 1,
                                                     v + 2,
                                                     v + 1,
                                                     v + 3,
                                                     v + 2 });

                faces[t + 1] = new Face(new int[] {  v + 4,
                                                     v + 5,
                                                     v + 6,
                                                     v + 5,
                                                     v + 7,
                                                     v + 6 });

                v += 8;
                t += 2;
            }

            // sides
            if (sides)
            {
                // first step is special case - only needs a quad, but all other steps need
                // a quad and tri.
                float x = 0f;

                for (int side = 0; side < 2; side++)
                {
                    Vector3[] sides_v = new Vector3[steps * 4 + (steps - 1) * 3];
                    Face[] sides_f = new Face[steps + steps - 1];

                    int sv = 0, st = 0;

                    for (int i = 0; i < steps; i++)
                    {
                        heightInc0 = Mathf.Max(i, 1) * stepsHeight;
                        heightInc1 = i != steps-1 ? (i + 1) * stepsHeight : meshSize.y;
                        inc0 = Mathf.Max(i, 1) / (float)steps;
                        inc1 = (i + 1) / (float)steps;

                        y0 = useStepHeight ? heightInc0 : inc0 * meshSize.y;
                        y1 = useStepHeight ? heightInc1 : inc1 * meshSize.y;

                        inc0 = i / (float)steps;

                        z0 = inc0 * meshSize.z;
                        z1 = inc1 * meshSize.z;

                        sides_v[sv + 0] = new Vector3(x, 0f, z0) - extents;
                        sides_v[sv + 1] = new Vector3(x, 0f, z1) - extents;
                        sides_v[sv + 2] = new Vector3(x, y0, z0) - extents;
                        sides_v[sv + 3] = new Vector3(x, y1, z1) - extents;

                        sides_f[st++] = new Face(side % 2 == 0 ?
                                new int[] { v + 0, v + 1, v + 2, v + 1, v + 3, v + 2 } :
                                new int[] { v + 2, v + 1, v + 0, v + 2, v + 3, v + 1 });

                        sides_f[st - 1].textureGroup = side + 1;

                        v += 4;
                        sv += 4;

                        // that connecting triangle
                        if (i > 0)
                        {
                            sides_v[sv + 0] = new Vector3(x, y0, z0) - extents;
                            sides_v[sv + 1] = new Vector3(x, y1, z0) - extents;
                            sides_v[sv + 2] = new Vector3(x, y1, z1) - extents;

                            sides_f[st++] = new Face(side % 2 == 0 ?
                                    new int[] { v + 2, v + 1, v + 0 } :
                                    new int[] { v + 0, v + 1, v + 2 });

                            sides_f[st - 1].textureGroup = side + 1;

                            v += 3;
                            sv += 3;
                        }
                    }

                    vertices = vertices.Concat(sides_v);
                    faces = faces.Concat(sides_f);

                    x += meshSize.x;
                }

                // add that last back face
                vertices = vertices.Concat(new Vector3[] {
                    new Vector3(0f, 0f, meshSize.z) - extents,
                    new Vector3(meshSize.x, 0f, meshSize.z) - extents,
                    new Vector3(0f, meshSize.y, meshSize.z) - extents,
                    new Vector3(meshSize.x, meshSize.y, meshSize.z) - extents
                });

                faces = faces.Add(new Face(new int[] { v + 0, v + 1, v + 2, v + 1, v + 3, v + 2 }));
            }

            var sizeSigns = Math.Sign(size);
            for(int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = rotation * vertices[i];
                vertices[i].Scale(sizeSigns);
            }

            var sizeSign = sizeSigns.x * sizeSigns.y * sizeSigns.z;
            if(sizeSign < 0)
            {
                foreach(var face in faces)
                    face.Reverse();
            }

            mesh.RebuildWithPositionsAndFaces(vertices, faces);

            return UpdateBounds(mesh, size, rotation, new Bounds());
        }

        Bounds BuildCurvedStairs(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
        {
            var meshSize = Math.Abs(size);

            var buildSides = m_Sides;
            var maxWidth = Mathf.Min(meshSize.x, meshSize.z);
            var innerRadius = Mathf.Clamp(m_InnerRadius, 0f, maxWidth - float.Epsilon);
            var stairWidth = maxWidth - innerRadius;
            var height = Mathf.Abs(meshSize.y);
            var circumference = m_Circumference;
            bool noInnerSide = innerRadius < Mathf.Epsilon;
            bool useStepHeight = m_StepGenerationType == StepGenerationType.Height;

            var stepsHeight = Mathf.Min(m_StepsHeight, height);
            var steps = m_StepsCount;
            if(useStepHeight && stepsHeight > 0.01f * m_StepsHeight)
            {
                if(height > 0)
                {
                    steps = (int) ( height / m_StepsHeight );
                    if(m_HomogeneousSteps && steps > 0)
                        stepsHeight = height / steps;
                    else
                        steps += ( ( height / m_StepsHeight ) - steps ) > 0.001f ? 1 : 0;
                }
                else
                    steps = 1;
            }

            //Clamping max steps number
            if(steps > 256)
            {
                steps = 256;
                stepsHeight = height / steps;
            }

            // 4 vertices per quad, vertical step first, then floor step can be 3 or 4 verts depending on
            // if the inner radius is 0 or not.
            Vector3[] positions = new Vector3[(4 * steps) + ((noInnerSide ? 3 : 4) * steps)];
            Face[] faces = new Face[steps * 2];

            // vertex index, face index
            int v = 0, t = 0;

            float cir = Mathf.Abs(circumference) * Mathf.Deg2Rad;
            float outerRadius = innerRadius + stairWidth;

            for (int i = 0; i < steps; i++)
            {
                float inc0 = (i / (float)steps) * cir;
                float inc1 = ((i + 1) / (float)steps) * cir;

                float h0 = useStepHeight ? i * stepsHeight : ((i / (float)steps) * height);
                float h1 = useStepHeight ? ((i != steps-1) ? ((i+1) * stepsHeight) : height) :( ((i + 1) / (float)steps) * height );

                Vector3 v0 = new Vector3(-Mathf.Cos(inc0), 0f, Mathf.Sin(inc0));
                Vector3 v1 = new Vector3(-Mathf.Cos(inc1), 0f, Mathf.Sin(inc1));

                /*
                 *
                 *      /6-----/7
                 *     /      /
                 *    /5_____/4
                 *    |3     |2
                 *    |      |
                 *    |1_____|0
                 *
                 */

                positions[v + 0] = v0 * innerRadius;
                positions[v + 1] = v0 * outerRadius;
                positions[v + 2] = v0 * innerRadius;
                positions[v + 3] = v0 * outerRadius;

                positions[v + 0].y = h0;
                positions[v + 1].y = h0;
                positions[v + 2].y = h1;
                positions[v + 3].y = h1;

                positions[v + 4] = positions[v + 2];
                positions[v + 5] = positions[v + 3];

                positions[v + 6] = v1 * outerRadius;
                positions[v + 6].y = h1;

                if (!noInnerSide)
                {
                    positions[v + 7] = v1 * innerRadius;
                    positions[v + 7].y = h1;
                }

                faces[t + 0] = new Face(new int[] {
                    v + 0,
                    v + 1,
                    v + 2,
                    v + 1,
                    v + 3,
                    v + 2
                });

                if (noInnerSide)
                {
                    faces[t + 1] = new Face(new int[] {
                        v + 4,
                        v + 5,
                        v + 6
                    });
                }
                else
                {
                    faces[t + 1] = new Face(new int[] {
                        v + 4,
                        v + 5,
                        v + 6,
                        v + 4,
                        v + 6,
                        v + 7
                    });
                }

                float uvRotation = ((inc1 + inc0) * -.5f) * Mathf.Rad2Deg;
                uvRotation %= 360f;
                if (uvRotation < 0f)
                    uvRotation = 360f + uvRotation;

                var uv = faces[t + 1].uv;
                uv.rotation = uvRotation;
                faces[t + 1].uv = uv;

                v += noInnerSide ? 7 : 8;
                t += 2;
            }

            // sides
            if (buildSides)
            {
                // first step is special case - only needs a quad, but all other steps need
                // a quad and tri.
                float x = noInnerSide ? innerRadius + stairWidth : innerRadius;

                for (int side = (noInnerSide ? 1 : 0); side < 2; side++)
                {
                    Vector3[] sides_v = new Vector3[steps * 4 + (steps - 1) * 3];
                    Face[] sides_f = new Face[steps + steps - 1];

                    int sv = 0, st = 0;

                    for (int i = 0; i < steps; i++)
                    {
                        float inc0 = (i / (float)steps) * cir;
                        float inc1 = ((i + 1) / (float)steps) * cir;

                        float h0 = useStepHeight ? Mathf.Max(i, 1) * stepsHeight : ((Mathf.Max(i, 1) / (float)steps) * height);
                        float h1 = useStepHeight ? (i != steps-1 ? (i + 1) * stepsHeight : meshSize.y) : (((i + 1) / (float)steps) * height);

                        Vector3 v0 = new Vector3(-Mathf.Cos(inc0), 0f, Mathf.Sin(inc0)) * x;
                        Vector3 v1 = new Vector3(-Mathf.Cos(inc1), 0f, Mathf.Sin(inc1)) * x;

                        sides_v[sv + 0] = v0;
                        sides_v[sv + 1] = v1;
                        sides_v[sv + 2] = v0;
                        sides_v[sv + 3] = v1;

                        sides_v[sv + 0].y = 0f;
                        sides_v[sv + 1].y = 0f;
                        sides_v[sv + 2].y = h0;
                        sides_v[sv + 3].y = h1;

                        sides_f[st++] = new Face(side % 2 == 0 ?
                                new int[] { v + 2, v + 1, v + 0, v + 2, v + 3, v + 1 } :
                                new int[] { v + 0, v + 1, v + 2, v + 1, v + 3, v + 2 });
                        sides_f[st - 1].smoothingGroup = side + 1;

                        v += 4;
                        sv += 4;

                        // that connecting triangle
                        if (i > 0)
                        {
                            sides_f[st - 1].textureGroup = (side * steps) + i;

                            sides_v[sv + 0] = v0;
                            sides_v[sv + 1] = v1;
                            sides_v[sv + 2] = v0;
                            sides_v[sv + 0].y = h0;
                            sides_v[sv + 1].y = h1;
                            sides_v[sv + 2].y = h1;

                            sides_f[st++] = new Face(side % 2 == 0 ?
                                    new int[] { v + 2, v + 1, v + 0 } :
                                    new int[] { v + 0, v + 1, v + 2 });

                            sides_f[st - 1].textureGroup = (side * steps) + i;
                            sides_f[st - 1].smoothingGroup = side + 1;

                            v += 3;
                            sv += 3;
                        }
                    }

                    positions = positions.Concat(sides_v);
                    faces = faces.Concat(sides_f);

                    x += stairWidth;
                }

                // // add that last back face
                float cos = -Mathf.Cos(cir), sin = Mathf.Sin(cir);

                positions = positions.Concat(new Vector3[]
                {
                    new Vector3(cos, 0f, sin) * innerRadius,
                    new Vector3(cos, 0f, sin) * outerRadius,
                    new Vector3(cos * innerRadius, height, sin * innerRadius),
                    new Vector3(cos * outerRadius, height, sin * outerRadius)
                });

                faces = faces.Add(new Face(new int[] { v + 2, v + 1, v + 0, v + 2, v + 3, v + 1 }));
            }

            if (circumference < 0f)
            {
                Vector3 flip = new Vector3(-1f, 1f, 1f);

                for (int i = 0; i < positions.Length; i++)
                    positions[i].Scale(flip);

                foreach (Face f in faces)
                    f.Reverse();
            }

            var sizeSigns = Math.Sign(size);
            for(int i = 0; i < positions.Length; i++)
            {
                positions[i] = rotation * positions[i];
                positions[i].Scale(sizeSigns);
            }

            var sizeSign = sizeSigns.x * sizeSigns.y * sizeSigns.z;
            if(sizeSign < 0)
            {
                foreach(var face in faces)
                    face.Reverse();
            }

            mesh.RebuildWithPositionsAndFaces(positions, faces);

            mesh.TranslateVerticesInWorldSpace(mesh.mesh.triangles, mesh.transform.TransformDirection(-mesh.mesh.bounds.center));
            mesh.Refresh();

            return UpdateBounds(mesh, size, rotation, new Bounds());
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Stairs))]
    public class StairsDrawer : PropertyDrawer
    {
        static bool s_foldoutEnabled = true;

        const bool k_ToggleOnLabelClick = true;

        static readonly GUIContent k_StepGenerationContent = new GUIContent("Steps Generation", L10n.Tr("Whether to generate steps using the number of steps or by step height."));
        static readonly GUIContent k_StepsCountContent = new GUIContent("Steps Count", L10n.Tr("Number of steps of the stair."));
        static readonly GUIContent k_StepsHeightContent = new GUIContent("Steps Height", L10n.Tr("Height of each step of the generated stairs."));
        static readonly GUIContent k_HomogeneousStepsContent = new GUIContent("Homogeneous Steps", L10n.Tr("Whether to round the step height to create homogenous steps."));
        static readonly GUIContent k_CircumferenceContent = new GUIContent("Circumference", L10n.Tr("Circumference of the stairs. Use a negative number to rotate in the opposite direction."));
        static readonly GUIContent k_SidesContent = new GUIContent("Sides", L10n.Tr("Whether to generate sides."));
        static readonly GUIContent k_InnerRadius = new GUIContent("Inner Radius", L10n.Tr("In a curved stair-set, this defines the radius from center to the inner edge of the stair."));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            s_foldoutEnabled = EditorGUI.Foldout(position, s_foldoutEnabled, "Stairs Settings", k_ToggleOnLabelClick);

            EditorGUI.indentLevel++;

            if(s_foldoutEnabled)
            {
                var typeProperty = property.FindPropertyRelative("m_StepGenerationType");
                StepGenerationType typeEnum = (StepGenerationType)(typeProperty.intValue);
                EditorGUI.BeginChangeCheck();
                typeEnum = (StepGenerationType)EditorGUILayout.EnumPopup(k_StepGenerationContent, typeEnum);
                if(EditorGUI.EndChangeCheck())
                    typeProperty.intValue = (int)typeEnum;

                if(typeEnum == StepGenerationType.Count)
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("m_StepsCount"), k_StepsCountContent);
                else
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("m_StepsHeight"), k_StepsHeightContent);
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("m_HomogeneousSteps"), k_HomogeneousStepsContent);
                }

                var circumference = property.FindPropertyRelative("m_Circumference");
                var innerRadius = property.FindPropertyRelative("m_InnerRadius");
                EditorGUILayout.PropertyField(circumference, k_CircumferenceContent);
                EditorGUI.BeginDisabledGroup(Mathf.Abs(circumference.floatValue) < float.Epsilon);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(innerRadius, k_InnerRadius);
                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Sides"), k_SidesContent);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
#endif
}
