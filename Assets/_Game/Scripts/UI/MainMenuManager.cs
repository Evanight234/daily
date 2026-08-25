using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject quitPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button quitYesButton;
    [SerializeField] private Button quitNoButton;
    [SerializeField] private SettingsPopup settingsPopup;

    private void Awake()
    {
        if (quitPanel == null)
            Debug.LogWarning("[MainMenuManager] Quit Panel belum diisi di Inspector.");
        if (settingsPopup == null)
            Debug.LogWarning("[MainMenuManager] Settings Popup belum diisi di Inspector.");

        if (quitPanel != null)
            quitPanel.SetActive(false);

        if (playButton != null) playButton.onClick.AddListener(NewGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (quitButton != null) quitButton.onClick.AddListener(ShowQuitPopup);
        if (quitYesButton != null) quitYesButton.onClick.AddListener(QuitYes);
        if (quitNoButton != null) quitNoButton.onClick.AddListener(QuitNo);
    }

    public void NewGame()
    {
        Debug.Log("[MainMenuManager] New Game belum tersedia. Save System menyusul.");
    }

    public void OpenSettings()
    {
        if (settingsPopup != null)
            settingsPopup.Open();
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
}
