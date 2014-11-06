#if !UNITY_WP8

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using ProBuilder2.Common;

public class SaveLoadSerializedPbObject : Editor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Experimental/Load ProBuilder Object")]
	public static void MenuLoadPbObject()
	{
		string path = EditorUtility.OpenFilePanel("Load Serialized ProBuilder Object", Application.dataPath + "../", "pbo");

		pb_SerializableObject obj = null;

		BinaryFormatter formatter = new BinaryFormatter();
		Debug.Log("path : " + path);
		Stream stream = File.Open(path, FileMode.Open);

		obj = (pb_SerializableObject)formatter.Deserialize(stream);
		stream.Close();

		Selection.activeTransform = pb_Object.InitWithSerializableObject(obj).transform;
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Experimental/Save ProBuilder Object to File")]
	public static void MenuSavePbObject()
	{
		pb_Object[] selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);
		int len = selection.Length;

		if(len < 1) return;

		string path = "";

		if(len == 1)
			path = EditorUtility.SaveFilePanel("Save ProBuilder Object", "", selection[0].name, "pbo");// "Save ProBuilder Object to File.");
		else
			path = EditorUtility.SaveFolderPanel("Save ProBuilder Objects to Folder", "", "");

		foreach(pb_Object pb in selection)
		{
			//Creates a new pb_SerializableObject object.
			pb_SerializableObject obj = new pb_SerializableObject(pb);

			//Opens a file and serializes the object into it in binary format.
			Stream stream = File.Open( len == 1 ? path : path + pb.name + ".pbo", FileMode.Create);

			BinaryFormatter formatter = new BinaryFormatter();

			formatter.Serialize(stream, obj);
			
			stream.Close();			
		}
	}
}

#endif