using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] DialogData dialogData;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text dialogText;
    [SerializeField] Image backgroundImage;
    [SerializeField] AudioSource voiceSource;
    [SerializeField] GameObject dialogBox;
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
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
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
        if (_index >= _currentData.lines.Count)
        {
            CompleteDialog();
            return;
        }

        ShowLine(_currentData.lines[_index]);
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

        voiceSource.Stop();
        if (line.voice != null)
        {
            voiceSource.clip = line.voice;
            voiceSource.Play();
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

        if (nameText != null)
            nameText.text = "";
        if (dialogText != null)
            dialogText.text = "";
        if (backgroundImage != null)
            backgroundImage.enabled = false;

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
