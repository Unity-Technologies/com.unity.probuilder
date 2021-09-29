using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [InitializeOnLoad]
    static class UndoUtility
    {
        static List<Object> s_UndoBuffer = new List<Object>();
        
        static UndoUtility()
        {
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        static void UndoRedoPerformed()
        {
            // material preview when dragging in scene-view is done by applying then undoing changes. we don't want to
            // rebuild the mesh every single frame when dragging.
            if (SceneDragAndDropListener.isDragging)
                return;

            // Two passes
            // 1. Ensure every ProBuilderMesh in the scene has a valid mesh
            // 2. Rebuild every ProBuilderMesh in the selection to reflect undone changes.

            // Synchronize just checks that the mesh is not null, and UV2 is still valid. This should be very cheap except
            // for the FindObjectsOfType call.
            foreach (var mesh in Object.FindObjectsOfType<ProBuilderMesh>())
            {
                EditorUtility.SynchronizeWithMeshFilter(mesh);
                mesh.InvalidateCaches();
            }

            foreach (var mesh in InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms))
            {
                mesh.InvalidateCaches();

                using (new ProBuilderMesh.NonVersionedEditScope(mesh))
                {
                    mesh.Rebuild();
                    mesh.Optimize();
                }
            }

            ProBuilderEditor.Refresh();
        }

        // todo selection should be a separate object from ProBuilderMesh
        public static void RecordSelection(string msg) => RecordSelection(MeshSelection.topInternal, msg);

        public static void RecordSelection<T>(T obj, string msg) where T : Object
            => RecordSelection(new[] { obj }, msg);
        
        public static void RecordSelection<T>(IEnumerable<T> objs, string msg) where T : Object 
            => RecordObjects(objs, msg);

        public static void RecordObject(Object obj, string msg) 
            => RecordObjects(new[] { obj }, msg);

        static bool CollectUndoTargets<T>(IEnumerable<T> objs, out Object[] targets) where T : Object
        {
            foreach (var obj in objs)
            {
                if (obj is ProBuilderMesh mesh)
                    s_UndoBuffer.Add(mesh.pmesh);
                s_UndoBuffer.Add(obj);
            }

            targets = s_UndoBuffer.ToArray();
            s_UndoBuffer.Clear();
            return targets.Length > 0;
        }

        // Convenience method handles registering ProBuilderMesh + PMesh object
        // todo Rename this to something more indicative of function
        public static void RecordObjects(IEnumerable<Object> objs, string msg)
        {
            if(CollectUndoTargets(objs, out var targets))
                Undo.RecordObjects(targets, msg);
        }

        public static void RegisterCompleteObjectUndo(Object obj, string msg)
            => RegisterCompleteObjectUndo(new[] { obj }, msg);
        
        public static void RegisterCompleteObjectUndo(IEnumerable<Object> objs, string msg)
        {
            if(CollectUndoTargets(objs, out var targets))
                Undo.RegisterCompleteObjectUndo(targets, msg);
        }

        public static void DestroyImmediate(Object obj)
        {
            Undo.DestroyObjectImmediate(obj);
        }

        public static void RegisterCreatedObjectUndo(Object obj, string msg)
        {
            Undo.RegisterCreatedObjectUndo(obj, msg);
        }

        public static void RecordComponents<T0, T1>(IEnumerable<Component> objs, string message)
            where T0 : Component
            where T1 : Component
        {
            List<Object> targets = new List<Object>();

            foreach (var o in objs)
            {
                var t = o.GetComponent<T0>();
                var k = o.GetComponent<T1>();

                if (t != null)
                    targets.Add(t);

                if (k != null)
                    targets.Add(k);
            }

            RecordObjects(targets, message);
        }

        public static void RecordComponents<T0, T1, T2>(IEnumerable<Component> objs, string message)
            where T0 : Component
            where T1 : Component
            where T2 : Component
        {
            List<Object> targets = new List<Object>();

            foreach (var o in objs)
            {
                var t0 = o.GetComponent<T0>();
                var t1 = o.GetComponent<T1>();
                var t2 = o.GetComponent<T2>();

                if (t0 != null)
                    targets.Add(t0);

                if (t1 != null)
                    targets.Add(t1);

                if (t2 != null)
                    targets.Add(t2);
            }

            RecordObjects(targets, message);
        }
    }
}
