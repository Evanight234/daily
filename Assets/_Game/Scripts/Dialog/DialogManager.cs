using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] AudioSource musicSource;
    [SerializeField] GameObject dialogBox;
    [SerializeField] Button skipButton;
    [SerializeField] Transform optionsContainer;
    [SerializeField] TMP_Text feedbackText;
    [SerializeField] GameObject optionButtonPrefab;
    [SerializeField] float charsPerSecond = 40f;
    [SerializeField, Range(0f, 1f)] float musicVolume = 0.5f;
    [SerializeField] bool playOnStart = true;

    int _index = -1;
    bool _typing;
    Coroutine _typeRoutine;
    string _fullText = "";
    DialogData _currentData;
    bool _waitingAnswer;
    DialogLine _currentQuestion;
    List<Button> _answerButtons = new List<Button>();

    public bool IsPlaying { get; private set; }

    void Awake()
    {
        if (voiceSource == null)
            voiceSource = GetComponent<AudioSource>();
        if (voiceSource == null)
            voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();
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
        if (_waitingAnswer)
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

        if (skipButton != null)
            skipButton.gameObject.SetActive(true);

        Advance();
    }

    public void Advance()
    {
        if (!IsPlaying || _waitingAnswer)
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

    public void SkipDialog()
    {
        if (!IsPlaying)
            return;

        if (_typing)
            StopTyping();

        if (_waitingAnswer)
        {
            _waitingAnswer = false;
            ClearAnswerButtons();
            feedbackText.text = "";
            Advance();
            return;
        }

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

        if (line.isQuestion && line.options != null && line.options.Length > 0)
        {
            ShowQuestion(line);
            return;
        }

        voiceSource.Stop();
        if (line.voice != null)
        {
            voiceSource.clip = line.voice;
            voiceSource.Play();
        }

        if (line.musicBackground != null && line.musicBackground != musicSource.clip)
        {
            musicSource.Stop();
            musicSource.clip = line.musicBackground;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        _fullText = line.text ?? "";
        if (_typeRoutine != null)
            StopCoroutine(_typeRoutine);
        _typeRoutine = StartCoroutine(TypeText(_fullText));
    }

    void ShowQuestion(DialogLine line)
    {
        _waitingAnswer = true;
        _currentQuestion = line;
        feedbackText.text = "";

        if (nameText != null)
            nameText.text = line.speakerName;

        if (dialogText != null)
            dialogText.text = line.questionText;

        ClearAnswerButtons();

        for (int i = 0; i < line.options.Length; i++)
        {
            var btn = Instantiate(optionButtonPrefab, optionsContainer);
            var btnText = btn.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = line.options[i];

            var button = btn.GetComponent<Button>();
            if (button != null)
            {
                int index = i;
                button.onClick.AddListener(() => OnAnswer(index));
                _answerButtons.Add(button);
            }
        }
    }

    void OnAnswer(int index)
    {
        if (!_waitingAnswer)
            return;

        ClearAnswerButtons();
        _waitingAnswer = false;

        bool isCorrect = index == _currentQuestion.correctIndex;
        string feedback = isCorrect ? "Benar!" : "Salah! Jawaban: " + _currentQuestion.options[_currentQuestion.correctIndex];
        Color feedbackColor = isCorrect ? Color.green : Color.red;

        if (feedbackText != null)
        {
            feedbackText.text = feedback;
            feedbackText.color = feedbackColor;
        }

        StartCoroutine(AdvanceAfterFeedback());
    }

    IEnumerator AdvanceAfterFeedback()
    {
        yield return new WaitForSeconds(2f);
        feedbackText.text = "";
        Advance();
    }

    void ClearAnswerButtons()
    {
        for (int i = 0; i < _answerButtons.Count; i++)
        {
            if (_answerButtons[i] != null)
                Destroy(_answerButtons[i].gameObject);
        }
        _answerButtons.Clear();
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
        ClearAnswerButtons();

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
