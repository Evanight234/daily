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
    Transform _optionsContainer;
    TMP_Text _feedbackText;
    Canvas _canvas;

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

        CreateQuestionUI();
    }

    void CreateQuestionUI()
    {
        if (dialogBox == null) return;

        _canvas = dialogBox.GetComponentInParent<Canvas>();
        if (_canvas == null) return;

        string containerName = "OptionsContainer_" + gameObject.name;
        var containerGO = new GameObject(containerName);
        containerGO.transform.SetParent(_canvas.transform, false);
        var rectTransform = containerGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0, 100);
        rectTransform.sizeDelta = new Vector2(600, 120);

        var layoutGroup = containerGO.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = 10;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;

        var contentSizeFitter = containerGO.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _optionsContainer = rectTransform;

        string feedbackName = "FeedbackText_" + gameObject.name;
        var feedbackGO = new GameObject(feedbackName);
        feedbackGO.transform.SetParent(_canvas.transform, false);
        var feedbackRect = feedbackGO.AddComponent<RectTransform>();
        feedbackRect.anchorMin = new Vector2(0.5f, 0f);
        feedbackRect.anchorMax = new Vector2(0.5f, 0f);
        feedbackRect.pivot = new Vector2(0.5f, 0.5f);
        feedbackRect.anchoredPosition = new Vector2(0, -40);
        feedbackRect.sizeDelta = new Vector2(500, 40);

        var feedbackText = feedbackGO.AddComponent<TMP_Text>();
        if (feedbackText != null)
        {
            feedbackText.fontSize = 24;
            feedbackText.alignment = TextAlignmentOptions.Center;
            feedbackText.color = Color.clear;
        }

        _feedbackText = feedbackText;
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
        if (_feedbackText != null)
            _feedbackText.text = "";
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

        if (_feedbackText != null)
            _feedbackText.color = Color.clear;

        if (nameText != null)
            nameText.text = line.speakerName;

        if (dialogText != null)
            dialogText.text = line.questionText;

        ClearAnswerButtons();

        for (int i = 0; i < line.options.Length; i++)
        {
            var btnGO = new GameObject("OptionButton_" + i);
            btnGO.transform.SetParent(_optionsContainer, false);
            var rectTransform = btnGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 50);

            var btnImage = btnGO.AddComponent<Image>();
            btnImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            var button = btnGO.AddComponent<Button>();
            int index = i;
            button.onClick.AddListener(() => OnAnswer(index));
            _answerButtons.Add(button);

            var textGO = new GameObject("OptionButtonText");
            textGO.transform.SetParent(btnGO.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var buttonText = textGO.AddComponent<TextMeshProUGUI>();
            buttonText.text = line.options[i];
            buttonText.fontSize = 20;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
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

        if (_feedbackText != null)
        {
            _feedbackText.text = feedback;
            _feedbackText.color = feedbackColor;
        }

        StartCoroutine(AdvanceAfterFeedback());
    }

    IEnumerator AdvanceAfterFeedback()
    {
        yield return new WaitForSeconds(2f);
        if (_feedbackText != null)
            _feedbackText.text = "";
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
