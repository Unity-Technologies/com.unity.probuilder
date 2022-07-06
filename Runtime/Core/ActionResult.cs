using System;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Contains information about the results of a ProBuilder action (success, failure, and other notifications)
    /// </summary>
    public sealed class ActionResult
    {
        /// <summary>
        /// Describes the results of an action.
        /// </summary>
        public enum Status
        {
            /// <summary>
            /// The action was a success.
            /// </summary>
            Success,
            /// <summary>
            /// A critical failure prevented the action from running.
            /// </summary>
            Failure,
            /// <summary>
            /// The action was not completed due to invalid parameters.
            /// </summary>
            Canceled,
            /// <summary>
            /// The action did not run because there was no meaningful action to be made.
            /// </summary>
            NoChange
        }

        /// <summary>
        /// Gets the status of the action following the operation.
        /// </summary>
        public Status status { get; private set; }

        /// <summary>
        /// Gets the short description of the results (a few words long).
        /// </summary>
        public string notification { get; private set; }

        /// <summary>
        /// Creates a new ActionResult with a specific status value.
        /// </summary>
        /// <param name="status">Status value to use for the action.</param>
        /// <param name="notification">A short summary of the action performed.</param>
        public ActionResult(ActionResult.Status status, string notification)
        {
            this.status = status;
            this.notification = notification;
        }

        /// <summary>
        /// Converts the specified result to a boolean value, where true indicates success.
        /// </summary>
        /// <param name="res">ActionResult to convert.</param>
        /// <returns>True if action was <see cref="Status.Success"/>; false otherwise.</returns>
        public static implicit operator bool(ActionResult res)
        {
            return res != null && res.status == Status.Success;
        }

        /// <summary>
        /// Checks whether the current ActionResult is set to <see cref="Status.Success"/> or not.
        /// </summary>
        /// <returns>True if this ActionResult has a status of <see cref="Status.Success"/>; false otherwise.</returns>
        public bool ToBool()
        {
            return status == Status.Success;
        }

        /// <summary>
        /// Returns the value of the specified `success` value.
        /// </summary>
        /// <param name="success">Boolean value to check.</param>
        /// <returns>Generic boolean value corresponding to the success parameter.</returns>
        public static bool FromBool(bool success)
        {
            return success ? ActionResult.Success : new ActionResult(ActionResult.Status.Failure, "Failure");
        }

        /// <summary>
        /// Creates a generic "Success" action result with no notification text.
        /// </summary>
        public static ActionResult Success
        {
            get { return new ActionResult(ActionResult.Status.Success, ""); }
        }

        /// <summary>
        /// Creates a generic "No Selection" action result with "Nothing Selected" notification.
        /// </summary>
        public static ActionResult NoSelection
        {
            get { return new ActionResult(ActionResult.Status.Canceled, "Nothing Selected"); }
        }

        /// <summary>
        /// Creates a generic "Canceled" action result with "User Canceled" notification.
        /// </summary>
        public static ActionResult UserCanceled
        {
            get { return new ActionResult(ActionResult.Status.Canceled, "User Canceled"); }
        }
    }
}
