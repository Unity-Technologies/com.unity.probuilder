using UnityEngine;

namespace UnityEditor.ProBuilder
{
	class ISceneEditor : ScriptableObject, ISerializationCallbackReceiver
	{
		protected static ISceneEditor instance;

		public static ISceneEditor Create<T>() where T : ISceneEditor
		{
			if (instance != null)
				return instance;

			instance = ScriptableObject.CreateInstance<T>();
			instance.hideFlags = HideFlags.DontSave;

			SceneView.onSceneGUIDelegate += instance.OnSceneGUI;
			Undo.undoRedoPerformed += instance.UndoRedoPerformed;

			EditorApplication.delayCall += instance.OnInitialize;

			SceneView.RepaintAll();

			return instance;
		}

		public void Close()
		{
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			Undo.undoRedoPerformed -= UndoRedoPerformed;

			instance.OnDestroy();

			instance = null;
			GameObject.DestroyImmediate(this);
			SceneView.RepaintAll();
		}

		public void OnBeforeSerialize()
		{
			OnSerialize();
		}

		public void OnAfterDeserialize()
		{
			instance = this;
			SceneView.onSceneGUIDelegate += OnSceneGUI;
			Undo.undoRedoPerformed += UndoRedoPerformed;
			OnDeserialize();
			SceneView.RepaintAll();
		}

		public virtual void OnInitialize()
		{
		}

		public virtual void OnDestroy()
		{
		}

		public virtual void OnSerialize()
		{
		}

		public virtual void OnDeserialize()
		{
		}

		public virtual void OnSceneGUI(SceneView scnview)
		{
		}

		public virtual void UndoRedoPerformed()
		{
		}
	}
}