using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetCard : MonoBehaviour
{
    public worldInfo world;

    [Header("UI")]
    public Image planetImage;
    public Image planetGlow;
    public TMP_Text planetNameText;
    public TMP_Text careerText;
    public GameObject recommendedBadge;
    public Button button;

    [Header("Feel")]
    public MMF_Player appearFeedback;

    [Header("Pulso del planeta recomendado")]
    public float pulseAmount = 0.06f;
    public float pulseSpeed = 3f;

    private bool isRecommended;

    public void Setup(bool _recommended)
    {
        isRecommended = _recommended;
        recommendedBadge.SetActive(_recommended);

        if (world == null)
        {
            Debug.LogError($"{name} no tiene worldInfo asignado.");
            return;
        }

        planetNameText.text = world.worldName.ToUpper();
        careerText.text = world.carrera.GetDisplayName();

        if (world.worldImage != null)
        {
            planetImage.sprite = world.worldImage;
            planetImage.color = Color.white;
        }
        else
        {
            planetImage.color = world.planetColor;
        }
        if (planetGlow != null)
        {
            Color glow = world.planetColor;
            glow.a = planetGlow.color.a;
            planetGlow.color = glow;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(GoToWorld);
    }

    public void Appear()
    {
        if (appearFeedback != null)
        {
            appearFeedback.PlayFeedbacks();
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    private void GoToWorld()
    {
        if (string.IsNullOrEmpty(world.sceneName))
        {
            Debug.LogWarning($"{world.name} no tiene sceneName asignado.");
            return;
        }
        StaticMenu.LoadSceneSafe(world.sceneName);
    }

    private void Update()
    {
        if (!isRecommended) return;
        float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        planetImage.rectTransform.localScale = Vector3.one * scale;
    }
}
