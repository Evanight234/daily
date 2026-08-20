using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogOption
{
    public string text;
    public bool isCorrect;
}

[Serializable]
public class DialogQuestLine
{
    public string speakerName;
    [TextArea(2, 4)]
    public string questionText;
    public DialogOption[] options;
    public AudioClip voice;
    public AudioClip musicBackground;
    public Sprite background;
}

[CreateAssetMenu(fileName = "NewDialogQuest", menuName = "Daily/Dialog Quest")]
public class DialogQuestData : ScriptableObject
{
    public List<DialogQuestLine> questions = new List<DialogQuestLine>();

    [Header("Setelah quest selesai")]
    public DialogEndAction endAction = DialogEndAction.None;
    public string sceneToLoad;
    public UnityEvent onComplete;
}
