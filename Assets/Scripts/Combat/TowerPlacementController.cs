// using SuperRodentGame.InputHelpers;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SuperRodentGame.Combat
{
    /// Постановка башни: на ПК по умолчанию Shift+ЛКМ по collider сокета.
    /// Для VR укажите vrRayOrigin (контроллер) и при желании отключите requireShiftForMousePlacement.
    public class TowerPlacementController : MonoBehaviour
    {
        [SerializeField] private GameObject towerPrefab;
        [SerializeField] private Camera castingCamera;

        [Tooltip("Луч из контроллера/камеры для VR или наведения на сокет сверху.")]
        [SerializeField] private Transform vrRayOrigin;

        [SerializeField] private TowerSocket[] sockets;

        [Tooltip("На ПК: только со Shift (удобно для VR + мышь без ложных кликов); для теста без VR обычно выкл.")]
        [SerializeField] private bool requireShiftForMousePlacement;

        [SerializeField] private int towerPlacementCost = 18;

        [SerializeField] private LayerMask socketRayMask = -1;

        private GameObject _runtimeTowerTemplate;

        private void Awake()
        {
            if (castingCamera == null)
            {
                castingCamera = Camera.main;
            }

            RegisterSocketsFromScene();
        }

        private void Update()
        {
            if (!ShouldProcessPlaceInputThisFrame(out var ray))
            {
                return;
            }

            TryPlaceTowerWithRay(ray);
        }

        private GameObject GetTowerPrototype()
        {
            if (towerPrefab != null)
            {
                return towerPrefab;
            }

            if (_runtimeTowerTemplate == null)
            {
                _runtimeTowerTemplate = CreateRuntimeTowerTemplate();
                _runtimeTowerTemplate.SetActive(false);
                _runtimeTowerTemplate.hideFlags = HideFlags.HideAndDontSave;
            }

            return _runtimeTowerTemplate;
        }

        private static GameObject CreateRuntimeTowerTemplate()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "RuntimeTowerTemplate";
            Object.Destroy(go.GetComponent<CapsuleCollider>());
            var col = go.AddComponent<CapsuleCollider>();
            col.height = 1.4f;
            col.radius = 0.55f;
            col.center = new Vector3(0f, 0.7f, 0f);
            go.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);
            var gun = go.AddComponent<TowerGun>();
            gun.ConfigurePrototypeStats(11f, 12f, 1.75f);
            return go;
        }

        /// Для Canvas Button: индекс в массиве <see cref="sockets"/> (или порядок FindObjectsByType).
        public void UITryPlaceAtSocket(int index)
        {
            RegisterSocketsFromScene();
            if (sockets == null || index < 0 || index >= sockets.Length || sockets[index] == null)
            {
                return;
            }

            TryPlaceTowerPaid(sockets[index]);
        }

        public bool TryPlaceTowerOnSocket(TowerSocket socket)
        {
            RegisterSocketsFromScene();
            if (socket == null || socket.IsOccupied)
            {
                return false;
            }

            return TryPlaceTowerPaid(socket);
        }

        private void RegisterSocketsFromScene()
        {
            if (sockets != null && sockets.Length > 0)
            {
                return;
            }

            sockets = FindObjectsByType<TowerSocket>(FindObjectsSortMode.None);
        }

    private bool ShouldProcessPlaceInputThisFrame(out Ray ray)
{
    ray = default;

    var mouse = UnityEngine.InputSystem.Mouse.current;

    bool placePressed =
        mouse != null &&
        mouse.leftButton.wasPressedThisFrame;

#if ENABLE_INPUT_SYSTEM
    if (!placePressed && Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame)
    {
        placePressed = true;
    }
#endif

    if (!placePressed)
    {
        return false;
    }

    bool modifierOk =
        !requireShiftForMousePlacement ||
        vrRayOrigin != null ||
        (UnityEngine.InputSystem.Keyboard.current != null &&
         UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed);

    if (!modifierOk)
    {
        return false;
    }

    if (vrRayOrigin != null)
    {
        ray = new Ray(vrRayOrigin.position, vrRayOrigin.forward);
        return true;
    }

    if (castingCamera == null)
    {
        return false;
    }

    if (mouse == null)
    {
        return false;
    }

    var screenPos = mouse.position.ReadValue();
    ray = castingCamera.ScreenPointToRay(screenPos);

    return true;
}

        private void TryPlaceTowerWithRay(Ray ray)
        {
            RegisterSocketsFromScene();
            const float maxDist = 400f;

            var hits = Physics.RaycastAll(ray, maxDist, socketRayMask, QueryTriggerInteraction.Collide);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                var socket = hit.collider.GetComponentInParent<TowerSocket>();
                if (socket != null && !socket.IsOccupied && TryPlaceTowerPaid(socket))
                {
                    return;
                }
            }
        }

        /// <summary>Списывает золото и ставит башню (если кошелёка нет в сцене — ставка бесплатная).</summary>
        private bool TryPlaceTowerPaid(TowerSocket socket)
        {
            if (socket == null || socket.IsOccupied)
            {
                return false;
            }

            var proto = GetTowerPrototype();

            if (towerPlacementCost > 0 && EconomyWallet.TryGetInstance(out var wallet))
            {
                if (!wallet.TrySpend(towerPlacementCost))
                {
                    return false;
                }

                if (!socket.TryPlaceTower(proto))
                {
                    wallet.Add(towerPlacementCost);
                    return false;
                }

                return true;
            }

            return socket.TryPlaceTower(proto);
        }
    }
}
