using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Daily/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea(2, 4)]
    public string description;
    public DialogData introDialog;
    public List<QuestionData> questions = new List<QuestionData>();
    public DialogData completionDialog;
    public int scoreReward;
    public UnityEvent onComplete;
    public UnityEvent onFail;
}
