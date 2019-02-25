#if UNITY_2019_1_OR_NEWER
#define SHORTCUT_MANAGER
#endif

#if !SHORTCUT_MANAGER
using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// ProBuilder shortcuts.
    /// </summary>
#if SHORTCUT_MANAGER
    [System.Obsolete]
#endif
    sealed class Shortcut
    {
        public Shortcut(string a, string d, KeyCode k, EventModifiers e)
        {
            action = a;
            description = d;
            key = k;
            eventModifiers = e;
        }

        public string action;
        public string description;
        public KeyCode key;
        public EventModifiers eventModifiers;

        public Shortcut(string str)
        {
            try
            {
                string[] split = str.Split('-');

                action = split[0];
                description = split[1];

                int t;

                if (int.TryParse(split[2], out t))
                    key = (KeyCode)t;

                if (int.TryParse(split[3], out t))
                    eventModifiers = (EventModifiers)t;
            }
            catch
            {
                Debug.LogWarning("Failed parsing shortcut: " + str);
            }
        }

        public bool Matches(KeyCode key, EventModifiers modifiers)
        {
            return this.key == key && this.eventModifiers == modifiers;
        }

        public static int IndexOf(Shortcut[] shortcuts, KeyCode k, EventModifiers e)
        {
            for (int i = 0; i < shortcuts.Length; i++)
            {
                if (shortcuts[i].key == k && shortcuts[i].eventModifiers == e)
                    return i;
            }
            return -1;
        }

        /**
         *  \brief Returns a new Shortcut array containing the default values.
         *  \returns The default Shortcut array.
         */
        public static IEnumerable<Shortcut> DefaultShortcuts()
        {
            List<Shortcut> shortcuts = new List<Shortcut>();

            shortcuts.Add(new Shortcut("Escape", "Top Level", KeyCode.Escape, 0));
            shortcuts.Add(new Shortcut("Toggle Geometry Mode", "Geometry Level", KeyCode.G, 0));
            shortcuts.Add(new Shortcut("Toggle Selection Mode", "Toggle Selection Mode.  If Toggle Mode Shortcuts is disabled, this shortcut does not apply.", KeyCode.H, 0));
            shortcuts.Add(new Shortcut("Set Trigger", "Sets all selected objects to entity type Trigger.", KeyCode.T, 0));
            shortcuts.Add(new Shortcut("Set Occluder", "Sets all selected objects to entity type Occluder.", KeyCode.O, 0));
            shortcuts.Add(new Shortcut("Set Collider", "Sets all selected objects to entity type Collider.", KeyCode.C, 0));
            shortcuts.Add(new Shortcut("Set Mover", "Sets all selected objects to entity type Mover.", KeyCode.M, 0));
            shortcuts.Add(new Shortcut("Set Detail", "Sets all selected objects to entity type Brush.", KeyCode.B, 0));
            shortcuts.Add(new Shortcut("Toggle Handle Orientation", "Toggles the orientation of the ProBuilder selection handle.", KeyCode.P, 0));
            shortcuts.Add(new Shortcut("Set Pivot", "Center pivot around current selection.", KeyCode.J, EventModifiers.Command));
            shortcuts.Add(new Shortcut("Delete Face", "Deletes all selected faces.", KeyCode.Backspace, EventModifiers.FunctionKey));
            shortcuts.Add(new Shortcut("Vertex Mode", "Enter Vertex editing mode.  Automatically swaps to Element level editing.", KeyCode.H, (EventModifiers)0));
            shortcuts.Add(new Shortcut("Edge Mode", "Enter Edge editing mode.  Automatically swaps to Element level editing.", KeyCode.J, (EventModifiers)0));
            shortcuts.Add(new Shortcut("Face Mode", "Enter Face editing mode.  Automatically swaps to Element level editing.", KeyCode.K, (EventModifiers)0));

            return shortcuts;
        }

        public static IEnumerable<Shortcut> ParseShortcuts(string str)
        {
            // Initialize Defaults if no string argument passed, or string ain't right
            if (str == null || str.Length < 3)
                return DefaultShortcuts();

            string[] split = str.Split('*');
            Shortcut[] shortcuts = new Shortcut[split.Length];

            for (int i = 0; i < shortcuts.Length; i++)
                shortcuts[i] = new Shortcut(split[i]);

            return shortcuts;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}, {2} ({3})", action, key.ToString(), eventModifiers.ToString(), (int)eventModifiers);
        }

        public string Serialize()
        {
            // lazy sanitize action and description action, description, and key
            action = action.Replace("-", " ").Replace("*", "");
            description = description.Replace("-", " ").Replace("*", "");
            string val = action + "-" + description + "-" + (int)key + "-" + (int)eventModifiers;
            return val;
        }

        public static string ShortcutsToString(Shortcut[] shortcuts)
        {
            string val = "";

            for (int i = 0; i < shortcuts.Length; i++)
            {
                val += shortcuts[i].Serialize();

                if (i != shortcuts.Length - 1)
                    val += "*";
            }
            return val;
        }
    }
}
#endif
