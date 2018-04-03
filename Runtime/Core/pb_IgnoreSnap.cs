using UnityEngine;
using System;

namespace ProBuilder.Core
{
	/// <summary>
	/// ProGridsNoSnapAttribute tells ProGrids to skip snapping on this object.
	/// </summary>
	/// <remarks>
	/// This exists only as a stub for the ProGrids defined attribute.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	class ProGridsNoSnapAttribute : Attribute
	{
	}

	/// <summary>
	/// ProGridsConditionalSnapAttribute tells ProGrids to check IsSnapEnabled function on this object.
	/// </summary>
	/// <remarks>
	/// This exists only as a stub for the ProGrids defined attribute.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	class ProGridsConditionalSnapAttribute : Attribute
	{
	}
}
