using SuperRodentGame.Combat;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SuperRodentGame.UI
{
    /// World-Space HUD: HP ядра и золото.
    /// Фиксированная панель в пространстве (не следует за головой, чтобы не вызывать укачивание).
    /// TrackedDeviceGraphicRaycaster — только для чтения, интерактивности нет.
    public class CoreHealthHud : MonoBehaviour
    {
        [SerializeField] private CoreHealth coreHealth;
        [SerializeField] private EconomyWallet economyWallet;
        [SerializeField] private Text hpText;
        [SerializeField] private Text goldText;
        [SerializeField] private string hpFormat   = "❤ {0} / {1}";
        [SerializeField] private string goldFormat  = "★ {0}";

        [Tooltip("Позиция панели в мировых координатах.")]
        [SerializeField] private Vector3 worldPosition = new Vector3(-6f, 15f, 0f);
        [Tooltip("Поворот панели (Euler, градусы).")]
        [SerializeField] private Vector3 worldRotation = new Vector3(0f, 90f, 0f);

        private Canvas runtimeCanvas;
        private bool builtRuntime;

        private void Awake()
        {
            if (coreHealth == null)
                coreHealth = FindFirstObjectByType<CoreHealth>();
            if (economyWallet == null)
                economyWallet = FindFirstObjectByType<EconomyWallet>();

            if (coreHealth == null)
            {
                enabled = false;
                return;
            }

            if (hpText == null)
                BuildWorldSpaceUI();
        }

        private void Start()
        {
            BindWallet();
            RefreshAll();
        }

        private void OnEnable()
        {
            if (coreHealth != null)
            {
                coreHealth.HealthChanged += OnHealthChanged;
                OnHealthChanged(coreHealth.CurrentHealth, coreHealth.MaxHealth);
            }
            BindWallet();
        }

        private void OnDisable()
        {
            if (coreHealth != null)    coreHealth.HealthChanged -= OnHealthChanged;
            if (economyWallet != null) economyWallet.GoldChanged -= OnGoldChanged;
        }

        private void BindWallet()
        {
            if (economyWallet == null)
                economyWallet = FindFirstObjectByType<EconomyWallet>();
            if (economyWallet == null) return;
            economyWallet.GoldChanged -= OnGoldChanged;
            economyWallet.GoldChanged += OnGoldChanged;
            OnGoldChanged(economyWallet.Gold);
        }

        private void OnHealthChanged(float current, float max)
        {
            if (hpText != null)
                hpText.text = string.Format(hpFormat, Mathf.CeilToInt(current), Mathf.CeilToInt(max));
        }

        private void OnGoldChanged(int gold)
        {
            if (goldText != null)
                goldText.text = string.Format(goldFormat, gold);
        }

        private void RefreshAll()
        {
            if (coreHealth != null) OnHealthChanged(coreHealth.CurrentHealth, coreHealth.MaxHealth);
            if (economyWallet != null && goldText != null) OnGoldChanged(economyWallet.Gold);
        }

        // ── World Space UI ─────────────────────────────────────────────────

        private void BuildWorldSpaceUI()
        {
            var root = new GameObject("CoreHUD_Canvas");
            root.transform.SetParent(transform, false);

            runtimeCanvas = root.AddComponent<Canvas>();
            runtimeCanvas.renderMode = RenderMode.WorldSpace;
            root.AddComponent<TrackedDeviceGraphicRaycaster>();
            root.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;

            // 0.5 × 0.15 м
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500f, 150f);
            root.transform.localScale = Vector3.one * 0.001f;
            root.transform.position = worldPosition;
            root.transform.rotation = Quaternion.Euler(worldRotation);

            // Фон
            var bg = new GameObject("BG");
            bg.transform.SetParent(root.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.72f);

            hpText   = MakeLabel(root, "HP",   string.Format(hpFormat, Mathf.CeilToInt(coreHealth.CurrentHealth), Mathf.CeilToInt(coreHealth.MaxHealth)), 38, new Vector2(0f, 30f),  new Vector2(480f, 52f));
            goldText = MakeLabel(root, "Gold", string.Format(goldFormat, 0), 34, new Vector2(0f, -30f), new Vector2(480f, 46f));

            builtRuntime = true;
        }

        private static Text MakeLabel(GameObject parent, string goName, string content, int fontSize,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(parent.transform, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                     ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = fontSize;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.text = content;
            t.raycastTarget = false;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            return t;
        }

        private void OnDestroy()
        {
            if (builtRuntime && runtimeCanvas != null)
                Destroy(runtimeCanvas.gameObject);
        }
    }
}
