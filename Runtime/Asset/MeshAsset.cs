using System;

namespace UnityEngine.ProBuilder
{
    [Serializable]
    public class MeshAsset : ScriptableObject
    {
        public const string fileExtension = "pm";

        [SerializeField]
        int m_Version;

        [SerializeField]
        int m_AssetHash;

        [SerializeField]
        string m_MeshPath;

        [NonSerialized]
        EditMesh m_EditMesh;

        public int version
        {
            get => m_Version;
            set => m_Version = value;
        }

        public int assetHash => m_AssetHash;

        public string meshPath
        {
            get => m_MeshPath;
            set => m_MeshPath = value;
        }
    }

    [Serializable]
    public class EditMesh
    {
        [SerializeField]
        Color m_Color;

        public Color color
        {
            get => m_Color;
            set => m_Color = value;
        }

        public override int GetHashCode()
        {
            return color.GetHashCode();
        }
    }
}
