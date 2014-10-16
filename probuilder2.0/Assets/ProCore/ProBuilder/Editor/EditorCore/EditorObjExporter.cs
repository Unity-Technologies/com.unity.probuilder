/*
Based on ObjExporter.cs, this "wrapper" lets you export to .OBJ directly from the editor menu.
 
This should be put in your "Editor"-folder. Use by selecting the objects you want to export, and select
the appropriate menu item from "Custom->Export". Exported models are put in a folder called
"ExportedObj" in the root of your Unity-project. Textures should also be copied and placed in the
same folder.
N.B. there may be a bug so if the custom option doesn't come up refer to this thread http://answers.unity3d.com/questions/317951/how-to-use-editorobjexporter-obj-saving-script-fro.html */

/**
 *	@todo - Change the way textures are copied so that export is possible at runtime.
 *
 */
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
 
struct ObjMaterial
{
	public string name;
	public string textureName;
}
 
public class EditorObjExporter : ScriptableObject
{
	private static int vertexOffset = 0;
	private static int normalOffset = 0;
	private static int uvOffset = 0;

	public static void MeshToFile(MeshFilter mf, string path) 
	{
		vertexOffset = 0;
		normalOffset = 0;
		uvOffset = 0;

		if(path.Contains(".obj"))
			path = path.Replace(".obj", "");

		string folder = path;
		string filename = Path.GetFileName(path);

		if(!Directory.Exists(folder))
			Directory.CreateDirectory(folder);

		Dictionary<string, ObjMaterial> materialList = new Dictionary<string, ObjMaterial>();

		using (StreamWriter sw = new StreamWriter(folder +"/" + filename + ".obj")) 
		{
			sw.Write("mtllib ./" + filename + ".mtl\n");

			sw.Write(MeshToString(mf, materialList));
		}

		MaterialsToFile(materialList, folder, filename);
	}

	private static string MeshToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList) 
	{
		Mesh m = mf.sharedMesh;
		Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

		StringBuilder sb = new StringBuilder();

		sb.Append("g ").Append(mf.name).Append("\n");
		
		foreach(Vector3 lv in m.vertices) 
		{
			Vector3 wv = mf.transform.TransformPoint(lv);
				
			// this is not how to convert from left to right handed coordinates
			//This is sort of ugly - inverting x-component since we're in
			//a different coordinate system than "everyone" is "used to".
			sb.Append(string.Format("v {0} {1} {2}\n",-wv.x,wv.y,wv.z));
		}

		sb.Append("\n");

		foreach(Vector3 lv in m.normals) 
		{
			Vector3 wv = mf.transform.TransformDirection(lv);

			sb.Append(string.Format("vn {0} {1} {2}\n",-wv.x,wv.y,wv.z));
		}

		sb.Append("\n");

		foreach(Vector3 v in m.uv) 
		{
			sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
		}

		for (int i = 0; i < m.subMeshCount; i ++)
		{
			sb.Append("\n");
			sb.Append("usemtl ").Append(mats[i].name).Append("\n");
			sb.Append("usemap ").Append(mats[i].name).Append("\n");

			ObjMaterial objMaterial = new ObjMaterial();

			objMaterial.name = mats[i].name;

			if (mats[i].mainTexture)
				objMaterial.textureName = UnityEditor.AssetDatabase.GetAssetPath(mats[i].mainTexture);
			else 
				objMaterial.textureName = null;

			materialList.Add(objMaterial.name, objMaterial);

			int[] tri = m.GetTriangles(i);
			for (int n = 0; n < tri.Length; n += 3) 
			{
				//Because we inverted the x-component, we also needed to alter the triangle winding.
				sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n", 
				tri[n]+1 + vertexOffset, tri[n+1]+1 + normalOffset, tri[n+2]+1 + uvOffset));
			}
		}

		vertexOffset += m.vertices.Length;
		normalOffset += m.normals.Length;
		uvOffset += m.uv.Length;

		return sb.ToString();
	}

	private static void Clear()
	{
		vertexOffset = 0;
		normalOffset = 0;
		uvOffset = 0;
	}

	private static void MaterialsToFile(Dictionary<string, ObjMaterial> materialList, string folder, string filename)
	{
		using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".mtl")) 
		{
			foreach( KeyValuePair<string, ObjMaterial> kvp in materialList )
			{
				sw.Write("\n");
				sw.Write("newmtl {0}\n", kvp.Key);
				sw.Write("Ka  1.0 1.0 1.0\n");
				sw.Write("Kd  1.0 1.0 1.0\n");
				sw.Write("Ks  1.0 1.0 1.0\n");
				sw.Write("d  1.0\n");
				sw.Write("Ns  0.0\n");
				sw.Write("illum 2\n");

				if (kvp.Value.textureName != null)
				{
					string destinationFile = kvp.Value.textureName;

					int stripIndex = destinationFile.LastIndexOf('/');//FIXME: Should be Path.PathSeparator;

					if (stripIndex >= 0)
						destinationFile = destinationFile.Substring(stripIndex + 1).Trim();

					string relativeFile = destinationFile;

					destinationFile = folder + "/" + destinationFile;

					try {
						File.Copy(kvp.Value.textureName, destinationFile);
					} catch	{ }

					sw.Write("map_Kd {0}", relativeFile);
				}

				sw.Write("\n\n\n");
			}
		}
	}
}