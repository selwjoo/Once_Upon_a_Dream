using System.Collections;
using UnityEngine;

public class BlackOut : MonoBehaviour
{
    public CanvasGroup blackoutPanel; // fullscreen UI panel
    public float blackoutInterval = 10f;
    public float blackoutDuration = 1.5f;


    private Coroutine blackCoroutine;


    public void StartBlackouts()
    {
        if (blackCoroutine != null) StopCoroutine(blackCoroutine);
        blackCoroutine = StartCoroutine(BlackoutLoop());
    }


    public void StopBlackouts()
    {
        if (blackCoroutine != null) StopCoroutine(blackCoroutine);
        SetBlackout(false);
    }


    IEnumerator BlackoutLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(blackoutInterval);
            yield return StartCoroutine(DoBlackout());
        }
    }


    IEnumerator DoBlackout()
    {
        SetBlackout(true);
        yield return new WaitForSeconds(blackoutDuration);
        SetBlackout(false);
    }


    void SetBlackout(bool on)
    {
        if (blackoutPanel == null) return;
        blackoutPanel.alpha = on ? 1f : 0f;
        blackoutPanel.blocksRaycasts = on;
        blackoutPanel.interactable = !on;
    }
}
