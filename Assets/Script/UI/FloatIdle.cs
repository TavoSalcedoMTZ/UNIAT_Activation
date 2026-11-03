using UnityEngine;

//Flotacion suave en el espacio para planetas y titulos
[RequireComponent(typeof(RectTransform))]
public class FloatIdle : MonoBehaviour
{
    public float amplitude = 12f;
    public float speed = 1.2f;
    [Tooltip("Grados de balanceo, 0 para no rotar")]
    public float rotationAmplitude = 0f;

    private RectTransform rectTransform;
    private Vector2 basePosition;
    private float offset;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
        basePosition = rectTransform.anchoredPosition;
        offset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        float wave = Mathf.Sin(Time.time * speed + offset);
        rectTransform.anchoredPosition = basePosition + Vector2.up * (wave * amplitude);
        if (rotationAmplitude != 0f)
        {
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, wave * rotationAmplitude);
        }
    }
}
