using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.ProBuilder.AssetIdRemapUtility
{
	class PackageImporter : AssetPostprocessor
	{
		static readonly string[] k_AssetStoreInstallGuids = new string[]
		{
			"0472bdc8d6d15384d98f22ee34302f9c", // ProBuilderCore
			"b617d7797480df7499f141d87e13ebc5", // ProBuilderMeshOps
			"4df21bd079886d84699ca7be1316c7a7"  // ProBuilderEditor
		};

		static readonly string[] k_PackageManagerInstallGuids = new string[]
		{
			"4f0627da958b4bb78c260446066f065f", // Core
			"9b27d8419276465b80eb88c8799432a1", // Mesh Ops
			"e98d45d69e2c4936a7382af00fd45e58", // Editor
		};

		const string k_PackageManagerEditorCore = "e98d45d69e2c4936a7382af00fd45e58";
		const string k_AssetStoreEditorCore = "4df21bd079886d84699ca7be1316c7a7";

		internal static string EditorCorePackageManager { get { return k_PackageManagerEditorCore; } }
		internal static string EditorCoreAssetStore { get { return k_AssetStoreEditorCore; } }

		internal static void SetEditorDllEnabled(string guid, bool isEnabled)
		{
			string dllPath = AssetDatabase.GUIDToAssetPath(guid);

			var importer = AssetImporter.GetAtPath(dllPath) as PluginImporter;

			if (importer != null)
			{
				importer.SetCompatibleWithAnyPlatform(false);
				importer.SetCompatibleWithEditor(isEnabled);
			}
		}

		internal static bool IsEditorPluginEnabled(string guid)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			var importer = AssetImporter.GetAtPath(path) as PluginImporter;
			if (!importer)
				return false;
			return importer.GetCompatibleWithEditor() && !importer.GetCompatibleWithAnyPlatform();
		}

		internal static void Reimport(string guid)
		{
			string dllPath = AssetDatabase.GUIDToAssetPath(guid);

			if(!string.IsNullOrEmpty(dllPath))
				AssetDatabase.ImportAsset(dllPath);
		}

		static bool AreAnyAssetsAreLoaded(string[] guids)
		{
			foreach (var id in guids)
			{
				if (AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(id)) != null)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Check if any pre-4.0 ProBuilder package is present in the project
		/// </summary>
		/// <returns></returns>
		internal static bool IsPreProBuilder4InProject()
		{
			// easiest check, are any of the dlls from asset store present
			if (AreAnyAssetsAreLoaded(k_AssetStoreInstallGuids)
				|| AreAnyAssetsAreLoaded(k_PackageManagerInstallGuids))
				return true;

			// next check if the source version is in the project
			string[] pbObjectMonoScripts = Directory.GetFiles("Assets", "pb_Object.cs", SearchOption.AllDirectories);

			foreach (var pbScriptPath in pbObjectMonoScripts)
			{
				if (pbScriptPath.EndsWith(".cs"))
				{
					MonoScript ms = AssetDatabase.LoadAssetAtPath<MonoScript>(pbScriptPath);

					if (ms != null)
					{
						Type type = ms.GetClass();
						// pre-3.0 didn't have ProBuilder.Core namespace
						return type.ToString().Equals("pb_Object");
					}
				}
			}

			return false;
		}

		static Type FindType(string typeName)
		{
			// First try the current assembly
			Type found = Type.GetType(typeName);

			// Then scan the loaded assemblies
			if (found == null)
			{
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					found = assembly.GetType(typeName);

					if (found != null)
						break;
				}
			}

			return found;
		}

		internal static bool IsProBuilder4OrGreaterLoaded()
		{
			return AppDomain.CurrentDomain.GetAssemblies().Any(x => x.ToString().Contains("Unity.ProBuilder"));
		}
	}
}
