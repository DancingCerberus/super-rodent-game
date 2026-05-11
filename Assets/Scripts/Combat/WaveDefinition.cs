using System;
using UnityEngine;

namespace SuperRodentGame.Combat
{
    [CreateAssetMenu(menuName = "SuperRodent/Wave Definition", fileName = "WaveDefinition")]
    public class WaveDefinition : ScriptableObject
    {
        [Serializable]
        public class SpawnEntry
        {
            public GameObject enemyPrefab;
            public int count = 10;
            public float spawnInterval = 0.5f;
            [Tooltip("Масштаб спавнимого врага (1 = нормальный, 0.6 = маленький).")]
            public float sizeMultiplier = 1f;
        }

        [SerializeField] private SpawnEntry[] entries;

        public SpawnEntry[] Entries => entries;
    }
}
