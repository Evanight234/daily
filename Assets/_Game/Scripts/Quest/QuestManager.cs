using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Settings")]
    public QuestData currentQuest;
    public int totalScore = 0;
    public int completedQuests = 0;
    public int failedQuests = 0;

    [Header("UI References")]
    [SerializeField] GameObject questAcceptPanel;
    [SerializeField] GameObject questionPanel;
    [SerializeField] GameObject questResultPanel;
    [SerializeField] GameObject questLogPanel;
    [SerializeField] TMP_Text questNameText;
    [SerializeField] TMP_Text questDescText;
    [SerializeField] TMP_Text questionTextText;
    [SerializeField] Transform optionsContainer;
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] GameObject questLogEntryPrefab;

    int _questionIndex = 0;
    int _questScore = 0;
    bool _isAccepting = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleQuestLog();
    }

    public void StartQuest(QuestData quest)
    {
        currentQuest = quest;
        _questionIndex = 0;
        _questScore = 0;
        _isAccepting = true;

        questAcceptPanel.SetActive(true);
        questNameText.text = quest.questName;
        questDescText.text = quest.description;
    }

    public void AcceptQuest()
    {
        _isAccepting = false;
        questAcceptPanel.SetActive(false);
        ShowQuestion();
    }

    public void RejectQuest()
    {
        questAcceptPanel.SetActive(false);
        if (currentQuest.onFail != null)
            currentQuest.onFail.Invoke();
    }

    void ShowQuestion()
    {
        if (_questionIndex >= currentQuest.questions.Count)
        {
            CompleteQuest();
            return;
        }

        var q = currentQuest.questions[_questionIndex];
        questionPanel.SetActive(true);
        questionTextText.text = $"{_questionIndex + 1}. {q.questionText}";

        ClearOptions();

        if (q.type == QuestionType.TrueFalse)
        {
            CreateButton("Benar", 0);
            CreateButton("Salah", 1);
        }
        else
        {
            for (int i = 0; i < q.options.Count; i++)
            {
                CreateButton(q.options[i], i);
            }
        }
    }

    void CreateButton(string text, int index)
    {
        var btn = Instantiate(questLogEntryPrefab, optionsContainer);
        var btnText = btn.GetComponentInChildren<TMP_Text>();
        if (btnText != null)
            btnText.text = text;

        var button = btn.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => AnswerQuestion(index));
        }
    }

    void ClearOptions()
    {
        while (optionsContainer.childCount > 0)
        {
            Destroy(optionsContainer.GetChild(0).gameObject);
        }
    }

    public void AnswerQuestion(int answer)
    {
        var q = currentQuest.questions[_questionIndex];
        var selected = new List<int> { answer };
        _questScore += q.CalculateScore(selected);

        questionPanel.SetActive(false);
        _questionIndex++;

        if (_questionIndex >= currentQuest.questions.Count)
        {
            CompleteQuest();
        }
        else
        {
            ShowQuestion();
        }
    }

    public void CompleteQuest()
    {
        questResultPanel.SetActive(true);
        resultText.text = $"Quest Selesai!";
        scoreText.text = $"Skor: {_questScore} / {currentQuest.questions.Count * 2}";

        totalScore += _questScore;
        completedQuests++;

        if (_questScore >= currentQuest.scoreReward)
        {
            if (currentQuest.onComplete != null)
                currentQuest.onComplete.Invoke();
        }
        else
        {
            if (currentQuest.onFail != null)
                currentQuest.onFail.Invoke();
        }
    }

    public void ToggleQuestLog()
    {
        if (questLogPanel.activeSelf)
            questLogPanel.SetActive(false);
        else
            questLogPanel.SetActive(true);
    }

    public void BackToDialogue()
    {
        questResultPanel.SetActive(false);
        if (currentQuest.completionDialog != null)
        {
            var dm = FindObjectOfType<DialogManager>();
            if (dm != null)
                dm.Play(currentQuest.completionDialog);
        }
    }
}
