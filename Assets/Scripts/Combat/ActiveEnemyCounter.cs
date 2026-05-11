using UnityEngine;

namespace SuperRodentGame.Combat
{
    /// <summary>Сколько активных врагов в игре сейчас (после <see cref="EnemyAgent.Initialize"/>).</summary>
    public static class ActiveEnemyCounter
    {
        private static int _alive;

        public static int AliveCount => _alive;

        public static void RegisterSpawned()
        {
            _alive++;
        }

        public static void RegisterDespawned()
        {
            _alive = Mathf.Max(0, _alive - 1);
        }

        public static void Reset()
        {
            _alive = 0;
        }
    }
}
