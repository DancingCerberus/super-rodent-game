using System.Collections.Generic;
using UnityEngine;

namespace SuperRodentGame.Optimization
{
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 16;

        private readonly Queue<GameObject> queue = new Queue<GameObject>();

        private void Awake()
        {
            Warmup(initialSize);
        }

        public void Warmup(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var item = Create();
                Return(item);
            }
        }

        public GameObject Get()
        {
            if (queue.Count == 0)
            {
                return Create();
            }

            var item = queue.Dequeue();
            item.SetActive(true);
            return item;
        }

        public void Return(GameObject item)
        {
            if (item == null)
            {
                return;
            }

            item.transform.SetParent(transform);
            item.SetActive(false);
            queue.Enqueue(item);
        }

        private GameObject Create()
        {
            var item = Instantiate(prefab, transform);
            var pooled = item.GetComponent<PooledObject>();
            if (pooled == null)
            {
                pooled = item.AddComponent<PooledObject>();
            }
            pooled.SetOwner(this);
            item.SetActive(false);
            return item;
        }
    }
}
