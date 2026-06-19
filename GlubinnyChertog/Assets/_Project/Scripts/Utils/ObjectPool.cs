using System.Collections.Generic;
using UnityEngine;

namespace GlubinnyChertog.Utils
{
    /// <summary>
    /// Generic object pool for any Component-based prefab.
    /// Critical for auto-shooter performance: avoids Instantiate/Destroy spikes
    /// when spawning large numbers of bullets/enemies.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly T _prefab;
        private readonly Transform _parent;

        public ObjectPool(T prefab, int prewarmCount, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < prewarmCount; i++)
            {
                T instance = Object.Instantiate(_prefab, _parent);
                instance.gameObject.SetActive(false);
                _pool.Enqueue(instance);
            }
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T instance = _pool.Count > 0
                ? _pool.Dequeue()
                : Object.Instantiate(_prefab, _parent);

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Release(T instance)
        {
            instance.gameObject.SetActive(false);
            _pool.Enqueue(instance);
        }
    }
}
