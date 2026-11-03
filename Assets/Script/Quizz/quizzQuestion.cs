using UnityEngine;

[CreateAssetMenu(fileName = "quizzQuestion", menuName = "Scriptable Objects/quizzQuestion")]
public class quizzQuestion : ScriptableObject
{
    [TextArea(3, 10)]
    public string questionText;
    public quizzAnswer TrueAnswer;
    public quizzAnswer FalseAnswer;

    public string GetQuestionText(string _name)
    {
        return questionText.Replace("{name}", _name);
    }

}
