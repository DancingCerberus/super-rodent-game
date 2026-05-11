using UnityEngine;

namespace SuperRodentGame.Optimization
{
    public class PooledObject : MonoBehaviour
    {
        private ObjectPool ownerPool;

        public void SetOwner(ObjectPool pool)
        {
            ownerPool = pool;
        }

        public void Release()
        {
            if (ownerPool != null)
            {
                ownerPool.Return(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
