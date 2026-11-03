using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class QuizzManager : MonoBehaviour
{
    public List<quizzQuestion> questions = new();
    [Header("UI")]
    public QuizzUI quizzUI;
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
            Debug.Log("Quizz finished!");
            // Handle end of quiz logic here

        }
    }

    public void NextQuestion()
    {
        currentQuestionIndex++;
        ShowCurrentQuestion();
    }

}
