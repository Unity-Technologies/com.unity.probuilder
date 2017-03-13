using UnityEngine;
using System;

namespace ProBuilder2.Common
{
	/**
	 * ProGridsNoSnapAttribute tells ProGrids to skip snapping on this object.
	 */
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class ProGridsNoSnapAttribute : Attribute
	{
	}

	/**
	 * ProGridsConditionalSnapAttribute tells ProGrids to check IsSnapEnabled function on this object.
	 */
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class ProGridsConditionalSnapAttribute : Attribute
	{
	}
}
