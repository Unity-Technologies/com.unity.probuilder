using UnityEngine;
using System.Collections;

/**
 *	\brief Base class for creating and parsing shortcuts within ProBuilder.
 */
namespace ProBuilder2.Common {
public class pb_Shortcut
{

	public pb_Shortcut(string a, string d, KeyCode k, EventModifiers e)
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

	public override string ToString()
	{
		string val = action + "-" + description + "-" + key + "-" + eventModifiers;
		return val; 
	}

	public pb_Shortcut(string str)
	{
		string[] split = str.Split('-');
		// split[0] = action
		// split[1] = description
		KeyCode k = pbUtil.ParseEnum(split[2], KeyCode.None);
		string[] modSplit = split[3].Split(',');
		EventModifiers e = (EventModifiers)0;
		for(int i = 0; i < modSplit.Length; i++)
		{
			e |= pbUtil.ParseEnum(modSplit[i], (EventModifiers)0);
		}
		
		action = split[0];
		description = split[1];
		key = k;
		eventModifiers = e;
		// return new Shortcut(split[0], split[1], k, e);
	}

	public static int IndexOf(pb_Shortcut[] shortcuts, KeyCode k, EventModifiers e)
	{
		for(int i = 0; i < shortcuts.Length; i++)
		{
			if(shortcuts[i].key == k && shortcuts[i].eventModifiers == e)
				return i;
		}
		return -1;
	}

	/**
	 *	\brief Returns a new Shortcut array containing the default values.
	 *	\returns The default Shortcut array.
	 */
	public static pb_Shortcut[] DefaultShortcuts()
	{
		#if !PROTOTYPE
		pb_Shortcut[] shortcuts = new pb_Shortcut[13];
		#else
		pb_Shortcut[] shortcuts = new pb_Shortcut[11];
		#endif

		int i = 0;
		shortcuts[i++] = new pb_Shortcut("Escape", "Top Level", KeyCode.Escape, 0);
/**/	shortcuts[i++] = new pb_Shortcut("Toggle Geometry Mode", "Geometry Level", KeyCode.G, 0);
/**/	shortcuts[i++] = new pb_Shortcut("Texture Window", "Opens the UV editor", KeyCode.J, 0);
/**/	shortcuts[i++] = new pb_Shortcut("Toggle Selection Mode", "Toggle Selection Mode", KeyCode.H, 0);
		shortcuts[i++] = new pb_Shortcut("Set Trigger", "Sets all selected objects to entity type Trigger.", KeyCode.T, 0);
		shortcuts[i++] = new pb_Shortcut("Set Occluder", "Sets all selected objects to entity type Occluder.", KeyCode.O, 0);
		shortcuts[i++] = new pb_Shortcut("Set Collider", "Sets all selected objects to entity type Collider.", KeyCode.C, 0);
		shortcuts[i++] = new pb_Shortcut("Set Mover", "Sets all selected objects to entity type Mover.", KeyCode.M, 0);
		shortcuts[i++] = new pb_Shortcut("Set Detail", "Sets all selected objects to entity type Brush.", KeyCode.B, 0);
		shortcuts[i++] = new pb_Shortcut("Toggle Handle Pivot", "Toggles the orientation of the ProBuilder selection handle.", KeyCode.P, 0);
		shortcuts[i++] = new pb_Shortcut("Set Pivot", "Center pivot around current selection.", KeyCode.J, EventModifiers.Command);
		#if !PROTOTYPE
		shortcuts[i++] = new pb_Shortcut("Quick Apply Nodraw", "When the Texture Window is open, this shortcut will apply the Nodraw material to every selected face.", KeyCode.N, 0);
		shortcuts[i++] = new pb_Shortcut("Delete Face", "Deletes all selected faces.", KeyCode.Backspace, EventModifiers.FunctionKey);
		#endif

		// shortcuts[i++] = new pb_Shortcut("Vertex Mode", "Enter Vertex editing mode.  Automatically swaps to Element level editing.", KeyCode.A, (EventModifiers)0);
		// shortcuts[i++] = new pb_Shortcut("Edge Mode", "Enter Edge editing mode.  Automatically swaps to Element level editing.", KeyCode.S, (EventModifiers)0);
		// shortcuts[i++] = new pb_Shortcut("Face Mode", "Enter Face editing mode.  Automatically swaps to Element level editing.", KeyCode.D, (EventModifiers)0);

		return shortcuts;
	}

	public static pb_Shortcut[] ParseShortcuts(string str)
	{
		pb_Shortcut[] shortcuts;
		
		// Initialize Defaults if no string argument passed, or string ain't right
		if(str == null || str.Length < 3)
		{
			shortcuts = DefaultShortcuts();
		}
		else
		{
			string[] split = str.Split('*');
			shortcuts = new pb_Shortcut[split.Length];

			for(int i = 0; i < shortcuts.Length; i++)
				shortcuts[i] = new pb_Shortcut(split[i]);
		}

		return shortcuts;
	}

	public static string ShortcutsToString(pb_Shortcut[] shortcuts)
	{
		string val = "";
		for(int i = 0; i < shortcuts.Length; i++)
		{
			val += shortcuts[i].ToString();
			if(i!=shortcuts.Length-1)
				val += "*";
		}
		return val;
	}

}
}