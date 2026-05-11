using System.Collections.Generic;
using SuperRodentGame.Combat;
using UnityEngine;

namespace SuperRodentGame.Optimization
{
    public class EnemyUpdateThrottler : MonoBehaviour
    {
        [SerializeField] private int updatesPerFrame = 20;

        private readonly List<EnemyAgent> trackedEnemies = new List<EnemyAgent>();
        private int cursor;

        public void Register(EnemyAgent enemy)
        {
            if (enemy != null && !trackedEnemies.Contains(enemy))
            {
                trackedEnemies.Add(enemy);
            }
        }

        public void Unregister(EnemyAgent enemy)
        {
            trackedEnemies.Remove(enemy);
        }

        private void LateUpdate()
        {
            if (trackedEnemies.Count == 0) return;

            int count = Mathf.Min(updatesPerFrame, trackedEnemies.Count);
            for (int i = 0; i < count; i++)
            {
                cursor %= trackedEnemies.Count;
                var enemy = trackedEnemies[cursor];
                if (enemy == null || !enemy.gameObject.activeInHierarchy)
                {
                    trackedEnemies.RemoveAt(cursor);
                    continue;
                }

                enemy.transform.hasChanged = false;
                cursor++;
            }
        }
    }
}
