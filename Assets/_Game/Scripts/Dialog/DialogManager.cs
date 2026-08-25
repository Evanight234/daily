using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] DialogData dialogData;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text dialogText;
    [SerializeField] Image backgroundImage;
    [SerializeField] AudioSource voiceSource;
    [SerializeField] AudioSource musicSource;
    [SerializeField] GameObject dialogBox;
    [SerializeField] Button skipButton;
    [SerializeField] float charsPerSecond = 40f;
    [SerializeField] bool playOnStart = true;

    int _index = -1;
    bool _typing;
    Coroutine _typeRoutine;
    string _fullText = "";
    DialogData _currentData;

    public bool IsPlaying { get; private set; }

    void Awake()
    {
        if (voiceSource == null)
            voiceSource = GetComponent<AudioSource>();
        if (voiceSource == null)
            voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipDialog);
    }

    void Start()
    {
        if (playOnStart)
            Play(dialogData);
    }

    void Update()
    {
        if (!IsPlaying)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;
            Advance();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            Advance();
    }

    public void Play(DialogData data)
    {
        if (data == null || data.lines == null || data.lines.Count == 0)
        {
            IsPlaying = false;
            return;
        }

        _currentData = data;
        _index = -1;
        IsPlaying = true;

        if (dialogBox != null)
            dialogBox.SetActive(true);

        if (skipButton != null)
            skipButton.gameObject.SetActive(true);

        Advance();
    }

    public void Advance()
    {
        if (!IsPlaying)
            return;

        if (_typing)
        {
            StopTyping();
            if (dialogText != null)
                dialogText.text = _fullText;
            return;
        }

        _index++;
        if (_currentData == null || _index >= _currentData.lines.Count)
        {
            CompleteDialog();
            return;
        }

        ShowLine(_currentData.lines[_index]);
    }

    public void SkipDialog()
    {
        if (!IsPlaying)
            return;

        if (_typing)
            StopTyping();

        CompleteDialog();
    }

    void ShowLine(DialogLine line)
    {
        if (line == null)
            return;

        if (nameText != null && !string.IsNullOrEmpty(line.speakerName))
            nameText.text = line.speakerName;

        if (backgroundImage != null && line.background != null)
        {
            backgroundImage.sprite = line.background;
            backgroundImage.enabled = true;
            backgroundImage.color = Color.white;
        }

        if (voiceSource != null)
        {
            voiceSource.Stop();
            if (line.voice != null)
            {
                voiceSource.clip = line.voice;
                voiceSource.volume = PlayerPrefs.GetFloat("VoiceVolume", 0.8f);
                voiceSource.Play();
            }
        }

        if (musicSource != null && line.musicBackground != null && line.musicBackground != musicSource.clip)
        {
            musicSource.Stop();
            musicSource.clip = line.musicBackground;
            musicSource.volume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
            musicSource.Play();
        }

        _fullText = line.text ?? "";
        if (_typeRoutine != null)
            StopCoroutine(_typeRoutine);
        _typeRoutine = StartCoroutine(TypeText(_fullText));
    }

    IEnumerator TypeText(string full)
    {
        _typing = true;
        if (dialogText != null)
            dialogText.text = "";

        if (charsPerSecond <= 0f || full.Length == 0)
        {
            if (dialogText != null)
                dialogText.text = full;
            _typing = false;
            _typeRoutine = null;
            yield break;
        }

        float delay = 1f / charsPerSecond;
        for (int i = 0; i < full.Length; i++)
        {
            if (dialogText != null)
                dialogText.text = full.Substring(0, i + 1);
            yield return new WaitForSeconds(delay);
        }

        _typing = false;
        _typeRoutine = null;
    }

    void StopTyping()
    {
        if (_typeRoutine != null)
        {
            StopCoroutine(_typeRoutine);
            _typeRoutine = null;
        }
        _typing = false;
    }

    void CompleteDialog()
    {
        IsPlaying = false;

        if (dialogBox != null)
            dialogBox.SetActive(false);

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        if (nameText != null)
            nameText.text = "";
        if (dialogText != null)
            dialogText.text = "";
        if (backgroundImage != null)
            backgroundImage.enabled = false;

        if (musicSource != null)
            musicSource.Stop();

        if (_currentData != null)
        {
            _currentData.onComplete?.Invoke();

            if (_currentData.endAction == DialogEndAction.LoadScene)
            {
                if (!string.IsNullOrEmpty(_currentData.sceneToLoad))
                    SceneManager.LoadScene(_currentData.sceneToLoad);
                else
                    Debug.LogWarning("[DialogManager] sceneToLoad kosong, tidak bisa load scene.");
            }
        }

        _currentData = null;
    }
}
