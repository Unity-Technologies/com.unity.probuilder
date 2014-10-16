using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using ProBuilder2.Common;

/**
 * http://wiki.unity3d.com/index.php?title=ObjExporter
 */
public class pb_BatchObjExporter : Editor
{
	/**
	 * Save all pb_Objects to a directory.
	 */
	[MenuItem("Tools/ProBuilder/Actions/Export All Selected to Obj")]
	public static void MenuBatchObjExport()
	{
		string directory = EditorUtility.SaveFilePanelInProject("Export Selected ProBuilder Objects to Directory", "Exported ProBuilder Objects", "", "Enter a name for the new directory to save Obj files to.");

		if(directory != "")
		{
			pb_Object[] pbs = pbUtil.GetComponents<pb_Object>(Selection.transforms);
			bool success = SaveAllToDirectory( pbs, directory );

			pb_Editor_Utility.ShowNotification(success ? "Exported " + pbs.Length + " Objects" : "Failed Export");
		}
	}

	public static bool SaveAllToDirectory(pb_Object[] pbs, string directory)
	{
		if(!Directory.Exists(directory))
			Directory.CreateDirectory(directory);

		Clear();

		foreach(pb_Object pb in pbs)
		{
			string obj_path = GenerateUniqueAssetPath(directory, pb.name, ".obj");
			string mat_path = GenerateUniqueAssetPath(directory, pb.name, ".mtl");

			Dictionary<string, ObjMaterial> materialList = MeshToFile(pb.GetComponent<MeshFilter>(), obj_path, Path.GetFileNameWithoutExtension(mat_path));
			MaterialsToFile(materialList, mat_path);
		}

		return true;
	}

	static string GenerateUniqueAssetPath(string directory, string name, string extension)
	{
		int n = 0;
		string t_name = name;

		while( File.Exists(directory + "/" + t_name + extension) ) {
			t_name = name + (n++).ToString();
		}

		return directory + "/" + name + extension;
	}

#region Write Obj

	private static int vertexOffset = 0;
	private static int normalOffset = 0;
	private static int uvOffset = 0;
	
	private static Dictionary<string, ObjMaterial> MeshToFile(MeshFilter mf, string obj_path_full, string mat_name_no_extension) 
	{
		vertexOffset = 0;
		normalOffset = 0;
		uvOffset = 0;

		Dictionary<string, ObjMaterial> matlist;

		using (StreamWriter sw = new StreamWriter(obj_path_full)) 
		{
			sw.Write("mtllib ./" + mat_name_no_extension + ".mtl\n");

			sw.Write(MeshToString(mf, out matlist));
		}

		return matlist;
	}

	private static string MeshToString(MeshFilter mf, out Dictionary<string, ObjMaterial> materialList) 
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

		materialList = new Dictionary<string, ObjMaterial>();

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

	private static void MaterialsToFile(Dictionary<string, ObjMaterial> materialList, string path)
	{
		using (StreamWriter sw = new StreamWriter(path)) 
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

					string folder = Path.GetDirectoryName(path);
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
#endregion
}