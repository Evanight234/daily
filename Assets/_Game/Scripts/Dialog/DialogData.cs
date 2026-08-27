using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum DialogEndAction
{
    None,
    LoadScene
}

[Serializable]
public class DialogLine
{
    public string speakerName;
    [TextArea(2, 6)]
    public string text;
    public AudioClip voice;
    public AudioClip musicBackground;
    public AudioClip sfx;
    public Sprite background;
}

[CreateAssetMenu(fileName = "NewDialog", menuName = "Daily/Dialog Data")]
public class DialogData : ScriptableObject
{
    public List<DialogLine> lines = new List<DialogLine>();

    [Header("Setelah dialog selesai")]
    public DialogEndAction endAction = DialogEndAction.None;
    public string sceneToLoad;
    public UnityEvent onComplete;
}
