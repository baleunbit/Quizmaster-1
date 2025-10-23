using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// 웹빌드에서 문제 생성을 관리하는 클래스
/// </summary>
public class WebBuildQuestionManager : MonoBehaviour
{
    [Header("웹빌드 문제 관리")]
    [SerializeField] private bool useFallbackQuestions = true;
    [SerializeField] private TextMeshProUGUI statusText;

    private ChatGPTClient chatGPTClient;
    private QuestionCache questionCache;
    private WebFallbackQuestions fallbackQuestions;

    public static WebBuildQuestionManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 웹에서 안전한 컴포넌트 검색
        chatGPTClient = WebBuildBugFixer.SafeFindObjectsOfType<ChatGPTClient>().FirstOrDefault();
        questionCache = QuestionCache.instance;
        fallbackQuestions = WebFallbackQuestions.instance;

        #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(InitializeWebBuildQuestions());
        #endif
    }

    private IEnumerator InitializeWebBuildQuestions()
    {
        UpdateStatus("웹빌드 문제 시스템 초기화 중...");

        // 1. 대체 문제 시스템 확인
        yield return new WaitForSeconds(0.5f);
        if (fallbackQuestions != null)
        {
            UpdateStatus("대체 문제 시스템 활성화됨");
        }
        else
        {
            UpdateStatus("❌ 대체 문제 시스템 비활성화");
        }

        // 2. 캐시 시스템 확인
        yield return new WaitForSeconds(0.5f);
        if (questionCache != null)
        {
            UpdateStatus("캐시 시스템 활성화됨");
        }
        else
        {
            UpdateStatus("❌ 캐시 시스템 비활성화");
        }

        // 3. API 클라이언트 확인
        yield return new WaitForSeconds(0.5f);
        if (chatGPTClient != null)
        {
            UpdateStatus("API 클라이언트 활성화됨");
        }
        else
        {
            UpdateStatus("❌ API 클라이언트 비활성화");
        }

        UpdateStatus("웹빌드 문제 시스템 초기화 완료!");
    }

    /// <summary>
    /// 웹빌드에서 문제를 안전하게 생성합니다
    /// </summary>
    public void GenerateQuestionsSafely(string topic, int count)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(GenerateQuestionsWithFallback(topic, count));
        #else
            // 데스크톱에서는 일반 생성
            if (chatGPTClient != null)
            {
                chatGPTClient.GenerateQuestions(count, topic);
            }
        #endif
    }

    private IEnumerator GenerateQuestionsWithFallback(string topic, int count)
    {
        UpdateStatus($"문제 생성 시작: {topic} 주제 {count}개");

        // 1. 캐시에서 문제 확인
        if (questionCache != null && questionCache.HasEnoughQuestions(topic, count))
        {
            UpdateStatus("캐시에서 문제 로드 중...");
            yield return new WaitForSeconds(0.5f);
            UpdateStatus("캐시에서 문제 로드 완료!");
            yield break;
        }

        // 2. 대체 문제 사용
        if (useFallbackQuestions && fallbackQuestions != null && fallbackQuestions.HasFallbackQuestions(topic))
        {
            UpdateStatus("대체 문제 사용 중...");
            yield return new WaitForSeconds(0.5f);
            UpdateStatus("대체 문제 사용 완료!");
            yield break;
        }

        // 3. API 호출 시도
        UpdateStatus("API 호출 시도 중...");
        if (chatGPTClient != null)
        {
            chatGPTClient.GenerateQuestions(count, topic);
        }

        // 4. API 호출 실패 시 대체 문제 사용
        yield return new WaitForSeconds(5f); // API 응답 대기
        if (useFallbackQuestions && fallbackQuestions != null)
        {
            UpdateStatus("API 호출 실패 - 대체 문제 사용");
            List<QuestionSO> fallbackQuestionsList = fallbackQuestions.GetFallbackQuestions(topic, count);
            if (fallbackQuestionsList != null && fallbackQuestionsList.Count > 0)
            {
                // 문제 생성 이벤트 발생
                var quizGenerateHandler = chatGPTClient.GetType().GetEvent("quizGenerateHandler");
                if (quizGenerateHandler != null)
                {
                    // 이벤트 호출 (리플렉션 사용)
                    var method = chatGPTClient.GetType().GetMethod("OnQuizGenerated");
                    if (method != null)
                    {
                        method.Invoke(chatGPTClient, new object[] { fallbackQuestionsList });
                    }
                }
            }
        }
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            WebBuildSettings.SetWebText(statusText, message);
        }
        Debug.Log($"WebBuildQuestionManager: {message}");
    }

    /// <summary>
    /// 웹빌드 문제 생성 상태를 확인합니다
    /// </summary>
    public bool IsQuestionGenerationWorking()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            return fallbackQuestions != null && fallbackQuestions.HasFallbackQuestions("일반상식");
        #else
            return chatGPTClient != null;
        #endif
    }

    /// <summary>
    /// 웹빌드 진단 정보를 반환합니다
    /// </summary>
    public string GetWebBuildDiagnostics()
    {
        string diagnostics = "웹빌드 진단 정보:\n";
        diagnostics += $"플랫폼: {Application.platform}\n";
        #if UNITY_WEBGL && !UNITY_EDITOR
            diagnostics += $"웹빌드: true\n";
        #else
            diagnostics += $"웹빌드: false\n";
        #endif
        diagnostics += $"API 클라이언트: {(chatGPTClient != null ? "활성화" : "비활성화")}\n";
        diagnostics += $"캐시 시스템: {(questionCache != null ? "활성화" : "비활성화")}\n";
        diagnostics += $"대체 문제: {(fallbackQuestions != null ? "활성화" : "비활성화")}\n";
        
        if (fallbackQuestions != null)
        {
            string[] topics = fallbackQuestions.GetAvailableTopics();
            diagnostics += $"사용 가능한 주제: {string.Join(", ", topics)}\n";
        }
        
        return diagnostics;
    }
}
