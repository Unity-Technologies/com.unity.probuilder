using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(Entity))]
    [CanEditMultipleObjects]
    sealed class EntityEditor : Editor
    {
        Entity ent;
        ProBuilderMesh pb;

        public enum ColType
        {
            MeshCollider,
            BoxCollider,
            SphereCollider
        }

        public void OnEnable()
        {
            ent = (Entity)target;

            if (ent != null)
                pb = (ProBuilderMesh)ent.transform.GetComponent<ProBuilderMesh>();
        }

        public override void OnInspectorGUI()
        {
            if (pb == null) return;
            if (ent == null) return;

            EntityType et = ent.entityType;
            et = (EntityType)EditorGUILayout.EnumPopup("Entity Type", et);
            if (et != ent.entityType)
            {
                UndoUtility.RecordObjects(new Object[] {ent, ent.gameObject.GetComponent<ProBuilderMesh>() }, "Set Entity Type");
#pragma warning disable 0618
                EntityUtility.SetEntityType(et, ent.gameObject);
#pragma warning restore 0618
                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();
            }

            GUILayout.Space(4);

            pb.userCollisions = EditorGUILayout.Toggle("Custom Collider", pb.userCollisions);

            // Convience
            if (pb.userCollisions)
                GUI.enabled = false;

            GUILayout.Label("Add Collider", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Mesh Collider", EditorStyles.miniButtonLeft))
                EditorApplication.delayCall += AddMeshCollider;

            if (GUILayout.Button("Box Collider", EditorStyles.miniButtonMid))
                EditorApplication.delayCall += AddBoxCollider;

            if (GUILayout.Button("Remove Collider", EditorStyles.miniButtonRight))
                EditorApplication.delayCall += RemoveColliders;


            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        void AddMeshCollider()
        {
            AddCollider(ColType.MeshCollider);
        }

        void AddBoxCollider()
        {
            AddCollider(ColType.BoxCollider);
        }

        private void AddCollider(ColType c)
        {
            Collider[] colliders = serializedObject.targetObjects.Where(x => x is Entity).SelectMany(x => ((Entity)x).gameObject.GetComponents<Collider>()).ToArray();
            bool isTrigger = false;
            if (colliders != null)
                isTrigger = colliders.Any(x => x.isTrigger);

            RemoveColliders();

            foreach (Entity obj in serializedObject.targetObjects)
            {
                GameObject go = obj.gameObject;

                switch (c)
                {
                    case ColType.MeshCollider:
                    {
                        MeshCollider col = go.AddComponent<MeshCollider>();

                        if (ent.entityType == EntityType.Trigger)
                        {
                            col.convex = true;
                            col.isTrigger = true;
                        }
                        else if (ent.entityType == EntityType.Collider)
                        {
                            col.convex = true;
                        }
                        else if (isTrigger)
                        {
                            col.convex = true;
                            col.isTrigger = true;
                        }

                        break;
                    }

                    case ColType.BoxCollider:
                    {
                        BoxCollider col = go.AddComponent<BoxCollider>();

                        if (ent.entityType == EntityType.Trigger || isTrigger)
                            col.isTrigger = true;
                        break;
                    }

                    case ColType.SphereCollider:
                    {
                        SphereCollider col = go.AddComponent<SphereCollider>();
                        if (ent.entityType == EntityType.Trigger || isTrigger)
                            col.isTrigger = true;
                        break;
                    }

                    default:
                        break;
                }
            }
        }

        private void RemoveColliders()
        {
            foreach (Entity obj in serializedObject.targetObjects)
            {
                foreach (Collider c in obj.gameObject.GetComponents<Collider>())
                    DestroyImmediate(c);
            }
        }
    }
}
