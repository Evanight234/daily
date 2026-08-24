using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject quitPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button quitYesButton;
    [SerializeField] private Button quitNoButton;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider voiceSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown languageDropdown;

    private void Awake()
    {
        if (settingsPanel == null)
            Debug.LogWarning("[MainMenuManager] Settings Panel belum diisi di Inspector.");
        if (quitPanel == null)
            Debug.LogWarning("[MainMenuManager] Quit Panel belum diisi di Inspector.");
        if (settingsButton == null)
            Debug.LogWarning("[MainMenuManager] Settings Button belum diisi di Inspector.");
        if (quitButton == null)
            Debug.LogWarning("[MainMenuManager] Quit Button belum diisi di Inspector.");
        if (closeButton == null)
            Debug.LogWarning("[MainMenuManager] Close Button belum diisi di Inspector.");
        if (quitYesButton == null)
            Debug.LogWarning("[MainMenuManager] Quit Yes Button belum diisi di Inspector.");
        if (quitNoButton == null)
            Debug.LogWarning("[MainMenuManager] Quit No Button belum diisi di Inspector.");
        if (fullscreenToggle == null)
            Debug.LogWarning("[MainMenuManager] Fullscreen Toggle belum diisi di Inspector.");
        if (languageDropdown == null)
            Debug.LogWarning("[MainMenuManager] Language Dropdown belum diisi di Inspector.");

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (quitPanel != null)
            quitPanel.SetActive(false);

        if (playButton != null) playButton.onClick.AddListener(NewGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (quitButton != null) quitButton.onClick.AddListener(ShowQuitPopup);
        if (closeButton != null) closeButton.onClick.AddListener(CloseSettings);
        if (quitYesButton != null) quitYesButton.onClick.AddListener(QuitYes);
        if (quitNoButton != null) quitNoButton.onClick.AddListener(QuitNo);

        if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        if (voiceSlider != null) voiceSlider.onValueChanged.AddListener(OnVoiceChanged);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        if (languageDropdown != null) languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void Start()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        float voice = PlayerPrefs.GetFloat("VoiceVolume", 0.8f);
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        int language = PlayerPrefs.GetInt("Language", 0);

        if (masterSlider != null) masterSlider.SetValueWithoutNotify(master);
        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(bgm);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfx);
        if (voiceSlider != null) voiceSlider.SetValueWithoutNotify(voice);
        if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
        if (languageDropdown != null) languageDropdown.SetValueWithoutNotify(language);

        AudioListener.volume = master;
        Screen.fullScreen = fullscreen;
    }

    public void NewGame()
    {
        Debug.Log("[MainMenuManager] New Game belum tersedia. Save System menyusul.");
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ShowQuitPopup()
    {
        if (quitPanel != null)
            quitPanel.SetActive(true);
    }

    public void QuitYes()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void QuitNo()
    {
        if (quitPanel != null)
            quitPanel.SetActive(false);
    }

    public void OnMasterChanged(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        AudioListener.volume = value;
    }

    public void OnBGMChanged(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    public void OnSFXChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void OnVoiceChanged(float value)
    {
        PlayerPrefs.SetFloat("VoiceVolume", value);
    }

    public void OnFullscreenChanged(bool isOn)
    {
        Screen.fullScreen = isOn;
        Debug.Log($"[MainMenuManager] Fullscreen -> {isOn} | nilai aktif: {Screen.fullScreen}");
        PlayerPrefs.SetInt("Fullscreen", isOn ? 1 : 0);
    }

    public void OnLanguageChanged(int index)
    {
        PlayerPrefs.SetInt("Language", index);
    }
}
