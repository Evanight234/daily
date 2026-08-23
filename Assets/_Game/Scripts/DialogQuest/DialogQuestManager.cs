using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogQuestManager : MonoBehaviour
{
    [SerializeField] DialogQuestData questData;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text dialogText;
    [SerializeField] Image backgroundImage;
    [SerializeField] AudioSource voiceSource;
    [SerializeField] AudioSource musicSource;
    [SerializeField] GameObject dialogBox;
    [SerializeField, Range(0f, 1f)] float musicVolume = 0.5f;
    [SerializeField] bool playOnStart = true;
    [SerializeField] Vector2 optionsPosition = new Vector2(0, 100);
    [SerializeField] Vector2 feedbackPosition = new Vector2(0, 230);

    int _index = -1;
    DialogQuestData _currentData;
    DialogQuestLine _currentQuestion;
    bool _waitingAnswer;
    Coroutine _feedbackRoutine;
    List<Button> _answerButtons = new List<Button>();
    List<Image> _answerImages = new List<Image>();
    List<TMP_Text> _answerTexts = new List<TMP_Text>();
    Transform _optionsContainer;
    TextMeshProUGUI _feedbackText;

    static readonly Color OptionDefault = new Color(0.15f, 0.15f, 0.2f, 0.9f);
    static readonly Color OptionCorrect = new Color(0.12f, 0.55f, 0.22f, 0.95f);
    static readonly Color OptionWrong = new Color(0.6f, 0.15f, 0.15f, 0.95f);

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

        CreateQuestionUI();
    }

    void Start()
    {
        if (playOnStart)
            Play(questData);
    }

    void CreateQuestionUI()
    {
        if (dialogBox == null)
            return;

        var canvas = dialogBox.GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        var containerGO = new GameObject("OptionsContainer_" + gameObject.name);
        containerGO.transform.SetParent(canvas.transform, false);
        var rect = containerGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = optionsPosition;
        rect.sizeDelta = new Vector2(600, 120);

        var layout = containerGO.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 10;
        layout.childControlHeight = false;
        layout.childControlWidth = false;

        var fitter = containerGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        _optionsContainer = rect;

        var feedbackGO = new GameObject("FeedbackText_" + gameObject.name);
        feedbackGO.transform.SetParent(canvas.transform, false);
        var feedbackRect = feedbackGO.AddComponent<RectTransform>();
        feedbackRect.anchorMin = new Vector2(0.5f, 0f);
        feedbackRect.anchorMax = new Vector2(0.5f, 0f);
        feedbackRect.pivot = new Vector2(0.5f, 0.5f);
        feedbackRect.anchoredPosition = feedbackPosition;
        feedbackRect.sizeDelta = new Vector2(500, 40);

        var feedback = feedbackGO.AddComponent<TextMeshProUGUI>();
        feedback.fontSize = 24;
        feedback.alignment = TextAlignmentOptions.Center;
        feedback.color = Color.clear;
        feedback.raycastTarget = false;
        _feedbackText = feedback;
    }

    public void Play(DialogQuestData data)
    {
        if (data == null || data.questions == null || data.questions.Count == 0)
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

    void Advance()
    {
        if (!IsPlaying || _waitingAnswer)
            return;

        _index++;
        if (_currentData == null || _index >= _currentData.questions.Count)
        {
            CompleteQuest();
            return;
        }

        ShowQuestion(_currentData.questions[_index]);
    }

    static bool IsValidQuestion(DialogQuestLine line)
    {
        if (line == null || line.options == null || line.options.Length == 0)
            return false;
        for (int i = 0; i < line.options.Length; i++)
        {
            if (line.options[i] != null && line.options[i].isCorrect)
                return true;
        }
        return false;
    }

    void ShowQuestion(DialogQuestLine line)
    {
        if (_optionsContainer == null || !IsValidQuestion(line))
        {
            Debug.LogWarning("[DialogQuestManager] Question tidak valid, skip.");
            Advance();
            return;
        }

        _waitingAnswer = true;
        _currentQuestion = line;

        if (_feedbackText != null)
        {
            _feedbackText.text = "";
            _feedbackText.color = Color.clear;
        }

        if (nameText != null)
            nameText.text = line.speakerName ?? "";
        if (dialogText != null)
            dialogText.text = line.questionText ?? "";

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
                voiceSource.Play();
            }
        }

        if (musicSource != null && line.musicBackground != null && line.musicBackground != musicSource.clip)
        {
            musicSource.Stop();
            musicSource.clip = line.musicBackground;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        ClearAnswerButtons();

        for (int i = 0; i < line.options.Length; i++)
        {
            var btnGO = new GameObject("OptionButton_" + i);
            btnGO.transform.SetParent(_optionsContainer, false);
            var btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(400, 50);

            var btnLayout = btnGO.AddComponent<LayoutElement>();
            btnLayout.preferredWidth = 400;
            btnLayout.preferredHeight = 50;

            var btnImage = btnGO.AddComponent<Image>();
            btnImage.color = OptionDefault;

            var button = btnGO.AddComponent<Button>();
            int index = i;
            button.onClick.AddListener(() => OnAnswer(index));
            _answerButtons.Add(button);
            _answerImages.Add(btnImage);

            var textGO = new GameObject("OptionButtonText");
            textGO.transform.SetParent(btnGO.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var buttonText = textGO.AddComponent<TextMeshProUGUI>();
            buttonText.raycastTarget = false;
            buttonText.text = line.options[i].text;
            buttonText.fontSize = 20;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            _answerTexts.Add(buttonText);
        }
    }

    void OnAnswer(int index)
    {
        if (!_waitingAnswer || _currentQuestion == null)
            return;
        if (_currentQuestion.options == null || index < 0 || index >= _currentQuestion.options.Length)
            return;

        for (int i = 0; i < _answerButtons.Count; i++)
        {
            if (_answerButtons[i] != null)
                _answerButtons[i].interactable = false;
        }

        bool isCorrect = _currentQuestion.options[index].isCorrect;
        ApplyAnswerVisuals(index);

        string feedback = isCorrect ? "Benar!" : "Salah!";
        if (_feedbackText != null)
        {
            _feedbackText.text = feedback;
            _feedbackText.color = isCorrect ? Color.green : Color.red;
        }

        StopFeedbackRoutine();
        _feedbackRoutine = StartCoroutine(AdvanceAfterFeedback());
    }

    void ApplyAnswerVisuals(int selectedIndex)
    {
        int count = Mathf.Min(_answerImages.Count, _answerTexts.Count);
        if (_currentQuestion.options != null)
            count = Mathf.Min(count, _currentQuestion.options.Length);

        for (int i = 0; i < count; i++)
        {
            bool isCorrectOption = _currentQuestion.options[i].isCorrect;
            bool isSelected = i == selectedIndex;

            if (_answerImages[i] != null)
            {
                if (isCorrectOption)
                    _answerImages[i].color = OptionCorrect;
                else if (isSelected)
                    _answerImages[i].color = OptionWrong;
                else
                    _answerImages[i].color = OptionDefault;
            }

            if (_answerTexts[i] != null)
            {
                string label = _currentQuestion.options[i].text;
                if (isCorrectOption)
                    _answerTexts[i].text = "[V]  " + label;
                else if (isSelected)
                    _answerTexts[i].text = "[X]  " + label;
            }
        }
    }

    IEnumerator AdvanceAfterFeedback()
    {
        yield return new WaitForSeconds(2f);
        _feedbackRoutine = null;
        _waitingAnswer = false;
        ClearAnswerButtons();
        if (_feedbackText != null)
            _feedbackText.text = "";
        Advance();
    }

    void StopFeedbackRoutine()
    {
        if (_feedbackRoutine != null)
        {
            StopCoroutine(_feedbackRoutine);
            _feedbackRoutine = null;
        }
    }

    void ClearAnswerButtons()
    {
        for (int i = 0; i < _answerButtons.Count; i++)
        {
            if (_answerButtons[i] != null)
                Destroy(_answerButtons[i].gameObject);
        }
        _answerButtons.Clear();
        _answerImages.Clear();
        _answerTexts.Clear();
    }

    void CompleteQuest()
    {
        IsPlaying = false;
        StopFeedbackRoutine();
        ClearAnswerButtons();
        _waitingAnswer = false;

        if (dialogBox != null)
            dialogBox.SetActive(false);
        if (nameText != null)
            nameText.text = "";
        if (dialogText != null)
            dialogText.text = "";
        if (backgroundImage != null)
            backgroundImage.enabled = false;
        if (musicSource != null)
            musicSource.Stop();
        if (_feedbackText != null)
            _feedbackText.text = "";

        if (_currentData != null)
        {
            _currentData.onComplete?.Invoke();
            if (_currentData.endAction == DialogEndAction.LoadScene)
            {
                if (!string.IsNullOrEmpty(_currentData.sceneToLoad))
                    SceneManager.LoadScene(_currentData.sceneToLoad);
                else
                    Debug.LogWarning("[DialogQuestManager] sceneToLoad kosong.");
            }
        }

        _currentData = null;
    }
}
