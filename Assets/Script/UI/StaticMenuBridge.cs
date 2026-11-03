using UnityEngine;

public class StaticMenuBridge : MonoBehaviour
{
 public void LoadScene(string sceneName)
    {
        StaticMenu.LoadSceneSafe(sceneName);
    }
    public void FadeOut()
    {
        if (StaticMenu.Instance != null) StaticMenu.Instance.FadeOut();
    }
    public void FadeIn()
    {
        if (StaticMenu.Instance != null) StaticMenu.Instance.FadeIn();
    }
}
