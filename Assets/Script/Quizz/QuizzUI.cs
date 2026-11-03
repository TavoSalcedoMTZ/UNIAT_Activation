using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizzUI : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public Button trueBT;
    public Button falseBT;

    [Header("Feel")]
    public TMP_Text progressText;
    [Tooltip("POP de la caja de pregunta cada vez que cambia")]
    public FeelPopGroup questionPop;


    public void InitQuestion(quizzQuestion _q, int _index, int _total)
    {
        SetUIElements(_q);

        if (progressText != null)
        {
            progressText.text = $"PREGUNTA {_index + 1} / {_total}";
        }
        if (questionPop != null)
        {
            questionPop.Play();
        }
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
