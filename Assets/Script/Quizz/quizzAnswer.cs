using UnityEngine;

[CreateAssetMenu(fileName = "quizzAnswer", menuName = "Scriptable Objects/quizzAnswer")]
public class quizzAnswer : ScriptableObject
{
    public evaluationValues answerValues;
    public evaluationValues AddAnswerScore(evaluationValues _currentScore)
    {
        _currentScore.IPV += answerValues.IPV;
        _currentScore.LAV += answerValues.LAV;
        _currentScore.CAD += answerValues.CAD;
        _currentScore.CS += answerValues.CS;
        return _currentScore;
    }
    
}

[System.Serializable]
public class evaluationValues
{
    public int IPV;
    public int LAV;
    public int CAD;
    public int CS;
}
