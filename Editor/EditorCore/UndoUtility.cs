using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [InitializeOnLoad]
    static class UndoUtility
    {
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

            foreach(var mesh in Selection.GetFiltered<ProBuilderMesh>(SelectionMode.TopLevel))
                EditorUtility.SynchronizeWithMeshFilter(mesh);

            ProBuilderEditor.Refresh();
        }

        /**
         * Since Undo calls can potentially hang the main thread, store states when the diff
         * will large.
         */
        public static void RecordSelection(ProBuilderMesh pb, string msg)
        {
            if (pb.vertexCount > 256)
                RegisterCompleteObjectUndo(pb, msg);
            else
                Undo.RecordObject(pb, msg);
        }

        internal static void RecordSelection(string message)
        {
            RecordSelection(MeshSelection.topInternal.ToArray(), message);
        }

        internal static void RecordMeshAndTransformSelection(string message)
        {
            var count = MeshSelection.selectedObjectCount;
            var res = new Object[count * 2];
            var selection = MeshSelection.topInternal;

            for (int i = 0, c = count; i < c; i++)
            {
                res[i] = selection[i];
                res[i + c] = selection[i].transform;
            }

            Undo.RegisterCompleteObjectUndo(res, message);
        }

        /**
         *  Tests if any pb_Object in the selection has more than 512 vertices, and if so records the entire object
         *      instead of diffing the serialized object (which is very slow for large arrays).
         */
        public static void RecordSelection(ProBuilderMesh[] pb, string msg)
        {
            if (pb == null || pb.Length < 1)
                return;

            if (pb.Any(x => { return x.vertexCount > 256; }))
                RegisterCompleteObjectUndo(pb, msg);
            else
                Undo.RecordObjects(pb, msg);
        }

        /**
         * Record an object for Undo.
         */
        public static void RecordObject(Object obj, string msg)
        {
            if (obj is ProBuilderMesh && ((ProBuilderMesh)obj).vertexCount > 256)
            {
#if PB_DEBUG
                Debug.LogWarning("RecordObject()  ->  " + ((pb_Object)obj).vertexCount);
#endif
                RegisterCompleteObjectUndo(obj as ProBuilderMesh, msg);
            }
            else
            {
                Undo.RecordObject(obj, msg);
            }
        }

        /**
         * Record objects for Undo.
         */
        public static void RecordObjects(IEnumerable<Object> objs, string msg)
        {
            if (objs == null)
                return;

            Object[] obj = objs.Where(x => !(x is ProBuilderMesh)).ToArray();
            ProBuilderMesh[] pb = objs.OfType<ProBuilderMesh>().ToArray();

            if (obj.Length > 0)
                Undo.RecordObjects(obj, msg);

            RecordSelection(pb, msg);
        }

        /**
         * Undo.RegisterCompleteObjectUndo
         */
        public static void RegisterCompleteObjectUndo(Object objs, string msg)
        {
            Undo.RegisterCompleteObjectUndo(objs, msg);
        }

        /**
         * Undo.RegisterCompleteObjectUndo
         */
        public static void RegisterCompleteObjectUndo(Object[] objs, string msg)
        {
            Undo.RegisterCompleteObjectUndo(objs, msg);
        }

        /**
         * Record object prior to deletion.
         */
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
