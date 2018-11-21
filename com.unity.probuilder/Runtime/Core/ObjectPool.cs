using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Simple object pool implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    sealed class ObjectPool<T> : IDisposable
    {
        bool m_IsDisposed;
        Queue<T> m_Pool = new Queue<T>();

        public int desiredSize;
        public Func<T> constructor;
        public Action<T> destructor;

        public ObjectPool(int initialSize, int desiredSize, Func<T> constructor, Action<T> destructor, bool lazyInitialization = false)
        {
            if (constructor == null)
                throw new ArgumentNullException("constructor");

            if (destructor == null)
                throw new ArgumentNullException("destructor");

            this.constructor = constructor;
            this.destructor = destructor;
            this.desiredSize = desiredSize;

            for (int i = 0; i < initialSize && i < desiredSize && !lazyInitialization; i++)
                m_Pool.Enqueue(constructor());
        }

        public T Get()
        {
            if (m_Pool.Count > 0)
                return m_Pool.Dequeue();
            return constructor();
        }

        public void Put(T obj)
        {
            if (m_Pool.Count < desiredSize)
                m_Pool.Enqueue(obj);
            else
                destructor(obj);
        }

        public void Empty()
        {
            int count = m_Pool.Count;

            for (int i = 0; i < count; i++)
                destructor(m_Pool.Dequeue());
        }

        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposing && !m_IsDisposed)
            {
                Empty();
                m_IsDisposed = true;
            }
        }
    }
}
