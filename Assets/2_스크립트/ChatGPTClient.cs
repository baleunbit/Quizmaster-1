using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ChatGPTRequest
{
    public string model = "gpt-4.1-nano";
    public Message[] messages;
    public float temperature = 1.1f;
    public int max_completion_tokens = 4000;
}

[Serializable]
public class Message
{
    public string role;
    public string content;
}

[Serializable]
public class ChatGPTResponse
{
    public Choice[] choices;
}

[Serializable]
public class Choice
{
    public Message message;
}

[Serializable]
public class QuizData
{
    public QuizQuestion[] questions;
}

[Serializable]
public class QuizQuestion
{
    public string question;
    public string[] answers;
    public int correctAnswerIndex;
    public string hint;
}

public class ChatGPTClient : MonoBehaviour
{
    private const string API_URL = "https://api.openai.com/v1/chat/completions";
    private string apiKey;
    
    [Header("로딩 최적화")]
    [SerializeField] private float requestTimeout = 30f; // 요청 타임아웃 (초)
    [SerializeField] private int maxRetries = 2; // 최대 재시도 횟수
    [SerializeField] private float retryDelay = 2f; // 재시도 지연 시간
    
    // 힌트 이벤트
    public static event Action<string> OnHintReceived;

    private void Awake()
    {
        apiKey = LoadFromResources();
    }

    private string LoadFromResources()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서는 프록시 엔드포인트 사용
            return proxyEndpoint;
        #else
            try
            {
                // 웹에서 안전한 Resources.Load 사용
                TextAsset configFile = null;
                #if UNITY_WEBGL && !UNITY_EDITOR
                    // 웹에서는 WebBuildBugFixer의 안전한 메서드 사용
                    if (WebBuildBugFixer.SafeResourcesLoad<TextAsset>("config") != null)
                    {
                        configFile = WebBuildBugFixer.SafeResourcesLoad<TextAsset>("config");
                    }
                #else
                    configFile = WebBuildBugFixer.SafeResourcesLoad<TextAsset>("config");
                #endif
                
                if (configFile != null)
                {
                    string[] lines = configFile.text.Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("OPENAI_API_KEY="))
                        {
                            return line.Substring("OPENAI_API_KEY=".Length).Trim();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Resources 설정 파일 로드 실패: {e.Message}");
            }

            return "";
        #endif
    }

    private string GetRequestURL()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서는 프록시 엔드포인트 사용
            return string.IsNullOrEmpty(proxyEndpoint) ? API_URL : proxyEndpoint;
        #else
            // 데스크톱에서는 직접 API 호출
            return API_URL;
        #endif
    }

    public delegate void QuizGenerateHandler(List<QuestionSO> questions);
    public event QuizGenerateHandler quizGenerateHandler;

    // ✅ 호출부 호환용 래퍼 (스크린샷의 GenerateQuestions 호출과 동일한 시그니처)
    public void GenerateQuestions(int count = 3, string topic = "일반상식")
    {
        GenerateQuizQuestions(count, topic);
    }

    public void GenerateQuizQuestions(int count = 3, string topic = "일반상식")
    {
        StartCoroutine(RequestQuizQuestionsWithRetry(count, topic, 0));
    }

    private IEnumerator RequestQuizQuestionsWithRetry(int count, string topic, int retryCount)
    {
        yield return StartCoroutine(RequestQuizQuestions(count, topic));
        
        // 실패 시 재시도
        if (retryCount < maxRetries)
        {
            yield return new WaitForSeconds(retryDelay);
            StartCoroutine(RequestQuizQuestionsWithRetry(count, topic, retryCount + 1));
        }
    }

    private IEnumerator RequestQuizQuestions(int count, string topic)
    {
        // 캐시에서 문제 확인
        if (QuestionCache.instance != null && QuestionCache.instance.HasEnoughQuestions(topic, count))
        {
            List<QuestionSO> cachedQuestions = QuestionCache.instance.GetCachedQuestions(topic, count);
            if (cachedQuestions.Count > 0)
            {
                quizGenerateHandler?.Invoke(cachedQuestions);
                yield break;
            }
        }

        // 웹빌드에서 대체 문제 확인
        #if UNITY_WEBGL && !UNITY_EDITOR
            if (WebFallbackQuestions.instance != null && WebFallbackQuestions.instance.HasFallbackQuestions(topic))
            {
                List<QuestionSO> fallbackQuestions = WebFallbackQuestions.instance.GetFallbackQuestions(topic, count);
                if (fallbackQuestions.Count > 0)
                {
                    Debug.Log($"웹빌드 대체 문제 사용: {topic} 주제 {fallbackQuestions.Count}개");
                    quizGenerateHandler?.Invoke(fallbackQuestions);
                    yield break;
                }
            }
        #endif

        string prompt = $"다음 조건에 맞는 간결하고 재미있는 객관식 퀴즈 문제를 {count}개 생성해주세요:\n" +
                       $"주제: {topic}\n" +
                       "조건:\n" +
                       "- 문제는 20자 이내로 간결하게 작성해주세요\n" +
                       "- 각 선택지는 10자 이내로 간단명료하게 작성해주세요\n" +
                       "- 4개의 선택지를 제공해주세요\n" +
                       "- 정답은 0~3 사이의 인덱스로 표시해주세요\n" +
                       "- 문제와 선택지는 핵심만 담아 간결하게 작성해주세요\n" +
                       "- 각 문제마다 힌트를 30자 이내로 제공해주세요 (정답을 직접 말하지 않고 학습에 도움이 되는 정보)\n" +
                       "- 응답은 반드시 다음 JSON 형식으로만 제공해주세요:\n" +
                       "{\n" +
                       "  \"questions\": [\n" +
                       "    {\n" +
                       "      \"question\": \"문제 내용\",\n" +
                       "      \"answers\": [\"선택지1\", \"선택지2\", \"선택지3\", \"선택지4\"],\n" +
                       "      \"correctAnswerIndex\": 0,\n" +
                       "      \"hint\": \"힌트 내용\"\n" +
                       "    }\n" +
                       "  ]\n" +
                       "}";

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Prompt to ChatGPT:\n" + prompt);
        #endif

        ChatGPTRequest request = new ChatGPTRequest
        {
            messages = new Message[]
            {
                new Message { role = "user", content = prompt }
            },
            temperature = 0.7f, // 창의성과 일관성의 균형
            max_completion_tokens = 2000 // 토큰 수 제한으로 응답 속도 향상
        };

        string jsonRequest = JsonUtility.ToJson(request);
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Request JSON:\n" + jsonRequest);
        #endif

        string requestURL = GetRequestURL();
        using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                // 웹에서는 프록시를 통해 요청
                webRequest.SetRequestHeader("X-API-Key", apiKey);
                // CORS 헤더 추가
                webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");
                webRequest.SetRequestHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
                webRequest.SetRequestHeader("Access-Control-Allow-Headers", "Content-Type, X-API-Key");
            #else
                // 데스크톱에서는 직접 API 호출
                webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            #endif

            // 타임아웃 설정
            float startTime = Time.time;
            yield return webRequest.SendWebRequest();

            // 타임아웃 체크
            if (Time.time - startTime > requestTimeout)
            {
                Debug.LogError($"요청 타임아웃: {requestTimeout}초 초과");
                yield break;
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    Debug.Log("Raw response from ChatGPT:\n" + webRequest.downloadHandler.text);
                    ChatGPTResponse response = JsonUtility.FromJson<ChatGPTResponse>(webRequest.downloadHandler.text);

                    if (response == null || response.choices == null || response.choices.Length == 0)
                    {
                        Debug.LogError("Invalid response structure from ChatGPT API");
                        yield break;
                    }

                    if (response.choices[0].message == null)
                    {
                        Debug.LogError("Message content is null in ChatGPT response");
                        yield break;
                    }

                    string jsonContent = response.choices[0].message.content;

                    if (string.IsNullOrEmpty(jsonContent))
                    {
                        Debug.LogError("Content is empty. Finish reason: " + response.choices[0].message);
                        Debug.LogError("Consider increasing max_completion_tokens");
                        yield break;
                    }

                    Debug.Log("Response from ChatGPT:\n" + jsonContent);
                    
                    // JSON 문자열 정리 및 이스케이프 문자 처리
                    jsonContent = CleanJsonString(jsonContent);
                    
                    Debug.Log("Cleaned JSON content:\n" + jsonContent);

                    QuizData quizData = JsonUtility.FromJson<QuizData>(jsonContent);
                    List<QuestionSO> generatedQuestions = CreateQuestionSOs(quizData.questions);

                    // 생성된 문제를 캐시에 저장
                    if (QuestionCache.instance != null)
                    {
                        QuestionCache.instance.CacheQuestions(topic, generatedQuestions);
                    }

                    quizGenerateHandler?.Invoke(generatedQuestions);
                }
                catch (Exception e)
                {
                    Debug.LogError($"응답 파싱 오류: {e.Message}");
                    Debug.LogError($"응답 내용: {webRequest.downloadHandler.text}");
                    
                    // 웹빌드에서 대체 문제 사용
                    #if UNITY_WEBGL && !UNITY_EDITOR
                        UseFallbackQuestions(count, topic);
                    #endif
                }
            }
            else
            {
                Debug.LogError($"ChatGPT API 요청 실패: {webRequest.error}");
                Debug.LogError($"응답 코드: {webRequest.responseCode}");
                Debug.LogError($"응답 내용: {webRequest.downloadHandler.text}");
                
                // 웹빌드에서 대체 문제 사용
                #if UNITY_WEBGL && !UNITY_EDITOR
                    UseFallbackQuestions(count, topic);
                #endif
            }
        }
    }

    /// <summary>
    /// 웹빌드에서 대체 문제를 사용합니다
    /// </summary>
    private void UseFallbackQuestions(int count, string topic)
    {
        if (WebFallbackQuestions.instance != null && WebFallbackQuestions.instance.HasFallbackQuestions(topic))
        {
            List<QuestionSO> fallbackQuestions = WebFallbackQuestions.instance.GetFallbackQuestions(topic, count);
            if (fallbackQuestions.Count > 0)
            {
                Debug.Log($"웹빌드 대체 문제 사용: {topic} 주제 {fallbackQuestions.Count}개");
                quizGenerateHandler?.Invoke(fallbackQuestions);
                return;
            }
        }
        
        // 대체 문제도 없으면 일반상식으로 대체
        if (WebFallbackQuestions.instance != null)
        {
            List<QuestionSO> generalQuestions = WebFallbackQuestions.instance.GetFallbackQuestions("일반상식", count);
            if (generalQuestions.Count > 0)
            {
                Debug.Log($"웹빌드 일반상식 대체 문제 사용: {generalQuestions.Count}개");
                quizGenerateHandler?.Invoke(generalQuestions);
            }
        }
    }

    private List<QuestionSO> CreateQuestionSOs(QuizQuestion[] quizQuestions)
    {
        List<QuestionSO> questionSOs = new List<QuestionSO>();

        foreach (QuizQuestion quizQ in quizQuestions)
        {
            QuestionSO questionSO = ScriptableObject.CreateInstance<QuestionSO>();

            // Reflection을 사용하여 private 필드에 값 설정
            var questionField = typeof(QuestionSO).GetField("question", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var answersField = typeof(QuestionSO).GetField("answers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var correctAnswerIndexField = typeof(QuestionSO).GetField("correctAnswerIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hintField = typeof(QuestionSO).GetField("hint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            questionField?.SetValue(questionSO, quizQ.question);
            answersField?.SetValue(questionSO, quizQ.answers);
            correctAnswerIndexField?.SetValue(questionSO, quizQ.correctAnswerIndex);
            hintField?.SetValue(questionSO, quizQ.hint);

            questionSOs.Add(questionSO);
        }

        return questionSOs;
    }

    public void SetApiKey(string key)
    {
        apiKey = key;
        PlayerPrefs.SetString("OpenAI_API_Key", key);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// JSON 문자열을 정리하고 이스케이프 문자를 처리하는 메서드
    /// </summary>
    /// <param name="jsonString">정리할 JSON 문자열</param>
    /// <returns>정리된 JSON 문자열</returns>
    private string CleanJsonString(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
            return jsonString;

        // 기본 정리
        jsonString = jsonString.Trim();
        
        // 마크다운 코드 블록 제거
        if (jsonString.StartsWith("```json"))
        {
            jsonString = jsonString.Substring(7);
        }
        if (jsonString.EndsWith("```"))
        {
            jsonString = jsonString.Substring(0, jsonString.Length - 3);
        }
        jsonString = jsonString.Trim();
        
        // 잘못된 이스케이프 문자 수정
        jsonString = jsonString.Replace("\\\"", "\""); // \" -> "
        jsonString = jsonString.Replace("\\n", " ");   // \n -> 공백
        jsonString = jsonString.Replace("\\t", " ");   // \t -> 공백
        jsonString = jsonString.Replace("\\r", " ");   // \r -> 공백
        
        // 연속된 공백을 하나로 정리
        while (jsonString.Contains("  "))
        {
            jsonString = jsonString.Replace("  ", " ");
        }
        
        // JSON 구조 확인 및 수정
        if (!jsonString.StartsWith("{"))
        {
            int startIndex = jsonString.IndexOf('{');
            if (startIndex >= 0)
            {
                jsonString = jsonString.Substring(startIndex);
            }
        }
        
        if (!jsonString.EndsWith("}"))
        {
            int endIndex = jsonString.LastIndexOf('}');
            if (endIndex >= 0)
            {
                jsonString = jsonString.Substring(0, endIndex + 1);
            }
        }
        
        return jsonString;
    }

    /// <summary>
    /// 힌트를 요청하는 메서드
    /// </summary>
    /// <param name="question">문제 내용</param>
    /// <param name="answers">선택지들</param>
    /// <param name="correctIndex">정답 인덱스</param>
    public void RequestHint(string question, string[] answers, int correctIndex)
    {
        StartCoroutine(RequestHintCoroutine(question, answers, correctIndex));
    }

    private IEnumerator RequestHintCoroutine(string question, string[] answers, int correctIndex)
    {
        string prompt = $"다음 퀴즈 문제에 대한 힌트를 제공해주세요:\n\n" +
                       $"문제: {question}\n" +
                       $"선택지:\n";
        
        for (int i = 0; i < answers.Length; i++)
        {
            prompt += $"{i + 1}. {answers[i]}\n";
        }
        
        prompt += $"\n정답은 {correctIndex + 1}번입니다.\n\n" +
                 "조건:\n" +
                 "- 힌트는 정답을 직접적으로 말하지 않아야 합니다\n" +
                 "- 힌트는 50자 이내로 간결하게 작성해주세요\n" +
                 "- 학습에 도움이 되는 유용한 정보를 제공해주세요\n" +
                 "- 응답은 JSON 형식으로만 제공해주세요:\n" +
                 "{\n" +
                 "  \"hint\": \"힌트 내용\"\n" +
                 "}";

        Debug.Log("힌트 요청 프롬프트:\n" + prompt);

        ChatGPTRequest request = new ChatGPTRequest
        {
            messages = new Message[]
            {
                new Message { role = "user", content = prompt }
            }
        };

        string jsonRequest = JsonUtility.ToJson(request);
        Debug.Log("힌트 요청 JSON:\n" + jsonRequest);

        string requestURL = GetRequestURL();
        using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                // 웹에서는 프록시를 통해 요청
                webRequest.SetRequestHeader("X-API-Key", apiKey);
            #else
                // 데스크톱에서는 직접 API 호출
                webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);
            #endif

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string responseText = webRequest.downloadHandler.text;
                Debug.Log("힌트 응답:\n" + responseText);
                
                try
                {
                    ChatGPTResponse response = JsonUtility.FromJson<ChatGPTResponse>(responseText);
                    if (response.choices != null && response.choices.Length > 0)
                    {
                        string hintContent = response.choices[0].message.content;
                        
                        // JSON 문자열 정리
                        hintContent = CleanJsonString(hintContent);
                        
                        // JSON에서 힌트 추출
                        HintData hintData = JsonUtility.FromJson<HintData>(hintContent);
                        if (hintData != null && !string.IsNullOrEmpty(hintData.hint))
                        {
                            OnHintReceived?.Invoke(hintData.hint);
                        }
                        else
                        {
                            Debug.LogError("힌트 데이터 파싱 실패");
                            OnHintReceived?.Invoke("힌트를 받아올 수 없습니다.");
                        }
                    }
                    else
                    {
                        Debug.LogError("힌트 응답이 비어있습니다.");
                        OnHintReceived?.Invoke("힌트를 받아올 수 없습니다.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"힌트 응답 파싱 오류: {e.Message}");
                    OnHintReceived?.Invoke("힌트를 받아올 수 없습니다.");
                }
            }
            else
            {
                Debug.LogError($"힌트 요청 실패: {webRequest.error}");
                OnHintReceived?.Invoke("힌트를 받아올 수 없습니다.");
            }
        }
    }
}

[Serializable]
public class HintData
{
    public string hint;
}

