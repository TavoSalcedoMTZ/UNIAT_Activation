using UnityEngine;

public class StaticMenuBridge : MonoBehaviour
{
 public void LoadScene(string sceneName)
    {
        StaticMenu.Instance.LoadScene(sceneName);
    }
    public void FadeOut()
    {
        StaticMenu.Instance.FadeOut();
    }
    public void FadeIn()
    {
        StaticMenu.Instance.FadeIn();
    }
}
