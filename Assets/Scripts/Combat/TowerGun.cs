using UnityEngine;

namespace SuperRodentGame.Combat
{
    public class TowerGun : MonoBehaviour
    {
        [SerializeField] private float range = 10f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float fireRate = 2f;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private Transform turretHead;

        private float nextFireTime;

        private void Awake()
        {
            if (targetMask.value == 0)
            {
                targetMask.value = -1;
            }
        }

        public void ConfigurePrototypeStats(float rangeValue, float damageValue, float shotsPerSecond)
        {
            range = rangeValue;
            damage = damageValue;
            fireRate = shotsPerSecond;
            targetMask.value = -1;
        }

        private void Update()
        {
            var target = FindClosestTarget();
            if (target == null) return;

            if (turretHead != null)
            {
                turretHead.LookAt(target.transform.position);
            }

            if (Time.time < nextFireTime) return;
            nextFireTime = Time.time + (1f / fireRate);
            target.ApplyDamage(damage);
        }

        private EnemyAgent FindClosestTarget()
        {
            var hits = Physics.OverlapSphere(transform.position, range, targetMask);
            EnemyAgent best = null;
            var bestDistance = float.MaxValue;

            foreach (var hit in hits)
            {
                var enemy = hit.GetComponentInParent<EnemyAgent>();
                if (enemy == null || !enemy.IsAlive) continue;

                var distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }

            return best;
        }
    }
}
