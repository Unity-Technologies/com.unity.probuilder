using UnityEngine;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Contains information about a ProBuilder action (success, failure, notification, etc)
	 */
	public class pb_ActionResult
	{
		public enum Status
		{
			Success,
			Failure
		}

		public Status status = Status.Success;

		public string notification = "";

		public pb_ActionResult(Status status, string notification)
		{
			this.status = status;
			this.notification = notification;
		}
	}
}
