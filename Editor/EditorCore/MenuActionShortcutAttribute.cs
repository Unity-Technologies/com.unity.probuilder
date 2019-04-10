using System;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Decorate MenuAction classes to set default ShortcutBinding or override the ShortcutContext (default is global).
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	class MenuActionShortcutAttribute : Attribute
	{
		internal Type context { get; private set; }

		internal KeyCode key;

		internal EventModifiers modifiers;

		public MenuActionShortcutAttribute(KeyCode defaultKeyCode, EventModifiers defaultShorcutModifiers = EventModifiers.None)
			: this(null, defaultKeyCode, defaultShorcutModifiers)
		{
		}

		public MenuActionShortcutAttribute(Type defaultContext)
			: this(defaultContext, KeyCode.None, EventModifiers.None)
		{
		}

		public MenuActionShortcutAttribute(Type defaultContext, KeyCode defaultKeyCode, EventModifiers defaultShorcutModifiers = EventModifiers.None)
		{
			context = defaultContext;
			key = defaultKeyCode;
			modifiers = defaultShorcutModifiers;
		}
	}
}

