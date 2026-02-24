using UnityEngine;
using System.Collections.Generic;

namespace Squishies
{
    public class ObjectPool<T> where T : Component
    {
        private Queue<T> _pool = new Queue<T>();
        private T _prefab;
        private Transform _parent;

        public ObjectPool(T prefab, Transform parent, int initialSize)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < initialSize; i++)
            {
                var obj = Object.Instantiate(prefab, parent);
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public T Get()
        {
            T item;
            if (_pool.Count > 0)
            {
                item = _pool.Dequeue();
            }
            else
            {
                item = Object.Instantiate(_prefab, _parent);
            }
            item.gameObject.SetActive(true);
            return item;
        }

        public void Return(T item)
        {
            item.gameObject.SetActive(false);
            _pool.Enqueue(item);
        }
    }
}
