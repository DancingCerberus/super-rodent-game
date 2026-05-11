using SuperRodentGame.Audio;
using SuperRodentGame.Combat;
using SuperRodentGame.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SuperRodentGame.UI
{
    /// World-Space панель настроек: громкость (мастер / музыка / SFX).
    /// Фиксирована в пространстве; открывается кнопкой ⚙ (тоже World Space).
    /// TrackedDeviceGraphicRaycaster + XRUIInputModule на EventSystem → кнопки/слайдеры работают с XR-лучом.
    public class SettingsHud : MonoBehaviour
    {
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private WaveDirector waveDirector;
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private GameObject panelRoot;

        [Tooltip("Дистанция кнопки ⚙ от камеры (м).")]
        [SerializeField] private float gearDistance = 1.8f;
        [Tooltip("Смещение кнопки ⚙ вправо от взгляда (м).")]
        [SerializeField] private float gearSideOffset = 0.55f;
        [Tooltip("Смещение кнопки ⚙ по Y от камеры (м).")]
        [SerializeField] private float gearVerticalOffset = -0.3f;
        [Tooltip("Дистанция панели настроек от камеры при открытии (м).")]
        [SerializeField] private float panelDistance = 1.8f;
        [Tooltip("Смещение панели по Y от камеры (м).")]
        [SerializeField] private float panelVerticalOffset = -0.15f;

        private Canvas gearCanvas;
        private Canvas runtimeCanvas;
        private bool panelVisible;
        private Button[] difficultyButtons;
        private Text difficultyHintText;

        private void Awake()
        {
            if (audioManager == null)
                audioManager = FindFirstObjectByType<AudioManager>();
            if (waveDirector == null)
                waveDirector = FindFirstObjectByType<WaveDirector>();

            if (masterSlider == null)
                BuildWorldSpaceUI();

            LoadSettings();
            SetPanelVisible(false);
        }

        private void Start()
        {
            PositionGearNearPlayer();
        }

        private void PositionGearNearPlayer()
        {
            if (gearCanvas == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            var forward = cam.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            var right = Vector3.Cross(Vector3.up, forward).normalized * -1f;
            var pos = cam.transform.position
                      + forward * gearDistance
                      + right   * gearSideOffset
                      + Vector3.up * gearVerticalOffset;

            gearCanvas.transform.position = pos;
            gearCanvas.transform.rotation = Quaternion.LookRotation(forward);
        }

        public void TogglePanel()
        {
            SetPanelVisible(!panelVisible);
        }

        public void SetPanelVisible(bool visible)
        {
            panelVisible = visible;
            if (panelRoot != null) panelRoot.SetActive(visible);
            if (visible) PositionPanelInFrontOfPlayer();
        }

        private void PositionPanelInFrontOfPlayer()
        {
            if (runtimeCanvas == null) return;
            var cam = Camera.main;
            if (cam == null) return;

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

        private void LoadSettings()
        {
            if (masterSlider != null) masterSlider.value = PlayerPrefs.GetFloat("settings.master", 0.8f);
            if (musicSlider  != null) musicSlider.value  = PlayerPrefs.GetFloat("settings.music",  0.7f);
            if (sfxSlider    != null) sfxSlider.value    = PlayerPrefs.GetFloat("settings.sfx",    0.8f);
        }

        public void OnMasterChanged(float v)
        {
            PlayerPrefs.SetFloat("settings.master", v);
            AudioListener.volume = v;
        }

        public void OnMusicChanged(float v)
        {
            PlayerPrefs.SetFloat("settings.music", v);
            if (audioManager != null) audioManager.SetMusicVolume(v);
        }

        public void OnSfxChanged(float v)
        {
            PlayerPrefs.SetFloat("settings.sfx", v);
            if (audioManager != null) audioManager.SetSfxVolume(v);
        }

        public void OnDifficultySelected(int index)
        {
            DifficultySettings.Current = (Difficulty)index;
            RefreshDifficultyButtons();

            bool roundActive = waveDirector != null
                               && waveDirector.CurrentRound >= 0
                               && !waveDirector.IsInCountdown;

            if (!roundActive)
            {
                var core = FindFirstObjectByType<CoreHealth>();
                if (core != null)
                    core.SetMaxHealth(DifficultySettings.StartCoreHp);
            }

            if (difficultyHintText != null)
                difficultyHintText.text = roundActive ? "Вступит в силу со следующего раунда" : "";
        }

        private void RefreshDifficultyButtons()
        {
            if (difficultyButtons == null) return;
            var active = (int)DifficultySettings.Current;
            var activeColor   = new Color(0.1f, 0.55f, 0.2f, 1f);
            var inactiveColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            for (int i = 0; i < difficultyButtons.Length; i++)
            {
                var img = difficultyButtons[i].targetGraphic as Image;
                if (img != null) img.color = i == active ? activeColor : inactiveColor;
            }
        }

        // ── World Space UI ─────────────────────────────────────────────────

        private void BuildWorldSpaceUI()
        {
            // ── Кнопка-триггер (⚙) ───────────────────────────────────────
            var gearRoot = new GameObject("Settings_GearCanvas");
            gearRoot.transform.SetParent(transform, false);
            gearCanvas = gearRoot.AddComponent<Canvas>();
            gearCanvas.renderMode = RenderMode.WorldSpace;
            gearRoot.AddComponent<TrackedDeviceGraphicRaycaster>();
            gearRoot.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;
            var gearRt = gearRoot.GetComponent<RectTransform>();
            gearRt.sizeDelta = new Vector2(80f, 80f);
            gearRoot.transform.localScale = Vector3.one * 0.001f;
            // позиционируется в Start() относительно камеры

            var gearBtnGo = new GameObject("GearButton");
            gearBtnGo.transform.SetParent(gearRoot.transform, false);
            var gearBtnRect = gearBtnGo.AddComponent<RectTransform>();
            gearBtnRect.anchorMin = Vector2.zero;
            gearBtnRect.anchorMax = Vector2.one;
            gearBtnRect.offsetMin = Vector2.zero;
            gearBtnRect.offsetMax = Vector2.zero;
            var gearImg = gearBtnGo.AddComponent<Image>();
            gearImg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);
            var gearBtn = gearBtnGo.AddComponent<Button>();
            gearBtn.targetGraphic = gearImg;
            gearBtn.onClick.AddListener(TogglePanel);
            MakeInlineLabel(gearBtnGo, "⚙", 44);

            // ── Панель настроек ───────────────────────────────────────────
            var mainRoot = new GameObject("Settings_PanelCanvas");
            mainRoot.transform.SetParent(transform, false);
            runtimeCanvas = mainRoot.AddComponent<Canvas>();
            runtimeCanvas.renderMode = RenderMode.WorldSpace;
            mainRoot.AddComponent<TrackedDeviceGraphicRaycaster>();
            mainRoot.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;
            var mainRt = mainRoot.GetComponent<RectTransform>();
            mainRt.sizeDelta = new Vector2(500f, 460f);
            mainRoot.transform.localScale = Vector3.one * 0.001f;
            // начальная позиция — при открытии переедет перед игроком

            panelRoot = new GameObject("Panel");
            panelRoot.transform.SetParent(mainRoot.transform, false);
            var panelRect = panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.12f, 0.92f);

            // Заголовок
            MakeLabel(panelRoot, "Title", "Настройки", 40, new Vector2(0f, 200f), new Vector2(480f, 52f));

            // ── Громкость ─────────────────────────────────────────────────
            MakeLabel(panelRoot, "VolSection", "Громкость", 26, new Vector2(0f, 148f), new Vector2(480f, 36f));
            masterSlider = AddSliderRow(panelRoot, "Мастер", 100f, PlayerPrefs.GetFloat("settings.master", 0.8f), OnMasterChanged);
            musicSlider  = AddSliderRow(panelRoot, "Музыка",  48f, PlayerPrefs.GetFloat("settings.music",  0.7f), OnMusicChanged);
            sfxSlider    = AddSliderRow(panelRoot, "Звуки",   -4f, PlayerPrefs.GetFloat("settings.sfx",    0.8f), OnSfxChanged);

            // ── Сложность ─────────────────────────────────────────────────
            MakeLabel(panelRoot, "DiffSection", "Сложность", 26, new Vector2(0f, -64f), new Vector2(480f, 36f));

            string[] labels = { "Лёгкая", "Нормальная", "Сложная" };
            difficultyButtons = new Button[3];
            float[] btnX = { -160f, 0f, 160f };
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                var btnGo = new GameObject($"Diff_{labels[i]}");
                btnGo.transform.SetParent(panelRoot.transform, false);
                var btnRect = btnGo.AddComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(0.5f, 0.5f);
                btnRect.anchorMax = new Vector2(0.5f, 0.5f);
                btnRect.pivot     = new Vector2(0.5f, 0.5f);
                btnRect.anchoredPosition = new Vector2(btnX[i], -116f);
                btnRect.sizeDelta = new Vector2(144f, 52f);
                var btnImg = btnGo.AddComponent<Image>();
                var btn = btnGo.AddComponent<Button>();
                btn.targetGraphic = btnImg;
                btn.onClick.AddListener(() => OnDifficultySelected(idx));
                difficultyButtons[i] = btn;
                MakeInlineLabel(btnGo, labels[i], 26);
            }
            RefreshDifficultyButtons();

            difficultyHintText = MakeLabel(panelRoot, "DiffHint", "", 22, new Vector2(0f, -158f), new Vector2(480f, 30f));
            difficultyHintText.color = new Color(1f, 0.85f, 0.3f);

            // Кнопка «Закрыть»
            var closeGo = new GameObject("CloseBtn");
            closeGo.transform.SetParent(panelRoot.transform, false);
            var closeRect = closeGo.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.5f, 0.5f);
            closeRect.anchorMax = new Vector2(0.5f, 0.5f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = new Vector2(0f, -198f);
            closeRect.sizeDelta = new Vector2(180f, 52f);
            var closeImg = closeGo.AddComponent<Image>();
            closeImg.color = new Color(0.5f, 0.1f, 0.1f, 1f);
            var closeBtn = closeGo.AddComponent<Button>();
            closeBtn.targetGraphic = closeImg;
            closeBtn.onClick.AddListener(() => SetPanelVisible(false));
            MakeInlineLabel(closeGo, "Закрыть", 30);
        }

        private Slider AddSliderRow(GameObject parent, string label, float yPos, float initValue,
            UnityEngine.Events.UnityAction<float> onChange)
        {
            var row = new GameObject($"Row_{label}");
            row.transform.SetParent(parent.transform, false);
            var rowRect = row.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 0.5f);
            rowRect.anchorMax = new Vector2(1f, 0.5f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = new Vector2(0f, yPos);
            rowRect.sizeDelta = new Vector2(0f, 50f);

            var lbl = new GameObject("Label");
            lbl.transform.SetParent(row.transform, false);
            var lblRect = lbl.AddComponent<RectTransform>();
            lblRect.anchorMin = new Vector2(0f, 0f);
            lblRect.anchorMax = new Vector2(0.38f, 1f);
            lblRect.offsetMin = new Vector2(18f, 0f);
            lblRect.offsetMax = Vector2.zero;
            var lblTxt = lbl.AddComponent<Text>();
            lblTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                          ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            lblTxt.fontSize = 28;
            lblTxt.color = Color.white;
            lblTxt.text = label;
            lblTxt.alignment = TextAnchor.MiddleLeft;
            lblTxt.raycastTarget = false;

            var sliderGo = new GameObject("Slider");
            sliderGo.transform.SetParent(row.transform, false);
            var sliderRect = sliderGo.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.4f, 0.15f);
            sliderRect.anchorMax = new Vector2(0.96f, 0.85f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;
            var bgImg = sliderGo.AddComponent<Image>();
            bgImg.color = new Color(0.25f, 0.25f, 0.3f, 1f);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGo.transform, false);
            var faRect = fillArea.AddComponent<RectTransform>();
            faRect.anchorMin = new Vector2(0f, 0f);
            faRect.anchorMax = new Vector2(1f, 1f);
            faRect.offsetMin = new Vector2(6f, 6f);
            faRect.offsetMax = new Vector2(-18f, -6f);
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillArea.transform, false);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.65f, 0.35f, 1f);

            var hsa = new GameObject("Handle Slide Area");
            hsa.transform.SetParent(sliderGo.transform, false);
            var hsaRect = hsa.AddComponent<RectTransform>();
            hsaRect.anchorMin = Vector2.zero;
            hsaRect.anchorMax = Vector2.one;
            hsaRect.offsetMin = new Vector2(12f, 0f);
            hsaRect.offsetMax = new Vector2(-12f, 0f);
            var handle = new GameObject("Handle");
            handle.transform.SetParent(hsa.transform, false);
            var handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(24f, 0f);
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;

            var slider = sliderGo.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = initValue;
            slider.onValueChanged.AddListener(onChange);
            return slider;
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
            return t;
        }

        private static void MakeInlineLabel(GameObject btn, string content, int size)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(btn.transform, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                     ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = size;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.text = content;
            t.raycastTarget = false;
        }

        private void OnDestroy()
        {
            if (runtimeCanvas != null)
                Destroy(runtimeCanvas.gameObject);
        }
    }
}
