using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecommendationScreen : MonoBehaviour
{
    public List<worldInfo> worlds = new();

    [Header("Textos POP")]
    public TMP_Text popText;
    public List<string> introLines = new() { "NUESTRA", "RECOMENDACIÓN ES" };
    [Tooltip("Espera inicial para que termine el fade del StaticMenu")]
    public float startDelay = 0.7f;
    public float timeBetweenPops = 1.1f;

    [Header("Planeta")]
    public RectTransform planetGroup;
    public Image planetImage;
    public Image planetGlow;
    public TMP_Text careerText;

    [Header("Botones")]
    public CanvasGroup buttonsGroup;
    public Button goToPlanetBT;
    public Button otherPlanetsBT;
    public string planetSelectionSceneName = "3";
    [Tooltip("Tiempo despues de revelar el planeta para poder usar los botones")]
    public float buttonsEnableDelay = 1f;

    [Header("Feel")]
    public MMF_Player popFeedback;
    public MMF_Player planetRevealFeedback;

    [Header("Debug")]
    [Tooltip("Se usa si se abre la escena sin pasar por el quizz")]
    public CareerType previewCareer = CareerType.IPV;

    private worldInfo recommendedWorld;

    private void Start()
    {
        CareerType career = AppStaticManager.RecommendedCareer ?? previewCareer;
        recommendedWorld = worldInfo.FindByCareer(worlds, career);
        if (recommendedWorld == null)
        {
            Debug.LogError($"No hay worldInfo para la carrera {career} en RecommendationScreen.");
            return;
        }

        SetupPlanet(career);

        popText.text = "";
        popText.transform.localScale = Vector3.zero;
        planetGroup.localScale = Vector3.zero;
        SetButtonsActive(false);
        buttonsGroup.alpha = 0f;

        goToPlanetBT.onClick.AddListener(() => StaticMenu.LoadSceneSafe(recommendedWorld.sceneName));
        otherPlanetsBT.onClick.AddListener(() => StaticMenu.LoadSceneSafe(planetSelectionSceneName));

        StartCoroutine(RevealRoutine());
    }

    private void SetupPlanet(CareerType _career)
    {
        if (recommendedWorld.worldImage != null)
        {
            planetImage.sprite = recommendedWorld.worldImage;
            planetImage.color = Color.white;
        }
        else
        {
            planetImage.color = recommendedWorld.planetColor;
        }
        if (planetGlow != null)
        {
            Color glow = recommendedWorld.planetColor;
            glow.a = planetGlow.color.a;
            planetGlow.color = glow;
        }
        careerText.text = _career.GetDisplayName();
    }

    private IEnumerator RevealRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        foreach (string line in introLines)
        {
            Pop(line);
            yield return new WaitForSeconds(timeBetweenPops);
        }

        Pop(recommendedWorld.worldName.ToUpper());
        if (planetRevealFeedback != null)
        {
            planetRevealFeedback.PlayFeedbacks();
        }
        else
        {
            planetGroup.localScale = Vector3.one;
            buttonsGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(buttonsEnableDelay);
        SetButtonsActive(true);
    }

    private void Pop(string _text)
    {
        popText.text = _text;
        if (popFeedback != null)
        {
            popFeedback.PlayFeedbacks();
        }
        else
        {
            popText.transform.localScale = Vector3.one;
        }
    }

    private void SetButtonsActive(bool _active)
    {
        buttonsGroup.interactable = _active;
        buttonsGroup.blocksRaycasts = _active;
    }
}
