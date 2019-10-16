using System;

namespace UnityEngine.ProBuilder
{
    // Serializable wrapper around System.Guid
    [Serializable]
    struct ProBuilderGuid : ISerializationCallbackReceiver, IEquatable<ProBuilderGuid>, IComparable<ProBuilderGuid>, IFormattable
    {
        Guid m_Guid;

        [SerializeField]
        byte[] m_Serialized;

        public static implicit operator Guid(ProBuilderGuid id)
        {
            return id.m_Guid;
        }

        public static implicit operator ProBuilderGuid(Guid id)
        {
            return new ProBuilderGuid() { m_Guid = id };
        }

        public void OnBeforeSerialize()
        {
            m_Serialized = m_Guid.ToByteArray();
        }

        public void OnAfterDeserialize()
        {
            m_Guid = new Guid(m_Serialized);
        }

        public bool Equals(ProBuilderGuid other)
        {
            return m_Guid.Equals(other.m_Guid);
        }

        public override bool Equals(object obj)
        {
            return obj is ProBuilderGuid other && Equals(other);
        }

        public override int GetHashCode()
        {
            return m_Guid.GetHashCode();
        }

        public int CompareTo(ProBuilderGuid other)
        {
            return m_Guid.CompareTo(other.m_Guid);
        }

        public override string ToString()
        {
            return ToString("D");
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return m_Guid.ToString(format, formatProvider);
        }
    }

    [Serializable]
    struct AssetInfo : IEquatable<AssetInfo>
    {
        [SerializeField]
        ProBuilderGuid m_Guid;

        [SerializeField]
        int m_InstanceId;

        [SerializeField]
        Mesh m_Mesh;

        public ProBuilderGuid guid
        {
            get { return m_Guid; }
        }

        public int instanceId
        {
            set { m_InstanceId = value; }
            get { return m_InstanceId; }
        }

        public Mesh mesh
        {
            get { return m_Mesh; }
            set { m_Mesh = value; }
        }

        public AssetInfo(int id, Mesh mesh)
        {
            m_InstanceId = id;
            m_Mesh = mesh;
            m_Guid = Guid.NewGuid();
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
            return $"<b>id:</b> {instanceId} <b>guid:</b> {guid.ToString("N")}  <b>mesh:</b> {meshName}";
        }

        public bool Equals(AssetInfo other)
        {
            return instanceId == other.instanceId && Equals(mesh, other.mesh);
        }

        public override bool Equals(object obj)
        {
            return obj is AssetInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = guid.GetHashCode();
                hashCode = (hashCode * 397) ^ instanceId;
                hashCode = (hashCode * 397) ^ (mesh != null ? mesh.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
