using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StaticMenu : MonoBehaviour
{
    public static StaticMenu Instance { get; private set; }

    public Image loadingPanel;
    public float fadeDuration = 0.5f;

    private bool isLoading;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;

        yield return FadeRoutine(0f, 1f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        isLoading = false;
    }

    public void FadeOut()
    {
        StartFade(0f, 1f);
    }

    public void FadeIn()
    {
        StartFade(1f, 0f);
    }

    private void StartFade(float from, float to)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(from, to));
    }

    private IEnumerator FadeRoutine(float from, float to)
    {
        loadingPanel.gameObject.SetActive(true);
        loadingPanel.raycastTarget = true;

        Color c = loadingPanel.color;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(from, to, t / fadeDuration);
            loadingPanel.color = c;
            yield return null;
        }

        c.a = to;
        loadingPanel.color = c;

        if (to <= 0f)
        {
            loadingPanel.raycastTarget = false;
            loadingPanel.gameObject.SetActive(false);
        }
    }
}