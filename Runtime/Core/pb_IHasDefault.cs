using UnityEngine;

namespace ProBuilder.Core
{
	/// <summary>
	/// Interface for objects that contain a set of default values.
	/// Used by generated scriptable objects.
	/// </summary>
	interface pb_IHasDefault
	{
		/// <summary>
		/// Set this object to use default values.
		/// </summary>
		void SetDefaultValues();
	}
}
