using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using TMPro;

/// <summary>
/// 코드 호환성 및 작동 가능성을 검토하는 클래스
/// </summary>
public class CodeCompatibilityChecker : MonoBehaviour
{
    [Header("호환성 검사")]
    [SerializeField] private bool enableCompatibilityCheck = true;

    private void Start()
    {
        if (enableCompatibilityCheck)
        {
            StartCoroutine(CheckCodeCompatibility());
        }
    }

    private System.Collections.IEnumerator CheckCodeCompatibility()
    {
        yield return new WaitForSeconds(1f);

        Debug.Log("=== 코드 호환성 검사 시작 ===");

        // 1. 싱글톤 패턴 검사
        yield return new WaitForSeconds(0.5f);
        CheckSingletonPatterns();

        // 2. 의존성 검사
        yield return new WaitForSeconds(0.5f);
        CheckDependencies();

        // 3. 웹빌드 호환성 검사
        yield return new WaitForSeconds(0.5f);
        CheckWebBuildCompatibility();

        // 4. 메모리 사용량 검사
        yield return new WaitForSeconds(0.5f);
        CheckMemoryUsage();

        Debug.Log("=== 코드 호환성 검사 완료 ===");
    }

    private void CheckSingletonPatterns()
    {
        Debug.Log("--- 싱글톤 패턴 검사 ---");
        
        // ScoreKeeper 싱글톤 검사
        if (ScoreKeeper.instance != null)
        {
            Debug.Log("✅ ScoreKeeper 싱글톤: 정상");
        }
        else
        {
            Debug.LogWarning("❌ ScoreKeeper 싱글톤: 누락");
        }

        // AudioManager 싱글톤 검사
        if (AudioManager.instance != null)
        {
            Debug.Log("✅ AudioManager 싱글톤: 정상");
        }
        else
        {
            Debug.LogWarning("❌ AudioManager 싱글톤: 누락");
        }

        // QuestionCache 싱글톤 검사
        if (QuestionCache.instance != null)
        {
            Debug.Log("✅ QuestionCache 싱글톤: 정상");
        }
        else
        {
            Debug.LogWarning("❌ QuestionCache 싱글톤: 누락");
        }

        // WebFallbackQuestions 싱글톤 검사
        if (WebFallbackQuestions.instance != null)
        {
            Debug.Log("✅ WebFallbackQuestions 싱글톤: 정상");
        }
        else
        {
            Debug.LogWarning("❌ WebFallbackQuestions 싱글톤: 누락");
        }
    }

    private void CheckDependencies()
    {
        Debug.Log("--- 의존성 검사 ---");

        // ChatGPTClient 검사 (웹에서 안전하게)
        ChatGPTClient chatGPTClient = WebBuildBugFixer.SafeFindObjectsOfType<ChatGPTClient>().FirstOrDefault();
        if (chatGPTClient != null)
        {
            Debug.Log("✅ ChatGPTClient: 존재");
        }
        else
        {
            Debug.LogWarning("❌ ChatGPTClient: 누락");
        }

        // Timer 검사 (웹에서 안전하게)
        Timer timer = WebBuildBugFixer.SafeFindObjectsOfType<Timer>().FirstOrDefault();
        if (timer != null)
        {
            Debug.Log("✅ Timer: 존재");
        }
        else
        {
            Debug.LogWarning("❌ Timer: 누락");
        }

        // GameManger 검사 (웹에서 안전하게)
        GameManger gameManager = WebBuildBugFixer.SafeFindObjectsOfType<GameManger>().FirstOrDefault();
        if (gameManager != null)
        {
            Debug.Log("✅ GameManger: 존재");
        }
        else
        {
            Debug.LogWarning("❌ GameManger: 누락");
        }
    }

    private void CheckWebBuildCompatibility()
    {
        Debug.Log("--- 웹빌드 호환성 검사 ---");

        #if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("🌐 웹빌드 환경 감지");
            
            // 웹 전용 컴포넌트 검사 (웹에서 안전하게)
            WebBuildBugFixer bugFixer = WebBuildBugFixer.SafeFindObjectsOfType<WebBuildBugFixer>().FirstOrDefault();
            if (bugFixer != null)
            {
                Debug.Log("✅ WebBuildBugFixer: 활성화");
            }
            else
            {
                Debug.LogWarning("❌ WebBuildBugFixer: 누락 (웹에서 권장)");
            }

            // 웹 텍스트 렌더링 검사 (웹에서 안전하게)
            WebTextRenderer textRenderer = WebBuildBugFixer.SafeFindObjectsOfType<WebTextRenderer>().FirstOrDefault();
            if (textRenderer != null)
            {
                Debug.Log("✅ WebTextRenderer: 활성화");
            }
            else
            {
                Debug.LogWarning("❌ WebTextRenderer: 누락 (웹에서 권장)");
            }
        #else
            Debug.Log("🖥️ 데스크톱 환경 감지");
        #endif
    }

    private void CheckMemoryUsage()
    {
        Debug.Log("--- 메모리 사용량 검사 ---");
        
        long memoryUsage = System.GC.GetTotalMemory(false);
        Debug.Log($"현재 메모리 사용량: {memoryUsage / 1024 / 1024}MB");

        if (memoryUsage > 100 * 1024 * 1024) // 100MB 이상
        {
            Debug.LogWarning("⚠️ 메모리 사용량이 높습니다. 최적화가 필요할 수 있습니다.");
        }
        else
        {
            Debug.Log("✅ 메모리 사용량: 정상");
        }
    }

    /// <summary>
    /// 수동으로 호환성 검사 실행
    /// </summary>
    [ContextMenu("Check Compatibility")]
    public void ManualCompatibilityCheck()
    {
        StartCoroutine(CheckCodeCompatibility());
    }

    /// <summary>
    /// 웹빌드 전용 검사
    /// </summary>
    public void CheckWebBuildSpecific()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("=== 웹빌드 전용 검사 ===");
            
            // 웹에서 문제가 될 수 있는 컴포넌트들 검사
            CheckWebSpecificComponents();
        #endif
    }

    private void CheckWebSpecificComponents()
    {
        // FindObjectsOfType 사용 검사 (웹에서 안전하게)
        var allTexts = WebBuildBugFixer.SafeFindObjectsOfType<TextMeshProUGUI>();
        Debug.Log($"TextMeshPro 컴포넌트 수: {allTexts.Length}");

        // Resources.Load 사용 검사 (웹에서 안전하게)
        var configFile = WebBuildBugFixer.SafeResourcesLoad<TextAsset>("config");
        if (configFile != null)
        {
            Debug.Log("✅ config 파일 로드 성공");
        }
        else
        {
            Debug.LogWarning("❌ config 파일 로드 실패");
        }
    }
}
