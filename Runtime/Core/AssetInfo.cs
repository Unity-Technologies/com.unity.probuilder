using System;

namespace UnityEngine.ProBuilder
{
    [Serializable]
    struct AssetInfo : IEquatable<AssetInfo>
    {
        public int objectId;
        public Mesh mesh;

        public AssetInfo(int id)
        {
            objectId = id;
            mesh = null;
        }

        public AssetInfo(AssetInfo src, int id)
        {
            objectId = id;
            mesh = src.mesh;
        }

        public static bool operator ==(AssetInfo self, AssetInfo other)
        {
            return self.Equals(other);
        }

        public static bool operator !=(AssetInfo self, AssetInfo other)
        {
            return !self.Equals(other);
        }

        public override string ToString()
        {
            var meshName = mesh == null ? "null" : mesh.name;
            return $"<b>id:</b> {objectId} <b>mesh:</b> {meshName}";
        }

        public bool Equals(AssetInfo other)
        {
            return objectId == other.objectId && Equals(mesh, other.mesh);
        }

        public override bool Equals(object obj)
        {
            return obj is AssetInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (objectId * 397) ^ (mesh != null ? mesh.GetHashCode() : 0);
            }
        }
    }

}
