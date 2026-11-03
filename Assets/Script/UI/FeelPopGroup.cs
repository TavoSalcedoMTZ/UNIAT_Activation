using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

//Pone sus objetivos en escala 0 y reproduce un MMF_Player que los hace aparecer con POP (uno tras otro)
public class FeelPopGroup : MonoBehaviour
{
    public MMF_Player popFeedback;
    public List<Transform> targets = new();
    [Tooltip("Reproduce el POP cada vez que el objeto se activa, util para paneles que se prenden con SetActive")]
    public bool playOnEnable = true;

    private bool pendingPlay;

    private void OnEnable()
    {
        if (playOnEnable || pendingPlay)
        {
            pendingPlay = false;
            HideTargets();
            StartCoroutine(PlayNextFrame());
        }
    }

    public void Play()
    {
        HideTargets();
        //Si el objeto esta apagado esperamos a que se prenda para reproducir
        if (!isActiveAndEnabled)
        {
            pendingPlay = true;
            return;
        }
        StartCoroutine(PlayNextFrame());
    }

    private void HideTargets()
    {
        foreach (Transform target in targets)
        {
            if (target != null) target.localScale = Vector3.zero;
        }
    }

    //Esperamos un frame para que el MMF_Player ya este inicializado
    private IEnumerator PlayNextFrame()
    {
        yield return null;
        if (popFeedback == null)
        {
            foreach (Transform target in targets)
            {
                if (target != null) target.localScale = Vector3.one;
            }
            yield break;
        }
        popFeedback.StopFeedbacks();
        popFeedback.PlayFeedbacks();
    }
}
