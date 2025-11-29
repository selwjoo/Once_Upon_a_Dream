using System.Collections;
using UnityEngine;

public class BlackOut : MonoBehaviour
{
    public CanvasGroup blackoutPanel; // fullscreen UI panel
    public float blackoutInterval = 10f;
    public float blackoutDuration = 1.5f;


    public void StartBlackouts()
    {
        StartCoroutine(BlackoutLoop());
    }


    public void StopBlackouts()
    {
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
