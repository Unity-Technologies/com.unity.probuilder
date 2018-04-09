#define PB_PROGRIDS_ENABLED

#if PB_PROGRIDS_ENABLED

using System.Reflection;
using NUnit.Framework;
using ProBuilder.EditorCore;

namespace ProBuilder.EditorTests.ProGrids
{
	public class ProGridsMethodsExist
	{
		const BindingFlags k_BindingFlagsAll =
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		[Test]
		public static void GetProGridsType()
		{
			Assert.IsNotNull(pb_ProGridsInterface.GetProGridsType());
		}

		[Test]
		public static void ProGridsActive()
		{
			Assert.IsNotNull(pb_ProGridsInterface.GetProGridsType().GetMethod("SceneToolbarActive", k_BindingFlagsAll));
		}

		[Test]
		public static void SceneToolbarIsExtended()
		{
			Assert.IsNotNull(pb_ProGridsInterface.GetProGridsType().GetMethod("SceneToolbarIsExtended", k_BindingFlagsAll));
		}

		[Test]
		public static void UseAxisConstraints()
		{
			Assert.IsNotNull(pb_ProGridsInterface.GetProGridsType().GetMethod("UseAxisConstraints", k_BindingFlagsAll));
		}

		[Test]
		public static void SnapEnabled()
		{
			Assert.IsNotNull(pb_ProGridsInterface.GetProGridsType().GetMethod("SnapEnabled", k_BindingFlagsAll));
		}

		[Test]
		public static void SnapValue()
		{
			Assert.IsNotNull(pb_ProGridsInterface.GetProGridsType().GetMethod("SnapValue", k_BindingFlagsAll));
		}

		[Test]
		public static void GetPivot()
		{
			Assert.IsNotNull(pb_ProGridsInterface.GetProGridsType().GetMethod("GetPivot", k_BindingFlagsAll));
		}

		[Test]
		public static void SubscribePushToGridEvent()
		{
			Assert.IsNotNull(pb_ProGridsInterface.GetProGridsType().GetMethod("AddPushToGridListener", k_BindingFlagsAll));
		}

		[Test]
		public static void UnsubscribePushToGridEvent()
		{
			Assert.IsNotNull(pb_ProGridsInterface.GetProGridsType().GetMethod("RemovePushToGridListener", k_BindingFlagsAll));
		}

		[Test]
		public static void OnHandleMove()
		{
			Assert.IsNotNull(pb_ProGridsInterface.GetProGridsType().GetMethod("OnHandleMove", k_BindingFlagsAll));
		}

		[Test]
		public static void SubscribeToolbarEvent()
		{
			Assert.IsNotNull(pb_ProGridsInterface.GetProGridsType().GetMethod("AddToolbarEventSubscriber", k_BindingFlagsAll));
		}

		[Test]
		public static void UnsubscribeToolbarEvent()
		{
			Assert.IsNotNull(pb_ProGridsInterface.GetProGridsType().GetMethod("RemoveToolbarEventSubscriber", k_BindingFlagsAll));
		}
	}
}

#endif
