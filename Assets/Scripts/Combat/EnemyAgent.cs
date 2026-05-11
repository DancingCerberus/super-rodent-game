using System.Collections;
using SuperRodentGame.AI;
using SuperRodentGame.Core;
using SuperRodentGame.Optimization;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SuperRodentGame.Combat
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAgent : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 20f;
        [Tooltip("Здоровье ядра, которое снимает противник при достижении цели (настраивается по типам).")]
        [FormerlySerializedAs("contactDamage")]
        [SerializeField] private float damageToCoreOnReach = 1f;
        [Tooltip("Насколько близко к финальному waypoint считается «дошёл до цели» (план + NavMesh-путь).")]
        [FormerlySerializedAs("contactRange")]
        [SerializeField] private float arrivalRadius = 2.5f;
        [SerializeField] private float retargetDistance = 1.75f;
        [SerializeField] private float navMeshWarpRadius = 10f;
        [SerializeField] private float stagnantRepathThreshold = 0.015f;
        [SerializeField] private float stagnantRepathCooldown = 1.75f;
        [SerializeField] private float minMoveSpeedMultiplier = 0.35f;
        [SerializeField] private Color burnDebugColor = new Color(1f, 0.35f, 0.1f);
        [SerializeField] private int killBountyGold = 3;

        private NavMeshAgent agent;
        private RouteGraph routeGraph;
        private CoreHealth coreHealth;
        private int waypointIndex;
        private float health;
        private PooledObject pooledObject;
        private float slowFactor = 1f;
        private float slowTimer;
        private float burnTimer;
        private float burnDps;
        private Renderer cachedRenderer;
        private Color originalColor = Color.white;
        private float baseSpeed;
        private float stagnantTimer;
        private bool countedInActiveEnemyPool;
        private bool isGrabbed;
        private Rigidbody rb;

        public bool IsAlive => health > 0f;

        private enum EnemyRemovalReason
        {
            ReachedCore,
            KilledByDamage,
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            pooledObject = GetComponent<PooledObject>();
            rb = GetComponent<Rigidbody>();
            cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer != null && cachedRenderer.material.HasProperty("_Color"))
            {
                originalColor = cachedRenderer.material.color;
            }
            baseSpeed = agent.speed * DifficultySettings.SpeedMult;
            health = maxHealth * DifficultySettings.HealthMult;
            stagnantTimer = 0f;

            var grab = GetComponent<XRGrabInteractable>();
            if (grab != null)
            {
                grab.selectEntered.AddListener(OnGrabEnter);
                grab.selectExited.AddListener(OnGrabExit);
            }
        }

        private void OnGrabEnter(SelectEnterEventArgs args)
        {
            isGrabbed = true;
            agent.enabled = false;
            // XRI's SetupRigidbodyGrab already sets isKinematic=false for VelocityTracking,
            // but we ensure agent doesn't fight the physics.
        }

        private void OnGrabExit(SelectExitEventArgs args)
        {
            isGrabbed = false;
            if (!IsAlive) return;

            // XRI called SetupRigidbodyDrop → rb.isKinematic=true before this event.
            // We override it here so XRI's Detach() (runs in LateUpdate) can apply throw velocity.
            if (rb != null) rb.isKinematic = false;

            StopAllCoroutines();
            StartCoroutine(TryLandAfterThrow());
        }

        private IEnumerator TryLandAfterThrow()
        {
            // Wait for throw arc to develop (at least one physics frame), then for landing.
            float timeout = 3f;
            yield return null; // let LateUpdate Detach() apply velocity first
            while (timeout > 0f && IsAlive)
            {
                timeout -= Time.deltaTime;
                float vSq = rb != null ? rb.linearVelocity.sqrMagnitude : 0f;
                if (vSq < 0.15f && timeout < 2.7f) // nearly stopped and has been flying a bit
                    break;
                yield return null;
            }

            if (!IsAlive) yield break;

            if (rb != null) rb.isKinematic = true;

            if (NavMesh.SamplePosition(transform.position, out var hit, navMeshWarpRadius, NavMesh.AllAreas))
            {
                agent.enabled = true;
                agent.Warp(hit.position);
                ApplyDestinationForCurrentWaypoint();
            }
            else
            {
                Die(EnemyRemovalReason.KilledByDamage);
            }
        }

        public void Initialize(RouteGraph route, CoreHealth targetCore)
        {
            if (countedInActiveEnemyPool)
            {
                ActiveEnemyCounter.RegisterDespawned();
                countedInActiveEnemyPool = false;
            }

            StopAllCoroutines();
            routeGraph = route;
            coreHealth = targetCore;
            health = maxHealth;
            stagnantTimer = 0f;
            slowFactor = 1f;
            slowTimer = 0f;
            burnTimer = 0f;
            burnDps = 0f;
            isGrabbed = false;
            agent.speed = baseSpeed * slowFactor;
            enabled = true;
            agent.enabled = true;
            agent.isStopped = false;
            agent.updatePosition = true;
            agent.updateRotation = true;
            if (rb != null) rb.isKinematic = true;

            if (routeGraph == null || routeGraph.Count == 0)
            {
                return;
            }

            if (NavMesh.SamplePosition(transform.position, out var hit, navMeshWarpRadius, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }

            waypointIndex = FindFirstMeaningfulWaypointIndex();
            ApplyDestinationForCurrentWaypoint();

            ActiveEnemyCounter.RegisterSpawned();
            countedInActiveEnemyPool = true;
        }

        private void Update()
        {
            if (isGrabbed || routeGraph == null || coreHealth == null || !IsAlive)
            {
                return;
            }

            UpdateStatusEffects();

            int lastIdx = routeGraph.Count - 1;
            if (waypointIndex == lastIdx && HasArrivedAtDestination(lastIdx))
            {
                coreHealth.ApplyDamage(damageToCoreOnReach);
                Die(EnemyRemovalReason.ReachedCore);
                return;
            }

            TryRecoverStuckMovement();

            var waypointDistance = PlanarDistance(transform.position, routeGraph.GetPoint(waypointIndex));
            if (waypointDistance <= retargetDistance && waypointIndex < lastIdx)
            {
                waypointIndex = routeGraph.GetNextIndex(waypointIndex);
                ApplyDestinationForCurrentWaypoint();
            }
        }

        private bool HasArrivedAtDestination(int lastIdx)
        {
            var dest = routeGraph.GetPoint(lastIdx);
            float planar = PlanarDistance(transform.position, dest);
            if (planar <= arrivalRadius)
            {
                return true;
            }

            if (agent.pathPending)
            {
                return false;
            }

            if (!agent.hasPath)
            {
                return false;
            }

            return agent.remainingDistance <= agent.stoppingDistance + 0.4f;
        }

        private int FindFirstMeaningfulWaypointIndex()
        {
            float skipDist = Mathf.Max(retargetDistance * 1.5f, 1.5f);
            for (int i = 0; i < routeGraph.Count; i++)
            {
                if (PlanarDistance(transform.position, routeGraph.GetPoint(i)) > skipDist)
                {
                    return i;
                }
            }

            return routeGraph.Count - 1;
        }

        private void ApplyDestinationForCurrentWaypoint()
        {
            if (routeGraph == null || routeGraph.Count == 0)
            {
                return;
            }

            var dest = routeGraph.GetPoint(waypointIndex);
            if (NavMesh.SamplePosition(dest, out var hit, 8f, NavMesh.AllAreas))
            {
                dest = hit.position;
            }

            agent.SetDestination(dest);
        }

        private static float PlanarDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        private void TryRecoverStuckMovement()
        {
            if (agent.pathPending)
            {
                return;
            }

            if (agent.velocity.sqrMagnitude > stagnantRepathThreshold)
            {
                stagnantTimer = 0f;
                return;
            }

            stagnantTimer += Time.deltaTime;
            if (stagnantTimer < stagnantRepathCooldown)
            {
                return;
            }

            stagnantTimer = 0f;
            if (NavMesh.SamplePosition(transform.position, out var hit, navMeshWarpRadius, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }

            ApplyDestinationForCurrentWaypoint();
        }

        public void KillSilent()
        {
            if (IsAlive) Die(EnemyRemovalReason.ReachedCore);
        }

        public void ApplyDamage(float damage)
        {
            health -= damage;
            if (health <= 0f)
            {
                Die(EnemyRemovalReason.KilledByDamage);
            }
        }

        public void ApplySlow(float multiplier, float duration)
        {
            slowFactor = Mathf.Clamp(multiplier, minMoveSpeedMultiplier, 1f);
            slowTimer = Mathf.Max(slowTimer, duration);
            agent.speed = baseSpeed * slowFactor;
        }

        public void ApplyBurn(float dps, float duration)
        {
            burnDps = Mathf.Max(burnDps, dps);
            burnTimer = Mathf.Max(burnTimer, duration);
            if (cachedRenderer != null && cachedRenderer.material.HasProperty("_Color"))
            {
                cachedRenderer.material.color = burnDebugColor;
            }
        }

        private void UpdateStatusEffects()
        {
            if (slowTimer > 0f)
            {
                slowTimer -= Time.deltaTime;
                if (slowTimer <= 0f)
                {
                    slowFactor = 1f;
                    agent.speed = baseSpeed;
                }
            }

            if (burnTimer > 0f)
            {
                burnTimer -= Time.deltaTime;
                ApplyDamage(burnDps * Time.deltaTime);
                if (burnTimer <= 0f && cachedRenderer != null && cachedRenderer.material.HasProperty("_Color"))
                {
                    cachedRenderer.material.color = originalColor;
                }
            }
        }

        private void Die(EnemyRemovalReason reason)
        {
            StopAllCoroutines();
            if (countedInActiveEnemyPool)
            {
                ActiveEnemyCounter.RegisterDespawned();
                countedInActiveEnemyPool = false;
            }

            if (reason == EnemyRemovalReason.KilledByDamage &&
                EconomyWallet.TryGetInstance(out var economy))
            {
                economy.Add(Mathf.Max(0, killBountyGold));
            }

            health = 0f;
            isGrabbed = false;
            agent.enabled = false;
            if (rb != null) rb.isKinematic = true;

            if (pooledObject != null)
            {
                pooledObject.Release();
                return;
            }

            gameObject.SetActive(false);
        }
    }
}
