using System;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Classes inheriting MenuAction and tagged with this attribute will be displayed in the ProBuilderEditor window.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ProBuilderMenuActionAttribute : Attribute
	{
	}
}
