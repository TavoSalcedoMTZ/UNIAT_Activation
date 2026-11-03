using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Genera estrellas que parpadean dentro de su RectTransform para el fondo espacial
[RequireComponent(typeof(RectTransform))]
public class StarField : MonoBehaviour
{
    public Sprite starSprite;
    public int starCount = 140;
    public Vector2 sizeRange = new(2f, 7f);
    public Vector2 alphaRange = new(0.25f, 1f);
    public Vector2 twinkleSpeedRange = new(0.6f, 2.5f);

    private class Star
    {
        public Image image;
        public float baseAlpha;
        public float speed;
        public float offset;
    }

    private readonly List<Star> stars = new();

    private void Start()
    {
        Rect area = ((RectTransform)transform).rect;

        for (int i = 0; i < starCount; i++)
        {
            GameObject go = new GameObject("Star", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            RectTransform rt = (RectTransform)go.transform;
            float size = Random.Range(sizeRange.x, sizeRange.y);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));

            Image image = go.GetComponent<Image>();
            image.sprite = starSprite;
            image.raycastTarget = false;

            stars.Add(new Star
            {
                image = image,
                baseAlpha = Random.Range(alphaRange.x, alphaRange.y),
                speed = Random.Range(twinkleSpeedRange.x, twinkleSpeedRange.y),
                offset = Random.Range(0f, Mathf.PI * 2f)
            });
        }
    }

    private void Update()
    {
        foreach (Star star in stars)
        {
            Color c = star.image.color;
            c.a = star.baseAlpha * (0.6f + 0.4f * Mathf.Sin(Time.time * star.speed + star.offset));
            star.image.color = c;
        }
    }
}
