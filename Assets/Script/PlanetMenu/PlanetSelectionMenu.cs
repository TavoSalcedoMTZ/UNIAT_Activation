using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

public class PlanetSelectionMenu : MonoBehaviour
{
    public List<PlanetCard> cards = new();

    [Header("Titulo")]
    public TMP_Text titleText;
    public MMF_Player titleFeedback;

    [Header("Tiempos")]
    [Tooltip("Espera inicial para que termine el fade del StaticMenu")]
    public float startDelay = 0.6f;
    public float staggerDelay = 0.15f;

    private IEnumerator Start()
    {
        CareerType? recommended = AppStaticManager.RecommendedCareer;

        foreach (PlanetCard card in cards)
        {
            bool isRecommended = recommended.HasValue && card.world != null && card.world.carrera == recommended.Value;
            card.Setup(isRecommended);
            card.transform.localScale = Vector3.zero;
        }
        titleText.transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(startDelay);

        if (titleFeedback != null)
        {
            titleFeedback.PlayFeedbacks();
        }
        else
        {
            titleText.transform.localScale = Vector3.one;
        }

        foreach (PlanetCard card in cards)
        {
            yield return new WaitForSeconds(staggerDelay);
            card.Appear();
        }
    }
}
