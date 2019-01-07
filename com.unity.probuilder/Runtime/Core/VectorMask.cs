using System;

namespace UnityEngine.ProBuilder
{
    struct Vector2Mask
    {
        const byte X = 1 << 0;
        const byte Y = 1 << 1;

        public static readonly Vector2Mask XY = new Vector2Mask(X | Y);

        byte m_Mask;

        public float x
        {
            get { return (m_Mask & X) == X ? 1f : 0f; }
        }

        public float y
        {
            get { return (m_Mask & Y) == Y ? 1f : 0f; }
        }

        public Vector2Mask(Vector3 v, float epsilon = float.Epsilon)
        {
            m_Mask = 0x0;

            if (Mathf.Abs(v.x) > epsilon)
                m_Mask |= X;
            if (Mathf.Abs(v.y) > epsilon)
                m_Mask |= Y;
        }

        public Vector2Mask(byte mask)
        {
            m_Mask = mask;
        }

        public static implicit operator Vector2(Vector2Mask mask)
        {
            return new Vector2(mask.x, mask.y);
        }

        public static implicit operator Vector2Mask(Vector2 v)
        {
            return new Vector2Mask(v);
        }

        public static Vector2Mask operator|(Vector2Mask left, Vector2Mask right)
        {
            return new Vector2Mask((byte)(left.m_Mask | right.m_Mask));
        }

        public static Vector2Mask operator&(Vector2Mask left, Vector2Mask right)
        {
            return new Vector2Mask((byte)(left.m_Mask & right.m_Mask));
        }

        public static Vector2Mask operator^(Vector2Mask left, Vector2Mask right)
        {
            return new Vector2Mask((byte)(left.m_Mask ^ right.m_Mask));
        }

        public static Vector2 operator*(Vector2Mask mask, float value)
        {
            return new Vector2(mask.x * value, mask.y * value);
        }
    }

    struct Vector3Mask : IEquatable<Vector3Mask>
    {
        const byte X = 1 << 0;
        const byte Y = 1 << 1;
        const byte Z = 1 << 2;

        public static readonly Vector3Mask XYZ = new Vector3Mask(X | Y | Z);

        byte m_Mask;

        public float x
        {
            get { return (m_Mask & X) == X ? 1f : 0f; }
        }

        public float y
        {
            get { return (m_Mask & Y) == Y ? 1f : 0f; }
        }

        public float z
        {
            get { return (m_Mask & Z) == Z ? 1f : 0f; }
        }

        public Vector3Mask(Vector3 v, float epsilon = float.Epsilon)
        {
            m_Mask = 0x0;

            if (Mathf.Abs(v.x) > epsilon)
                m_Mask |= X;
            if (Mathf.Abs(v.y) > epsilon)
                m_Mask |= Y;
            if (Mathf.Abs(v.z) > epsilon)
                m_Mask |= Z;
        }

        public Vector3Mask(byte mask)
        {
            m_Mask = mask;
        }

        public override string ToString()
        {
            return string.Format("{{{0}, {1}, {2}}}", x, y, z);
        }

        /// <summary>
        /// The number of toggled axes.
        /// </summary>
        public int active
        {
            get
            {
                int count = 0;
                if ((m_Mask & X) > 0)
                    count++;
                if ((m_Mask & Y) > 0)
                    count++;
                if ((m_Mask & Z) > 0)
                    count++;
                return count;
            }
        }

        public static implicit operator Vector3(Vector3Mask mask)
        {
            return new Vector3(mask.x, mask.y, mask.z);
        }

        public static explicit operator Vector3Mask(Vector3 v)
        {
            return new Vector3Mask(v);
        }

        public static Vector3Mask operator|(Vector3Mask left, Vector3Mask right)
        {
            return new Vector3Mask((byte)(left.m_Mask | right.m_Mask));
        }

        public static Vector3Mask operator&(Vector3Mask left, Vector3Mask right)
        {
            return new Vector3Mask((byte)(left.m_Mask & right.m_Mask));
        }

        public static Vector3Mask operator^(Vector3Mask left, Vector3Mask right)
        {
            return new Vector3Mask((byte)(left.m_Mask ^ right.m_Mask));
        }

        public static Vector3 operator*(Vector3Mask mask, float value)
        {
            return new Vector3(mask.x * value, mask.y * value, mask.z * value);
        }

        public static Vector3 operator*(Quaternion rotation, Vector3Mask mask)
        {
            var active = mask.active;

            if (active > 2)
                return mask;

            var rotated = (rotation * (Vector3)mask).Abs();

            if (active > 1)
            {
                return new Vector3(
                    rotated.x > rotated.y || rotated.x > rotated.z ? 1 : 0,
                    rotated.y > rotated.x || rotated.y > rotated.z ? 1 : 0,
                    rotated.z > rotated.x || rotated.z > rotated.y ? 1 : 0
                );
            }

            return new Vector3(
                rotated.x > rotated.y && rotated.x > rotated.z ? 1 : 0,
                rotated.y > rotated.z && rotated.y > rotated.x ? 1 : 0,
                rotated.z > rotated.x && rotated.z > rotated.y ? 1 : 0);
        }

        public static bool operator ==(Vector3Mask left, Vector3Mask right)
        {
            return left.m_Mask == right.m_Mask;
        }

        public static bool operator !=(Vector3Mask left, Vector3Mask right)
        {
            return !(left == right);
        }

        public float this[int i]
        {
            get
            {
                if(i < 0 || i > 2)
                    throw new IndexOutOfRangeException();

                return (1 & (m_Mask >> i)) * 1f;
            }

            set
            {
                if(i < 0 || i > 2)
                    throw new IndexOutOfRangeException();

                m_Mask &= (byte) ~(1 << i);
                m_Mask |= (byte) ((value > 0f ? 1 : 0) << i);
            }
        }

        public bool Equals(Vector3Mask other)
        {
            return m_Mask == other.m_Mask;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Vector3Mask && Equals((Vector3Mask) obj);
        }

        public override int GetHashCode()
        {
            return m_Mask.GetHashCode();
        }
    }
}
