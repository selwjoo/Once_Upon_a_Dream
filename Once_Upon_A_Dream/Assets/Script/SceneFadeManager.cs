using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFadeManager : MonoBehaviour
{
    public static SceneFadeManager Instance;

    [Header("페이드 설정")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 시작 시 페이드 이미지 숨김
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }
    }

    void Start()
    {
        // 게임 시작 시 페이드 인
        StartCoroutine(FadeIn());
    }

    // 페이드 아웃 후 씬 전환
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    // 페이드 아웃
    IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
        yield return StartCoroutine(FadeIn());
    }

    // 페이드 아웃 (화면 어두워짐)
    IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    // 페이드 인 (화면 밝아짐)
    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
    }
}