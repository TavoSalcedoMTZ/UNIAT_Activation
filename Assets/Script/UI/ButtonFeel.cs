using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Reproduce un feedback de Feel al presionar el boton (se siente mejor en el pointer down que en el click)
[RequireComponent(typeof(Button))]
public class ButtonFeel : MonoBehaviour, IPointerDownHandler
{
    public MMF_Player pressFeedback;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.interactable && pressFeedback != null)
        {
            pressFeedback.PlayFeedbacks();
        }
    }
}
