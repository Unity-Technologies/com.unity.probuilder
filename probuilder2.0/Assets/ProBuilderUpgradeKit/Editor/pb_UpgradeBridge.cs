using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using ProBuilder2.Common;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.EditorCommon
{}

/**
 *	UpgradeKit namespace contains utilities for serializing ProBuilder components to JSON and re-initializing new components.
 *	ProBuilder 2.4.0f4 is the earliest supported version.
 */
namespace ProBuilder2.UpgradeKit
{
	/**
 	 * Methods for storing data about ProBuilder objects that may be translated back into PB post-upgrade.
	 */
	public class pb_UpgradeBridgeEditor : Editor
	{
		const string MaterialFieldRegex = "\"material\": [\\-0-9]{2,20}";

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Upgrade/Prepare Scene for Upgrade")]
		// [MenuItem("Tools/SERIALIZE SCENE")]
		static void MenuSerialize()
		{
			pb_Object[] objects = ((pb_Object[])Resources.FindObjectsOfTypeAll(typeof(pb_Object))).Where(x => x.gameObject.hideFlags == HideFlags.None).ToArray();
			pb_Object[] prefabs = FindPrefabsWithComponent<pb_Object>();

			objects = pbUtil.Concat(objects, prefabs).Distinct().ToArray();

			MakeSerializedComponent(objects);

		}

		static void MakeSerializedComponent(pb_Object[] objects)
		{
			if(pb_Editor.instance != null)
				pb_Editor.instance.Close();

			float len = objects.Length;
			int success = 0;

			if( len < 1 )
			{
				int c = ((pb_SerializedComponent[]) Resources.FindObjectsOfTypeAll(typeof(pb_SerializedComponent))).Length;

				if( c > 0 )
					EditorUtility.DisplayDialog("Prepare Scene", "There are " + c + " serialized components ready to be rebuilt into ProBuilder components in this scene.", "Okay");
				else
					EditorUtility.DisplayDialog("Prepare Scene", "No ProBuilder components found in scene.", "Okay");
			}
			else
			{
				if( !EditorUtility.DisplayDialog("Prepare Scene", "This will safely store all ProBuilder data in a new component, and remove ProBuilder components from all objects in the scene.\n\nThis must be run for each scene in your project.", "Okay", "Cancel") )
					return;

				foreach(pb_Object pb in objects)
				{
					if(pb == null)
						continue;

					EditorUtility.DisplayProgressBar("Serialize ProBuilder Data", "Object: " + pb.name, success / len);

					try
					{
						bool isPrefabInstance = IsPrefabInstance(pb.gameObject);

						if(isPrefabInstance)
							PrefabUtility.DisconnectPrefabInstance(pb.gameObject);

						pb_SerializableObject serializedObject = new pb_SerializableObject(pb);
						pb_SerializableEntity serializedEntity = new pb_SerializableEntity(pb.GetComponent<pb_Entity>());

						string obj = JsonConvert.SerializeObject(serializedObject, Formatting.Indented);
						string entity = JsonConvert.SerializeObject(serializedEntity, Formatting.Indented);
						
						pb_SerializedComponent storage = pb.gameObject.AddComponent<pb_SerializedComponent>();
						storage.isPrefabInstance = isPrefabInstance;

						storage.SetObjectData(obj);
						storage.SetEntityData(entity);

						RemoveProBuilderScripts(pb);

						success++;
					}
					catch (System.Exception e)
					{
						if( IsPrefabRoot(pb.gameObject) )
							Debug.LogWarning("Failed serializing: " + pb.name + "\nId: " + pb.gameObject.GetInstanceID() + "\nThis object is a prefab parent, and not in the current scene.  If this prefab is not used in another scene, it may not be safely saved.  To fix this warning, please place an instance of this prefab in a scene and run the \"Prepare Scene for Upgrade\" menu item");
						else
							Debug.LogError("Failed serializing: " + pb.name + "\nId: " + pb.gameObject.GetInstanceID() + "\nThis object will not be safely upgraded if you continue the process!\n" + e.ToString());
					}
				}

				EditorUtility.ClearProgressBar();

				#if UNITY_5
				EditorUtility.UnloadUnusedAssetsImmediate();
				#else
				EditorUtility.UnloadUnusedAssets();
				#endif

				if( EditorUtility.DisplayDialog("Prepare Scene", "Successfully serialized " + success + " / " + (int)len + " objects.", "Save Scene", "Don't Save"))
					EditorApplication.SaveScene("", false);
			}			
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Upgrade/Re-attach ProBuilder Scripts")]
		// [MenuItem("Tools/UN - SERIALIZE SCENE")]
		static void MenuDeserialize()
		{
			pb_SerializedComponent[] scene = (pb_SerializedComponent[])Resources.FindObjectsOfTypeAll(typeof(pb_SerializedComponent));
			pb_SerializedComponent[] prefabs = FindPrefabsWithComponent<pb_SerializedComponent>();

			pb_SerializedComponent[] serializedComponents = pbUtil.Concat(scene, prefabs).Distinct().ToArray();

			if( serializedComponents.Length < 1 )
			{
				EditorUtility.DisplayDialog("Deserialize ProBuilder Data", "No serialized ProBuilder components found in this scene.", "Okay");
			}
			else
			{
				int success = 0, c = 0;
				float len = serializedComponents.Length;

				for(int i = 0; i < serializedComponents.Length; i++)
				{
					pb_SerializedComponent ser = serializedComponents[i];

					EditorUtility.DisplayProgressBar("Deserialize ProBuilder Data", "Object: " + ser.gameObject.name, c++ / len);

					try
					{
						pb_SerializableObject serializedObject = JsonConvert.DeserializeObject<pb_SerializableObject>(ser.GetObjectData());
						pb_SerializableEntity serializedEntity = JsonConvert.DeserializeObject<pb_SerializableEntity>(ser.GetEntityData());

						pb_Object pb = ser.gameObject.GetComponent<pb_Object>() ?? ser.gameObject.AddComponent<pb_Object>();
						pb_EditorUpgradeKitUtils.InitObjectWithSerializedObject(pb, serializedObject);

						pb_Entity ent = ser.gameObject.GetComponent<pb_Entity>() ?? ser.gameObject.AddComponent<pb_Entity>();
						pb_EditorUpgradeKitUtils.InitEntityWithSerializedObject(ent, serializedEntity);

						// Check if the instance is equal to the prefab (vertex and uv changes aren't recorded as propertymodifications)
						// If it is, reconnect the prefab to the root.
						if( ser.isPrefabInstance )
						{
							Object parent = PrefabUtility.GetPrefabParent(ser.gameObject);

							if(parent != null)
							{
								pb_Object parent_pb = ((GameObject)parent).GetComponent<pb_Object>();

								if(parent_pb != null)
								{
									if( pb_UpgradeKitUtils.AreEqual(parent_pb, pb) )
										PrefabUtility.ReconnectToLastPrefab(ser.gameObject);
								}
								else
								{
									pb_SerializedComponent parent_ser_component = ((GameObject)parent).GetComponent<pb_SerializedComponent>();

									if( parent_ser_component != null )
									{
										if( parent_ser_component.GetObjectData().Equals(serializedObject) )
										{
											PrefabUtility.ReconnectToLastPrefab(ser.gameObject);
										}
									}
								}
							}
						}

						// Check if the object is a prefab root, and if so, mark the mesh with appropriate hideflags
						if(pb && IsPrefabRoot(pb.gameObject))
							pb.msh.hideFlags = (HideFlags) (1 | 2 | 4 | 8);

						success++;
					}
					catch(System.Exception e)
					{
						if(ser != null)
							Debug.LogError("Failed deserializing object: " + ser.gameObject.name + "\nObject ID: " + ser.gameObject.GetInstanceID() + "\n" + e.ToString());
						else
							Debug.LogError("Failed deserializing object\n" + e.ToString());

						continue;
					}

					DestroyImmediate( ser, true );
				}

				#if UNITY_5
				EditorUtility.UnloadUnusedAssetsImmediate();
				#else
				EditorUtility.UnloadUnusedAssets();
				#endif

				EditorUtility.ClearProgressBar();

				if( EditorUtility.DisplayDialog("Deserialize ProBuilder Data", "Successfully deserialized " + success + " / " + (int)len + " objects.", "Save", "Don't Save"))
					EditorApplication.SaveScene("", false);
			}
		}

		static void RemoveProBuilderScripts(pb_Object pb)
		{
			GameObject go = pb.gameObject;

			pb.Verify();

			if(pb.msh == null)
				return;

			// Copy the mesh (since destroying pb_Object will take the mesh reference with it)
			Mesh m = pb_UpgradeKitUtils.DeepCopyMesh(pb.msh);

			// Destroy pb_Object first, then entity.  Order is important.
			DestroyImmediate(pb, true);
			
			if(go.GetComponent<pb_Entity>())
				DestroyImmediate(go.GetComponent<pb_Entity>(), true);

			// Set the mesh back.
			go.GetComponent<MeshFilter>().sharedMesh = m;

			if(go.GetComponent<MeshCollider>())
				go.GetComponent<MeshCollider>().sharedMesh = m;
		}

		/**
		 * Returns all prefabs that reference pb_Object
		 */
		static T[] FindPrefabsWithComponent<T>() where T : Component
		{
			List<T> components = new List<T>();

			// t:T doesn't return anything, presumably because the top level asset is a gameObject.
			foreach(string cheese in AssetDatabase.FindAssets("t:GameObject"))
			{
				Object[] prefabs = (Object[])AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(cheese));

				foreach(GameObject go in prefabs.Where(x => x is GameObject))
				{
					T pb = go.GetComponent<T>();

					if(pb != null) components.Add(pb);

					T[] all = go.GetComponentsInChildren<T>();

					foreach(T i in all)
					{
						components.Add(i);
					}
				}
			}

			return components.ToArray();
		}

		static bool IsPrefabInstance(GameObject go)
		{
			return PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance;
		}

		static bool IsPrefabRoot(GameObject go)
		{
			return PrefabUtility.GetPrefabType(go) == PrefabType.Prefab;
		}
	}
}