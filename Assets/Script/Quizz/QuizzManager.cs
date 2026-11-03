using UnityEngine;
using System.Collections.Generic;

public class QuizzManager : MonoBehaviour
{
    public List<quizzQuestion> questions = new();
    [Header("UI")]
    public QuizzUI quizzUI;
    [Header("Resultado")]
    public string recommendationSceneName = "recomendation";
    private int currentQuestionIndex = 0;

    public void Start()
    {
        if (questions == null || questions.Count == 0)
        {
            Debug.LogError("No questions found in QuizzManager.");
            return;
        }
    }

    public void StartQuizz()
    {
        currentQuestionIndex = 0;
        AppStaticManager.ResetScore();
        ShowCurrentQuestion();
    }

    public void ShowCurrentQuestion()
    {
        if (currentQuestionIndex < questions.Count)
        {
            quizzUI.InitQuestion(questions[currentQuestionIndex]);
            quizzUI.trueBT.onClick.AddListener(() =>
            {
                NextQuestion();
            }); 

            quizzUI.falseBT.onClick.AddListener(() =>
            {
                NextQuestion();
            });
        }
        else
        {
            FinishQuizz();
        }
    }

    private void FinishQuizz()
    {
        //Quitamos los listeners para que clics extra ya no sumen puntos
        quizzUI.ClearUIElements();

        evaluationValues score = AppStaticManager.CurrentScore;
        CareerType recommended = score.GetRecommendedCareer();
        AppStaticManager.SetRecommendedCareer(recommended);
        Debug.Log($"Quizz finished! [{score}] Carrera recomendada: {recommended.GetDisplayName()}");

        StaticMenu.LoadSceneSafe(recommendationSceneName);
    }

    public void NextQuestion()
    {
        currentQuestionIndex++;
        ShowCurrentQuestion();
    }

}
