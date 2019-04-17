using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using System.Text;
using System.Linq;
using System.IO;
using MeshUtility = UnityEngine.ProBuilder.MeshUtility;

class MeshInfo : Editor
{
    [MenuItem("Tools/Debug/ProBuilder/Print Mesh Info")]
    static void PrintMeshInfo()
    {
        foreach (MeshFilter mf in Selection.transforms.Select(x => x.GetComponent<MeshFilter>()))
            if (mf.sharedMesh != null)
                Debug.Log(MeshUtility.Print(mf.sharedMesh));
    }

    [MenuItem("Tools/Debug/ProBuilder/Open Mesh Info")]
    static void PrintMeshInfo2()
    {
        foreach (MeshFilter mf in Selection.transforms.Select(x => x.GetComponent<MeshFilter>()))
        {
            if (mf.sharedMesh != null)
                System.Diagnostics.Process.Start(WriteTempFile(MeshUtility.Print(mf.sharedMesh)));
        }
    }

    /**
     *  Create a new unique temporary file and return it's path.
     */
    private static string WriteTempFile(string contents)
    {
        string m_TempFilePath = string.Format("{0}{1}{2}.txt",
                Directory.GetParent(Application.dataPath),
                Path.DirectorySeparatorChar,
                FileUtil.GetUniqueTempPathInProject());

        File.WriteAllText(m_TempFilePath, contents);

        return m_TempFilePath;
    }
}
