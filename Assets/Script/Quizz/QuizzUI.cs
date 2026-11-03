using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizzUI : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public Button trueBT;
    public Button falseBT;



    public void InitQuestion(quizzQuestion _q)
    {
        //Futuramente pondre una corrutina con feel pero ahora seteamos los valores en seco

        SetUIElements(_q);

    }

    public void SetUIElements(quizzQuestion _q)
    {
        ClearUIElements();


        //Si se prueba la escena sin pasar por el registro no hay usuario
        string userName = AppStaticManager.CurrentUser != null ? AppStaticManager.CurrentUser.userName : "Explorador";
        questionText.text = _q.GetQuestionText(userName);

        trueBT.onClick.AddListener(() =>
        {
           _q.TrueAnswer.AddAnswerScore(AppStaticManager.CurrentScore);
        });

        falseBT.onClick.AddListener(() =>
        {
            _q.FalseAnswer.AddAnswerScore(AppStaticManager.CurrentScore);
        });
    }

    public void ClearUIElements()
    {
        questionText.text = "";
        trueBT.onClick.RemoveAllListeners();
        falseBT.onClick.RemoveAllListeners();
    }

}
