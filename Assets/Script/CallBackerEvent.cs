using UnityEngine;
using UnityEngine.Events;

public class CallBackerEvent : MonoBehaviour
{
    public UnityEvent onStart;
    public UnityEvent onEnable;
    public UnityEvent onDisable;
    public UnityEvent onDestroy;
    public UnityEvent onApplicationQuit;

    private void Start()
    {
        onStart?.Invoke();
    }

    private void OnEnable()
    {
        onEnable?.Invoke();
    }

    private void OnDisable()
    {
        onDisable?.Invoke();
    }

    private void OnDestroy()
    {
        onDestroy?.Invoke();
    }

    private void OnApplicationQuit()
    {
        onApplicationQuit?.Invoke();
    }
}
