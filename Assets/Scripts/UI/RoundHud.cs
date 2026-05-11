using SuperRodentGame.Combat;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SuperRodentGame.UI
{
    /// World-Space HUD: номер волны, обратный отсчёт, кнопка «Начать сейчас».
    /// Canvas — World Space с TrackedDeviceGraphicRaycaster (совместим с XR Ray Interactor).
    /// При старте каунтдауна позиционируется перед игроком (Camera.main).
    public class RoundHud : MonoBehaviour
    {
        [SerializeField] private WaveDirector waveDirector;
        [SerializeField] private Text roundText;
        [SerializeField] private Text countdownText;
        [SerializeField] private GameObject startPanelRoot;

        [Tooltip("Дистанция от камеры до панели (м).")]
        [SerializeField] private float panelDistance = 1.8f;
        [Tooltip("Смещение по Y от позиции камеры (м).")]
        [SerializeField] private float panelVerticalOffset = -0.15f;

        private bool inCountdown;
        private Canvas runtimeCanvas;
        private GameObject startNowBtnGo;

        private void Awake()
        {
            if (waveDirector == null)
                waveDirector = FindFirstObjectByType<WaveDirector>();

            if (roundText == null)
                BuildWorldSpaceUI();

            if (startPanelRoot != null)
                startPanelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            if (waveDirector == null) return;
            waveDirector.OnRoundCountdownStarted += OnCountdownStarted;
            waveDirector.OnRoundStarted += OnRoundStarted;
            waveDirector.OnAllRoundsCompleted += OnAllDone;
            waveDirector.OnGameOver += OnGameOverReceived;
        }

        private void OnDisable()
        {
            if (waveDirector == null) return;
            waveDirector.OnRoundCountdownStarted -= OnCountdownStarted;
            waveDirector.OnRoundStarted -= OnRoundStarted;
            waveDirector.OnAllRoundsCompleted -= OnAllDone;
            waveDirector.OnGameOver -= OnGameOverReceived;
        }

        private void Update()
        {
            if (!inCountdown || waveDirector == null) return;
            float rem = waveDirector.CountdownRemaining;
            if (countdownText != null)
            {
                countdownText.text = rem > 0f
                    ? $"Авто-старт через: {Mathf.CeilToInt(rem)} с"
                    : "Начинаем...";
            }
        }

        public void OnStartNowClicked()
        {
            waveDirector?.RequestStartRound();
        }

        public void OnRestartClicked()
        {
            waveDirector?.RestartGame();
        }

        private void OnCountdownStarted(int roundIndex, float totalSeconds)
        {
            inCountdown = true;
            if (roundText != null)
                roundText.text = $"Волна {roundIndex + 1} / {(waveDirector != null ? waveDirector.TotalRounds : 0)}";
            PositionInFrontOfPlayer();
            if (startPanelRoot != null)
                startPanelRoot.SetActive(true);
        }

        private void OnRoundStarted(int roundIndex)
        {
            inCountdown = false;
            if (startPanelRoot != null)
                startPanelRoot.SetActive(false);
        }

        private void OnAllDone()
        {
            inCountdown = false;
            if (roundText != null)
                roundText.text = "Все волны отбиты!";
            PositionInFrontOfPlayer();
            if (startPanelRoot != null)
                startPanelRoot.SetActive(true);
            if (countdownText != null)
                countdownText.text = "Победа!";
        }

        private void OnGameOverReceived()
        {
            inCountdown = false;
            PositionInFrontOfPlayer();
            if (startNowBtnGo != null) startNowBtnGo.SetActive(false);
            if (startPanelRoot != null)
                startPanelRoot.SetActive(true);
            if (roundText != null)
                roundText.text = "Ядро уничтожено!";
            if (countdownText != null)
                countdownText.text = "Игра окончена";
            ShowRestartButton();
        }

        private GameObject restartBtnGo;

        private void ShowRestartButton()
        {
            if (restartBtnGo != null) return; // уже создана
            if (startPanelRoot == null) return;

            restartBtnGo = new GameObject("RestartBtn");
            restartBtnGo.transform.SetParent(startPanelRoot.transform, false);
            var rect = restartBtnGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -90f);
            rect.sizeDelta = new Vector2(280f, 72f);
            var img = restartBtnGo.AddComponent<Image>();
            img.color = new Color(0.7f, 0.15f, 0.1f, 1f);
            var btn = restartBtnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            var c = btn.colors;
            c.highlightedColor = new Color(1f, 0.25f, 0.15f, 1f);
            c.pressedColor     = new Color(0.4f, 0.05f, 0.05f, 1f);
            btn.colors = c;
            btn.onClick.AddListener(() =>
            {
                Destroy(restartBtnGo);
                restartBtnGo = null;
                if (startNowBtnGo != null) startNowBtnGo.SetActive(true);
                if (startPanelRoot != null) startPanelRoot.SetActive(false);
                OnRestartClicked();
            });
            MakeLabel(restartBtnGo, "BtnLabel", "Начать заново", 36, Vector2.zero, new Vector2(270f, 64f));
        }

        // ── Позиционирование перед камерой ────────────────────────────────

        private void PositionInFrontOfPlayer()
        {
            var cam = Camera.main;
            if (cam == null || runtimeCanvas == null) return;

            var forward = cam.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            var pos = cam.transform.position
                      + forward * panelDistance
                      + Vector3.up * panelVerticalOffset;

            runtimeCanvas.transform.position = pos;
            runtimeCanvas.transform.rotation = Quaternion.LookRotation(forward);
        }

        // ── World Space UI ─────────────────────────────────────────────────

        private void BuildWorldSpaceUI()
        {
            var root = new GameObject("RoundHUD_Canvas");
            root.transform.SetParent(transform, false);

            runtimeCanvas = root.AddComponent<Canvas>();
            runtimeCanvas.renderMode = RenderMode.WorldSpace;

            // TrackedDeviceGraphicRaycaster вместо обычного GraphicRaycaster
            root.AddComponent<TrackedDeviceGraphicRaycaster>();

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            // Размер 0.8 × 0.3 м (800 × 300 px @ scale 0.001)
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(800f, 300f);
            root.transform.localScale = Vector3.one * 0.001f;

            // Начальная позиция — перед игроком
            root.transform.position = new Vector3(0f, 15f, 5f);

            // ── Панель ──────────────────────────────────────────────────
            startPanelRoot = new GameObject("StartPanel");
            startPanelRoot.transform.SetParent(root.transform, false);
            var panelRect = startPanelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var bg = startPanelRoot.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.1f, 0.85f);

            roundText     = MakeLabel(startPanelRoot, "RoundLabel",     "Волна 1", 64,  new Vector2(0f, 80f),  new Vector2(760f, 80f));
            countdownText = MakeLabel(startPanelRoot, "CountdownLabel", "",        44,  new Vector2(0f, 10f),  new Vector2(760f, 56f));

            // Кнопка «Начать сейчас»
            startNowBtnGo = new GameObject("StartNowBtn");
            var btnGo = startNowBtnGo;
            btnGo.transform.SetParent(startPanelRoot.transform, false);
            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.pivot     = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(0f, -90f);
            btnRect.sizeDelta = new Vector2(280f, 72f);
            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.1f, 0.55f, 0.15f, 1f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var c = btn.colors;
            c.highlightedColor = new Color(0.15f, 0.75f, 0.2f, 1f);
            c.pressedColor     = new Color(0.05f, 0.35f, 0.1f, 1f);
            btn.colors = c;
            btn.onClick.AddListener(OnStartNowClicked);
            var btnLabel = MakeLabel(btnGo, "BtnLabel", "Начать сейчас", 38, Vector2.zero, new Vector2(270f, 64f));
            btnLabel.alignment = TextAnchor.MiddleCenter;
        }

        private static Text MakeLabel(GameObject parent, string goName, string content, int fontSize,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(parent.transform, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin       = new Vector2(0.5f, 0.5f);
            rect.anchorMax       = new Vector2(0.5f, 0.5f);
            rect.pivot           = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta       = sizeDelta;
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                     ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize  = fontSize;
            t.color     = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.text      = content;
            t.raycastTarget = false;
            var outline = go.AddComponent<Outline>();
            outline.effectColor    = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);
            return t;
        }

        private void OnDestroy()
        {
            if (runtimeCanvas != null)
                Destroy(runtimeCanvas.gameObject);
        }
    }
}
