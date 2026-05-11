using System;
using System.Collections;
using SuperRodentGame.AI;
using SuperRodentGame.Core;
using SuperRodentGame.Optimization;
using UnityEngine;

namespace SuperRodentGame.Combat
{
    public class WaveDirector : MonoBehaviour
    {
        [System.Serializable]
        private class EnemyPoolBinding
        {
            public GameObject enemyPrefab;
            public ObjectPool pool;
        }

        [SerializeField] private Transform spawnPoint;
        [SerializeField] private RouteGraph routeGraph;
        [SerializeField] private CoreHealth coreHealth;
        [SerializeField] private WaveDefinition[] waves;
        [SerializeField] private EnemyPoolBinding[] pools;

        [Tooltip("Секунды ожидания перед авто-стартом волны (0 = ждать только кнопку).")]
        [SerializeField] private float roundAutoStartDelay = 15f;

        [SerializeField] private bool ensureTowerSocketsIfMissing = true;

        // Текущее состояние раунда
        public int CurrentRound { get; private set; } = -1;
        public int TotalRounds => waves != null ? waves.Length : 0;
        public float CountdownRemaining { get; private set; }
        public bool IsInCountdown { get; private set; }

        // События для UI
        public event Action<int, float> OnRoundCountdownStarted;  // (roundIndex, totalSeconds)
        public event Action<int> OnRoundStarted;                  // (roundIndex)
        public event Action<int> OnRoundCompleted;                // (roundIndex)
        public event Action OnAllRoundsCompleted;
        public event Action OnGameOver;

        private Coroutine runningRoutine;
        private bool roundStartRequested;
        private bool gameOver;

        private void Start()
        {
            TryEnsureTowerSockets();
            if (coreHealth != null)
                coreHealth.CoreDestroyed += HandleCoreDestroyed;
            if (waves != null && waves.Length > 0 && spawnPoint != null && routeGraph != null && coreHealth != null)
                StartWaves();
        }

        private void OnDestroy()
        {
            if (coreHealth != null)
                coreHealth.CoreDestroyed -= HandleCoreDestroyed;
        }

        private void HandleCoreDestroyed()
        {
            if (gameOver) return;
            gameOver = true;
            if (runningRoutine != null)
            {
                StopCoroutine(runningRoutine);
                runningRoutine = null;
            }
            DespawnAllEnemies();
            IsInCountdown = false;
            OnGameOver?.Invoke();
        }

        public void DespawnAllEnemies()
        {
            var enemies = FindObjectsByType<EnemyAgent>(FindObjectsSortMode.None);
            foreach (var e in enemies)
                e.KillSilent();
            ActiveEnemyCounter.Reset();
        }

        public void RestartGame()
        {
            gameOver = false;
            if (runningRoutine != null)
            {
                StopCoroutine(runningRoutine);
                runningRoutine = null;
            }
            DespawnAllEnemies();
            if (coreHealth != null) coreHealth.Restore();
            if (EconomyWallet.TryGetInstance(out var wallet)) wallet.Reset();
            CurrentRound = -1;
            roundStartRequested = false;
            StartWaves();
        }

        private void TryEnsureTowerSockets()
        {
            if (!ensureTowerSocketsIfMissing) return;
            if (FindObjectsByType<TowerSocket>(FindObjectsSortMode.None).Length > 0) return;
            var builder = FindFirstObjectByType<ArenaFieldBuilder>();
            if (builder != null)
                builder.RebuildTowerSocketsOnly(true);
        }

        public void StartWaves()
        {
            if (runningRoutine != null) return;
            runningRoutine = StartCoroutine(RunWaves());
        }

        /// Вызывается кнопкой в RoundHud — пропускает обратный отсчёт и стартует волну немедленно.
        public void RequestStartRound()
        {
            roundStartRequested = true;
        }

        private IEnumerator RunWaves()
        {
            if (waves == null || waves.Length == 0)
            {
                runningRoutine = null;
                yield break;
            }

            for (int i = 0; i < waves.Length; i++)
            {
                CurrentRound = i;
                roundStartRequested = false;
                IsInCountdown = true;
                float delay = DifficultySettings.StartDelay > 0f ? DifficultySettings.StartDelay : roundAutoStartDelay;
                CountdownRemaining = delay;

                OnRoundCountdownStarted?.Invoke(i, delay);

                // Ждём кнопки или истечения таймера
                while (!roundStartRequested)
                {
                    if (delay > 0f && CountdownRemaining <= 0f)
                        break;

                    CountdownRemaining -= Time.deltaTime;
                    yield return null;
                }

                IsInCountdown = false;
                roundStartRequested = false;
                OnRoundStarted?.Invoke(i);

                yield return StartCoroutine(SpawnWave(waves[i]));
                yield return new WaitUntil(() => ActiveEnemyCounter.AliveCount == 0);

                OnRoundCompleted?.Invoke(i);
            }

            OnAllRoundsCompleted?.Invoke();
            runningRoutine = null;
        }

        private IEnumerator SpawnWave(WaveDefinition wave)
        {
            if (wave == null || wave.Entries == null)
                yield break;

            foreach (var entry in wave.Entries)
            {
                if (entry == null || entry.enemyPrefab == null) continue;

                for (int i = 0; i < entry.count; i++)
                {
                    GameObject enemyGo = null;
                    var poolBindings = pools ?? Array.Empty<EnemyPoolBinding>();
                    for (int p = 0; p < poolBindings.Length; p++)
                    {
                        if (poolBindings[p].enemyPrefab == entry.enemyPrefab && poolBindings[p].pool != null)
                        {
                            enemyGo = poolBindings[p].pool.Get();
                            enemyGo.transform.SetPositionAndRotation(spawnPoint.position, Quaternion.identity);
                            break;
                        }
                    }

                    if (enemyGo == null)
                        enemyGo = Instantiate(entry.enemyPrefab, spawnPoint.position, Quaternion.identity);

                    float scale = Mathf.Clamp(entry.sizeMultiplier, 0.2f, 3f);
                    enemyGo.transform.localScale = Vector3.one * scale;

                    var enemy = enemyGo.GetComponent<EnemyAgent>();
                    if (enemy != null)
                        enemy.Initialize(routeGraph, coreHealth);

                    yield return new WaitForSeconds(entry.spawnInterval);
                }
            }
        }
    }
}
