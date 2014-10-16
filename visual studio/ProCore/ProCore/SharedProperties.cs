
namespace ProCore.Common
{
    public class SharedProperties
    {
        public static bool snapEnabled = false;
        public static float snapValue = .25f;
        public static bool useAxisConstraints = true;

        /* Allows ProGrids to call 'PushToGrid' events and have ProBuilder pick up on them. */
		public delegate void PushToGridEventHandler(float snapValue);
		public static event PushToGridEventHandler PushToGridEvent;

		public static void PushToGrid(float snapValue)
		{
			if(PushToGridEvent != null)
				PushToGridEvent(snapValue);
		}
    }
}
