using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    public static event System.Action<float> onMasterChanged;
    public static event System.Action<float> onBGMChanged;
    public static event System.Action<float> onSFXChanged;
    public static event System.Action<float> onVoiceChanged;

    [SerializeField] private GameObject panel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider voiceSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private TMP_Text masterPercent;
    [SerializeField] private TMP_Text bgmPercent;
    [SerializeField] private TMP_Text sfxPercent;
    [SerializeField] private TMP_Text voicePercent;

    Button _langIdButton;
    Button _langEnButton;
    LocalizationManager _subscribedManager;
    CanvasGroup _overlayCanvasGroup;

    static readonly Color SliderBgColor = new Color(0.25f, 0.25f, 0.3f, 1f);
    static readonly Color SliderFillColor = new Color(0.2f, 0.65f, 0.3f, 1f);
    static readonly Color HandleColor = new Color(0.85f, 0.85f, 0.9f, 1f);
    static readonly Color LangOnColor = new Color(0.16f, 0.55f, 0.25f, 0.95f);
    static readonly Color LangOffColor = new Color(0.22f, 0.22f, 0.28f, 0.9f);
    static readonly Color ButtonColor = new Color(0.18f, 0.18f, 0.24f, 0.95f);

    private void Awake()
    {
        if (panel == null)
            CreateSettingsUI();

        if (panel == null)
        {
            Debug.LogWarning("[SettingsPopup] Panel belum diisi di Inspector dan gagal dibuat otomatis.");
            return;
        }

        _overlayCanvasGroup = panel.GetComponent<CanvasGroup>();
        if (_overlayCanvasGroup == null)
            _overlayCanvasGroup = panel.AddComponent<CanvasGroup>();

        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        if (voiceSlider != null) voiceSlider.onValueChanged.AddListener(OnVoiceChanged);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        if (languageDropdown != null) languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

        LoadSettings();
        HidePanel();
    }

    void HidePanel()
    {
        if (_overlayCanvasGroup != null)
        {
            _overlayCanvasGroup.alpha = 0f;
            _overlayCanvasGroup.blocksRaycasts = false;
            _overlayCanvasGroup.interactable = false;
        }
    }

    private void LoadSettings()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        float voice = PlayerPrefs.GetFloat("VoiceVolume", 0.8f);
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        if (masterSlider != null) masterSlider.SetValueWithoutNotify(master);
        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(bgm);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfx);
        if (voiceSlider != null) voiceSlider.SetValueWithoutNotify(voice);
        if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
        if (languageDropdown != null) languageDropdown.SetValueWithoutNotify(PlayerPrefs.GetInt("Language", 0));

        UpdatePercentText(masterPercent, master);
        UpdatePercentText(bgmPercent, bgm);
        UpdatePercentText(sfxPercent, sfx);
        UpdatePercentText(voicePercent, voice);

        AudioListener.volume = master;
        Screen.fullScreen = fullscreen;

        if (_langIdButton != null || _langEnButton != null)
        {
            _subscribedManager = LocalizationManager.Instance;
            _subscribedManager.onLanguageChanged += RefreshLanguageButtons;
            RefreshLanguageButtons();
        }
    }

    private void OnDestroy()
    {
        if (_subscribedManager != null)
            _subscribedManager.onLanguageChanged -= RefreshLanguageButtons;
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    static void UpdatePercentText(TMP_Text label, float value)
    {
        if (label != null)
            label.text = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f) + "%";
    }

    public void Open()
    {
        if (panel != null && _overlayCanvasGroup != null)
        {
            panel.transform.SetAsLastSibling();
            _overlayCanvasGroup.alpha = 1f;
            _overlayCanvasGroup.blocksRaycasts = true;
            _overlayCanvasGroup.interactable = true;
        }
    }

    public void Close()
    {
        HidePanel();
    }

    public void OnMasterChanged(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        AudioListener.volume = value;
        UpdatePercentText(masterPercent, value);
        PlayerPrefs.Save();
        onMasterChanged?.Invoke(value);
    }

    public void OnBGMChanged(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
        UpdatePercentText(bgmPercent, value);
        PlayerPrefs.Save();
        onBGMChanged?.Invoke(value);
    }

    public void OnSFXChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        UpdatePercentText(sfxPercent, value);
        PlayerPrefs.Save();
        onSFXChanged?.Invoke(value);
    }

    public void OnVoiceChanged(float value)
    {
        PlayerPrefs.SetFloat("VoiceVolume", value);
        UpdatePercentText(voicePercent, value);
        PlayerPrefs.Save();
        onVoiceChanged?.Invoke(value);
    }

    public void OnFullscreenChanged(bool isOn)
    {
        Screen.fullScreen = isOn;
        PlayerPrefs.SetInt("Fullscreen", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnLanguageChanged(int index)
    {
        LocalizationManager.Instance.SetLanguage(index);
    }

    public void SelectLanguage(int index)
    {
        LocalizationManager.Instance.SetLanguage(index);
        RefreshLanguageButtons();
    }

    void RefreshLanguageButtons()
    {
        int lang = PlayerPrefs.GetInt("Language", 0);
        if (_langIdButton != null)
            _langIdButton.image.color = lang == 0 ? LangOnColor : LangOffColor;
        if (_langEnButton != null)
            _langEnButton.image.color = lang == 1 ? LangOnColor : LangOffColor;
    }

    bool CreateSettingsUI()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>(true);
        if (canvas == null)
            return false;

        var overlayGO = new GameObject("SettingsOverlay_Auto", typeof(RectTransform), typeof(Image));
        overlayGO.transform.SetParent(canvas.transform, false);
        overlayGO.transform.SetAsLastSibling();
        var overlayRect = (RectTransform)overlayGO.transform;
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        panel = overlayGO;

        var windowGO = new GameObject("Window", typeof(RectTransform), typeof(Image),
            typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        windowGO.transform.SetParent(overlayGO.transform, false);
        var windowRect = (RectTransform)windowGO.transform;
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.pivot = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(540f, 0f);
        windowGO.GetComponent<Image>().color = new Color(0.11f, 0.11f, 0.15f, 0.97f);

        var vLayout = windowGO.GetComponent<VerticalLayoutGroup>();
        vLayout.padding = new RectOffset(24, 24, 20, 20);
        vLayout.spacing = 14;
        vLayout.childAlignment = TextAnchor.UpperCenter;
        vLayout.childControlWidth = false;
        vLayout.childControlHeight = false;
        vLayout.childForceExpandWidth = false;
        vLayout.childForceExpandHeight = false;

        var fitter = windowGO.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var title = CreateText(windowGO.transform, "Title", 26, FontStyles.Bold,
            TextAlignmentOptions.Center, Color.white);
        AddLocalized(title.gameObject, "settings.title");
        var titleLe = title.gameObject.AddComponent<LayoutElement>();
        titleLe.minWidth = 492;
        titleLe.preferredHeight = 40;

        masterSlider = CreateSliderRow(windowGO.transform, "MasterRow", "settings.master", out masterPercent);
        bgmSlider = CreateSliderRow(windowGO.transform, "BGMRow", "settings.bgm", out bgmPercent);
        sfxSlider = CreateSliderRow(windowGO.transform, "SFXRow", "settings.sfx", out sfxPercent);
        voiceSlider = CreateSliderRow(windowGO.transform, "VoiceRow", "settings.voice", out voicePercent);

        var fsRow = CreateRow(windowGO.transform, "FullscreenRow");
        var fsLabel = CreateText(fsRow.transform, "Label", 20, FontStyles.Normal,
            TextAlignmentOptions.Left, Color.white);
        AddLocalized(fsLabel.gameObject, "settings.fullscreen");
        var fsLabelLe = fsLabel.gameObject.AddComponent<LayoutElement>();
        fsLabelLe.flexibleWidth = 1f;
        fsLabelLe.preferredHeight = 30f;
        fullscreenToggle = CreateToggle(fsRow.transform);

        var langRow = CreateRow(windowGO.transform, "LanguageRow");
        var langLabel = CreateText(langRow.transform, "Label", 20, FontStyles.Normal,
            TextAlignmentOptions.Left, Color.white);
        AddLocalized(langLabel.gameObject, "settings.language");
        var langLabelLe = langLabel.gameObject.AddComponent<LayoutElement>();
        langLabelLe.flexibleWidth = 1f;
        langLabelLe.preferredHeight = 30f;
        _langIdButton = CreateSmallButton(langRow.transform, "IDButton", "ID");
        _langEnButton = CreateSmallButton(langRow.transform, "ENButton", "EN");
        int idIndex = 0;
        int enIndex = 1;
        _langIdButton.onClick.AddListener(() => SelectLanguage(idIndex));
        _langEnButton.onClick.AddListener(() => SelectLanguage(enIndex));

        var closeBtn = CreateWideButton(windowGO.transform, "CloseButton");
        AddLocalized(closeBtn.text.gameObject, "settings.close");
        closeButton = closeBtn.button;

        return true;
    }

    static GameObject CreateRow(Transform parent, string name)
    {
        var rowGO = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        rowGO.transform.SetParent(parent, false);
        var le = rowGO.AddComponent<LayoutElement>();
        le.preferredHeight = 34f;

        var hLayout = rowGO.GetComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 10f;
        hLayout.childAlignment = TextAnchor.MiddleLeft;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = false;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = false;
        return rowGO;
    }

    Slider CreateSliderRow(Transform parent, string rowName, string labelKey, out TMP_Text percentText)
    {
        var row = CreateRow(parent, rowName).transform;

        var label = CreateText(row, "Label", 20, FontStyles.Normal,
            TextAlignmentOptions.Left, Color.white);
        AddLocalized(label.gameObject, labelKey);
        var labelLe = label.gameObject.AddComponent<LayoutElement>();
        labelLe.preferredWidth = 140f;
        labelLe.preferredHeight = 30f;

        var sliderGO = new GameObject("Slider", typeof(RectTransform));
        sliderGO.transform.SetParent(row, false);
        var sliderLe = sliderGO.AddComponent<LayoutElement>();
        sliderLe.flexibleWidth = 1f;
        sliderLe.preferredHeight = 26f;

        var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(sliderGO.transform, false);
        var bgRect = (RectTransform)bg.transform;
        bgRect.anchorMin = new Vector2(0f, 0.35f);
        bgRect.anchorMax = new Vector2(1f, 0.65f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = SliderBgColor;

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        var fillAreaRect = (RectTransform)fillArea.transform;
        fillAreaRect.anchorMin = new Vector2(0f, 0.35f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.65f);
        fillAreaRect.offsetMin = new Vector2(6f, 0f);
        fillAreaRect.offsetMax = new Vector2(-6f, 0f);

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        var fillRect = (RectTransform)fill.transform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = SliderFillColor;

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderGO.transform, false);
        var handleAreaRect = (RectTransform)handleArea.transform;
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(8f, 0f);
        handleAreaRect.offsetMax = new Vector2(-8f, 0f);

        var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        var handleRect = (RectTransform)handle.transform;
        handleRect.sizeDelta = new Vector2(16f, 26f);
        handle.GetComponent<Image>().color = HandleColor;

        var slider = sliderGO.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;

        percentText = CreateText(row, "Percent", 20, FontStyles.Bold,
            TextAlignmentOptions.Right, Color.white);
        var pctLe = percentText.gameObject.AddComponent<LayoutElement>();
        pctLe.preferredWidth = 56f;
        pctLe.preferredHeight = 30f;

        return slider;
    }

    static Toggle CreateToggle(Transform parent)
    {
        var go = new GameObject("Toggle", typeof(RectTransform), typeof(Image), typeof(Toggle));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.sizeDelta = new Vector2(36f, 26f);
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 36f;
        le.preferredHeight = 26f;

        var bg = go.GetComponent<Image>();
        bg.color = LangOffColor;

        var check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        check.transform.SetParent(go.transform, false);
        var checkRect = (RectTransform)check.transform;
        checkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkRect.sizeDelta = new Vector2(20f, 20f);
        var checkImg = check.GetComponent<Image>();
        checkImg.color = SliderFillColor;

        var toggle = go.GetComponent<Toggle>();
        toggle.targetGraphic = bg;
        toggle.graphic = checkImg;
        return toggle;
    }

    static (Button button, TMP_Text text) CreateWideButton(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = ButtonColor;
        var le = go.AddComponent<LayoutElement>();
        le.minWidth = 492f;
        le.preferredHeight = 46f;

        var text = CreateText(go.transform, "Text", 20, FontStyles.Bold,
            TextAlignmentOptions.Center, Color.white);
        StretchFull((RectTransform)text.transform);
        return (go.GetComponent<Button>(), text);
    }

    static Button CreateSmallButton(Transform parent, string name, string caption)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = LangOffColor;
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 56f;
        le.preferredHeight = 30f;

        var text = CreateText(go.transform, "Text", 18, FontStyles.Bold,
            TextAlignmentOptions.Center, Color.white);
        text.text = caption;
        StretchFull((RectTransform)text.transform);
        return go.GetComponent<Button>();
    }

    static TMP_Text CreateText(Transform parent, string name, float size, FontStyles style,
        TextAlignmentOptions alignment, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void AddLocalized(GameObject go, string key)
    {
        var lt = go.AddComponent<LocalizedText>();
        lt.SetKey(key);
    }
}
