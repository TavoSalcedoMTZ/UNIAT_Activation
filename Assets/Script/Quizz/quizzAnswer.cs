using UnityEngine;
using System.Collections.Generic;

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

    public int GetScore(CareerType _career)
    {
        return _career switch
        {
            CareerType.IPV => IPV,
            CareerType.LAV => LAV,
            CareerType.CAD => CAD,
            CareerType.CS => CS,
            _ => 0
        };
    }

    //Regresa la carrera con mas puntos, si hay empate se elige al azar entre las empatadas
    public CareerType GetRecommendedCareer()
    {
        List<CareerType> bestCareers = new();
        int bestScore = int.MinValue;

        foreach (CareerType career in System.Enum.GetValues(typeof(CareerType)))
        {
            int score = GetScore(career);
            if (score > bestScore)
            {
                bestScore = score;
                bestCareers.Clear();
            }
            if (score == bestScore)
            {
                bestCareers.Add(career);
            }
        }

        return bestCareers[Random.Range(0, bestCareers.Count)];
    }

    public override string ToString()
    {
        return $"IPV:{IPV} LAV:{LAV} CAD:{CAD} CS:{CS}";
    }
}
