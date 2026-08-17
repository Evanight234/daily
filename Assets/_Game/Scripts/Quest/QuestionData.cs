using System.Collections.Generic;
using UnityEngine;

public enum QuestionType
{
    MultipleChoice,
    ComplexChoice,
    TrueFalse
}

[System.Serializable]
public class QuestionData
{
    public QuestionType type;
    [TextArea(2, 4)]
    public string questionText;
    public List<string> options = new List<string>();
    public int correctIndex;
    public List<int> correctIndices = new List<int>();
    public bool correctAnswer;
    [TextArea(1, 2)]
    public string explanation;

    public int CalculateScore(List<int> selected)
    {
        switch (type)
        {
            case QuestionType.MultipleChoice:
                return selected.Contains(correctIndex) ? 1 : 0;
            case QuestionType.ComplexChoice:
                if (selected.Count == 0) return 0;
                int correctCount = 0;
                int wrongCount = 0;
                for (int i = 0; i < options.Count; i++)
                {
                    if (correctIndices.Contains(i))
                    {
                        if (selected.Contains(i)) correctCount++;
                        else wrongCount++;
                    }
                    else
                    {
                        if (selected.Contains(i)) wrongCount++;
                    }
                }
                if (wrongCount == 0 && correctCount > 0) return 2;
                if (correctCount > 0) return 1;
                return 0;
            case QuestionType.TrueFalse:
                return selected.Count > 0 && selected[0] == 0 ? (correctAnswer ? 2 : 0) : (correctAnswer ? 0 : 2);
            default:
                return 0;
        }
    }

    public List<int> GetDefaultSelected()
    {
        return new List<int>();
    }
}
