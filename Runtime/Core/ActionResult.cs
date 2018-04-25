using UnityEngine;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Describes the results of a pb_Action.
	/// </summary>
	public enum Status
	{
		Success,
		Failure,
		Canceled,
		NoChange
	}

	/// <summary>
	/// Contains information about a ProBuilder action (success, failure, notification, etc)
	/// </summary>
	public class ActionResult
	{
		/// <summary>
		/// State of affairs after the operation.
		/// </summary>
		public Status status { get; private set; }

		/// <summary>
		/// Short description of the results. Should be no longer than a few words.
		/// </summary>
		public string notification { get; private set; }

		/// <summary>
		/// Create a new ActionResult.
		/// </summary>
		/// <param name="status"></param>
		/// <param name="notification"></param>
		public ActionResult(Status status, string notification)
		{
			this.status = status;
			this.notification = notification;
		}

		/// <summary>
		/// Convert a result to a boolean value, true if successful and false if not.
		/// </summary>
		/// <param name="res"></param>
		/// <returns>True if action was successful, false otherwise.</returns>
		public static implicit operator bool(ActionResult res)
		{
			return res != null && res.status == Status.Success;
		}

        public bool ToBool()
        {
            return status == Status.Success;
        }

        public static bool FromBool(bool success)
        {
            return success ? ActionResult.Success : new ActionResult(Status.Failure, "Failure");
        }

        /// <summary>
        /// Generic "Success" action result with no notification text.
        /// </summary>
        public static ActionResult Success { get { return new ActionResult(Status.Success, ""); } }

		/// <summary>
		/// Generic "No Selection" action result with "Nothing Selected" notification.
		/// </summary>
		public static ActionResult NoSelection { get {
			return new ActionResult(Status.Canceled, "Nothing Selected");
		} }

		/// <summary>
		/// Generic "Canceled" action result with "User Canceled" notification.
		/// </summary>
		public static ActionResult UserCanceled { get {
			return new ActionResult(Status.Canceled, "User Canceled");
		} }
	}
}
