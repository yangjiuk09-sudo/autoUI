using UnityEngine;
using UnityEngine.EventSystems;

public class QuitAfterHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float holdDuration = 4f;
    bool holding = false; float t = 0f;
    void Update()
    {
        if (!holding) return;
        t += Time.unscaledDeltaTime;
        if (t >= holdDuration)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
    public void OnPointerDown(PointerEventData eventData) { holding = true; t = 0f; }
    public void OnPointerUp(PointerEventData eventData) { holding = false; }
}
