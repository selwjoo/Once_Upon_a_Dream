using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroStory : MonoBehaviour
{
    [Header("스토리 이미지 리스트")]
    public List<Sprite> storyImages = new List<Sprite>();

    [Header("스토리 텍스트 리스트")]
    [TextArea(3, 10)]
    public List<string> storyTexts = new List<string>();

    [Header("UI")]
    public Image storyImageUI;
    public Text storyText; // 또는 public Text storyText;
    public GameObject skipText; // "Press Space to Continue" (선택사항)

    [Header("설정")]
    public float fadeSpeed = 1f;
    public float typingSpeed = 0.05f; // 타이핑 속도 (초당 글자)

    private int currentIndex = 0;
    private bool isTransitioning = false;
    private bool isTyping = false;
    private CanvasGroup canvasGroup;
    private string currentFullText = "";

    void Start()
    {
        canvasGroup = storyImageUI.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = storyImageUI.gameObject.AddComponent<CanvasGroup>();
        }

        // 리스트 개수 확인
        if (storyImages.Count != storyTexts.Count)
        {
            Debug.LogWarning($"이미지 개수({storyImages.Count})와 텍스트 개수({storyTexts.Count})가 다릅니다!");
        }

        if (storyImages.Count > 0)
        {
            ShowPage(0);
        }
        else
        {
            Debug.LogError("스토리 이미지가 없습니다!");
        }
    }

    void Update()
    {
        // 스페이스바, 엔터, 마우스 좌클릭 감지
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetMouseButtonDown(0))
        {
            // 타이핑 중이면 즉시 완성
            if (isTyping)
            {
                StopAllCoroutines();
                storyText.text = currentFullText;
                isTyping = false;
            }
            // 전환 중이 아니면 다음 페이지
            else if (!isTransitioning)
            {
                NextPage();
            }
        }
    }

    void NextPage()
    {
        currentIndex++;

        if (currentIndex >= storyImages.Count)
        {
            LoadNextScene();
        }
        else
        {
            ShowPage(currentIndex);
        }
    }

    void ShowPage(int index)
    {
        if (index < 0 || index >= storyImages.Count) return;

        Sprite image = storyImages[index];
        string text = (index < storyTexts.Count) ? storyTexts[index] : "";

        // 같은 이미지면 페이드 없이 텍스트만 변경
        if (storyImageUI.sprite == image)
        {
            storyText.text = "";
            StartCoroutine(TypeText(text));
        }
        else
        {
            StartCoroutine(FadeToPage(image, text));
        }
    }

    IEnumerator FadeToPage(Sprite image, string text)
    {
        isTransitioning = true;

        // 페이드 아웃
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        // 이미지와 텍스트 변경
        storyImageUI.sprite = image;
        storyText.text = "";

        // 페이드 인
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        isTransitioning = false;

        // 타이핑 효과 시작
        if (!string.IsNullOrEmpty(text))
        {
            StartCoroutine(TypeText(text));
        }
    }

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        currentFullText = fullText;
        storyText.text = "";

        foreach (char c in fullText)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("Login");
    }
}
