using UnityEngine;

namespace ProBuilder2.EditorCommon
{
	public enum Status
	{
		Success,
		Failure,
		Canceled
	}

	/**
	 *	Contains information about a ProBuilder action (success, failure, notification, etc)
	 */
	public class pb_ActionResult
	{
		public Status status = Status.Success;

		public string notification = "";

		public pb_ActionResult(Status status, string notification)
		{
			this.status = status;
			this.notification = notification;
		}

		public static pb_ActionResult Success { get { return new pb_ActionResult(Status.Success, ""); } }

		public static pb_ActionResult NoSelection { get {
			return new pb_ActionResult(Status.Canceled, "Nothing Selected");
		} }

		public static pb_ActionResult UserCanceled { get {
			return new pb_ActionResult(Status.Canceled, "User Canceled");
		} }
	}
}
