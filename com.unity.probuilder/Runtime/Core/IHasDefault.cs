using UnityEngine;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Interface for objects that contain a set of default values. Used by generated scriptable objects.
    /// </summary>
    interface IHasDefault
    {
        /// <summary>
        /// Set this object to use default values.
        /// </summary>
        void SetDefaultValues();
    }
}
