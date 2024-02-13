
using System;
using System.Linq;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class EditorPathSelectionUtility
    {
        const string k_ShortcutId = "ProBuilder/Selection/Select Path";

        [InitializeOnLoadMethod]
        static void Init()
        {
            ShortcutManager.instance.shortcutBindingChanged += ValidateShortcutBinding;
        }

        static void ValidateShortcutBinding(ShortcutBindingChangedEventArgs args)
        {
            if (args.shortcutId.Equals(k_ShortcutId))
            {
                var newCombination = args.newBinding.keyCombinationSequence.First();
                if(newCombination.modifiers.Equals(ShortcutModifiers.None))
                    Debug.LogWarning("The shortcut 'ProBuilder/Selection/Select Path' must have at least one modifier, otherwise path selection will not work.");
            }
        }

        public static bool IsSelectionPathModifier(EventModifiers em)
        {
            var binding = ShortcutManager.instance.GetShortcutBinding(k_ShortcutId);
            var combination = binding.keyCombinationSequence.First();

            //At least one modifier is required for the path selection
            if (combination.modifiers.Equals(ShortcutModifiers.None))
                return false;

            // Check if all desired modifiers are pressed
            if (combination.modifiers.HasFlag(ShortcutModifiers.Shift) && (em & EventModifiers.Shift) != EventModifiers.Shift)
                return false;
#if UNITY_EDITOR_OSX
            if (combination.modifiers.HasFlag(ShortcutModifiers.Control) && (em & EventModifiers.Control) != EventModifiers.Control)
                return false;
            if (combination.modifiers.HasFlag(ShortcutModifiers.Action) && (em & EventModifiers.Command) != EventModifiers.Command)
                return false;
#else
            if ((combination.modifiers.HasFlag(ShortcutModifiers.Control) || combination.modifiers.HasFlag(ShortcutModifiers.Action))
                && (em & EventModifiers.Control) != EventModifiers.Control)
                return false;
#endif
            if (combination.modifiers.HasFlag(ShortcutModifiers.Alt) && (em & EventModifiers.Alt) != EventModifiers.Alt)
                return false;

            return true;
        }

        [Shortcut("ProBuilder/Selection/Select Path", typeof(PositionToolContext.ProBuilderShortcutContext), KeyCode.Mouse0, ShortcutModifiers.Shift | ShortcutModifiers.Action)]
        static void DoPathSelection(ShortcutArguments args)
        {
            var mesh = EditorSceneViewPicker.selection?.mesh;
            if (mesh == null || ProBuilderEditor.selectMode != SelectMode.Face)
                return;

            var activeFace = mesh.GetActiveFace();
            if (activeFace != null)
            {
                var faces = mesh.facesInternal;
                var face = ProBuilderEditor.instance.hovering.faces[0];

                UndoUtility.RecordSelection(mesh, "Select Face");
                var pathFaces = SelectPathFaces.GetPath(mesh, Array.IndexOf<Face>(faces, activeFace), Array.IndexOf<Face>(faces, face));
                if (pathFaces != null)
                {
                    foreach (var pathFace in pathFaces)
                        mesh.AddToFaceSelection(pathFace);

                    Event.current.Use();
                    ProBuilderEditor.instance.ResetSceneGUIEvent();
                }
            }
        }
    }
}
