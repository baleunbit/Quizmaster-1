using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 로딩 최적화 시스템 - 사용자 경험 향상
/// </summary>
public class LoadingOptimizer : MonoBehaviour
{
    [Header("로딩 UI 설정")]
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private string[] loadingMessages = {
        "문제를 생성하고 있습니다...",
        "AI가 퀴즈를 만들고 있어요...",
        "잠시만 기다려주세요...",
        "재미있는 문제를 준비 중입니다...",
        "곧 시작됩니다!"
    };

    [Header("로딩 최적화")]
    [SerializeField] private float messageChangeInterval = 1.5f;
    [SerializeField] private bool enableLoadingAnimation = true;

    private int currentMessageIndex = 0;
    private Coroutine loadingCoroutine;

    private void Start()
    {
        if (loadingText != null)
        {
            StartLoadingAnimation();
        }
    }

    private void StartLoadingAnimation()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
        }
        loadingCoroutine = StartCoroutine(LoadingAnimation());
    }

    private IEnumerator LoadingAnimation()
    {
        while (true)
        {
            if (loadingText != null)
            {
                string message = loadingMessages[currentMessageIndex];
                if (enableLoadingAnimation)
                {
                    message += " " + GetLoadingDots();
                }
                
                WebBuildSettings.SetWebText(loadingText, message);
            }

            yield return new WaitForSeconds(messageChangeInterval);
            currentMessageIndex = (currentMessageIndex + 1) % loadingMessages.Length;
        }
    }

    private string GetLoadingDots()
    {
        int dotCount = (int)(Time.time * 2) % 4;
        return new string('.', dotCount);
    }

    /// <summary>
    /// 로딩 완료 시 호출
    /// </summary>
    public void OnLoadingComplete()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        if (loadingText != null)
        {
            WebBuildSettings.SetWebText(loadingText, "로딩 완료!");
        }
    }

    /// <summary>
    /// 로딩 실패 시 호출
    /// </summary>
    public void OnLoadingFailed()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        if (loadingText != null)
        {
            WebBuildSettings.SetWebText(loadingText, "로딩 실패. 다시 시도해주세요.");
        }
    }

    /// <summary>
    /// 커스텀 로딩 메시지 설정
    /// </summary>
    public void SetLoadingMessage(string message)
    {
        if (loadingText != null)
        {
            WebBuildSettings.SetWebText(loadingText, message);
        }
    }
}
