using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;

/// <summary>
/// 웹빌드에서 문제 생성 디버깅을 위한 클래스
/// </summary>
public class WebBuildDebugger : MonoBehaviour
{
    [Header("웹빌드 디버깅")]
    [SerializeField] private TextMeshProUGUI debugText;

    private ChatGPTClient chatGPTClient;
    private QuestionCache questionCache;

    private void Start()
    {
        // 웹에서 안전한 컴포넌트 검색
        chatGPTClient = WebBuildBugFixer.SafeFindObjectsOfType<ChatGPTClient>().FirstOrDefault();
        questionCache = QuestionCache.instance;

        #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(WebBuildDiagnostics());
        #endif
    }

    private IEnumerator WebBuildDiagnostics()
    {
        yield return new WaitForSeconds(1f);

        string debugInfo = "웹빌드 진단 시작...\n";
        UpdateDebugText(debugInfo);

        // 1. API 키 확인
        yield return new WaitForSeconds(0.5f);
        debugInfo += CheckAPIKey();
        UpdateDebugText(debugInfo);

        // 2. 네트워크 연결 확인
        yield return new WaitForSeconds(0.5f);
        debugInfo += CheckNetworkConnection();
        UpdateDebugText(debugInfo);

        // 3. 캐시 상태 확인
        yield return new WaitForSeconds(0.5f);
        debugInfo += CheckCacheStatus();
        UpdateDebugText(debugInfo);

        // 4. 문제 생성 테스트
        yield return new WaitForSeconds(0.5f);
        debugInfo += "문제 생성 테스트 시작...\n";
        UpdateDebugText(debugInfo);

        // 간단한 문제 생성 테스트
        if (chatGPTClient != null)
        {
            chatGPTClient.GenerateQuestions(1, "테스트");
        }
    }

    private string CheckAPIKey()
    {
        string result = "API 키 확인: ";
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서는 프록시 엔드포인트 확인
            if (chatGPTClient != null)
            {
                // ChatGPTClient의 proxyEndpoint 확인
                var proxyField = chatGPTClient.GetType().GetField("proxyEndpoint", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (proxyField != null)
                {
                    string proxyEndpoint = (string)proxyField.GetValue(chatGPTClient);
                    if (string.IsNullOrEmpty(proxyEndpoint))
                    {
                        result += "❌ 프록시 엔드포인트가 설정되지 않음\n";
                    }
                    else
                    {
                        result += $"✅ 프록시 엔드포인트: {proxyEndpoint}\n";
                    }
                }
            }
        #else
            result += "✅ 데스크톱 모드\n";
        #endif

        return result;
    }

    private string CheckNetworkConnection()
    {
        string result = "네트워크 연결: ";
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서는 CORS 문제 가능성 확인
            result += "웹 환경 - CORS 설정 필요\n";
            result += "서버에서 Access-Control-Allow-Origin 헤더 설정 필요\n";
        #else
            result += "✅ 데스크톱 모드\n";
        #endif

        return result;
    }

    private string CheckCacheStatus()
    {
        string result = "캐시 상태: ";
        
        if (questionCache != null)
        {
            result += "✅ 캐시 시스템 활성화\n";
            result += questionCache.GetCacheStatus();
        }
        else
        {
            result += "❌ 캐시 시스템 비활성화\n";
        }

        return result;
    }

    private void UpdateDebugText(string text)
    {
        if (debugText != null)
        {
            WebBuildSettings.SetWebText(debugText, text);
        }
    }

    /// <summary>
    /// 웹빌드에서 문제 생성 테스트
    /// </summary>
    [ContextMenu("Test Question Generation")]
    public void TestQuestionGeneration()
    {
        if (chatGPTClient != null)
        {
            Debug.Log("웹빌드 문제 생성 테스트 시작...");
            chatGPTClient.GenerateQuestions(1, "테스트");
        }
    }

    /// <summary>
    /// 웹빌드 진단 정보 출력
    /// </summary>
    [ContextMenu("Show Web Build Info")]
    public void ShowWebBuildInfo()
    {
        string info = $"플랫폼: {Application.platform}\n";
        #if UNITY_WEBGL && !UNITY_EDITOR
            info += $"웹빌드: true\n";
        #else
            info += $"웹빌드: false\n";
        #endif
        info += $"에디터: {Application.isEditor}\n";
        
        if (debugText != null)
        {
            WebBuildSettings.SetWebText(debugText, info);
        }
        
        Debug.Log(info);
    }
}
