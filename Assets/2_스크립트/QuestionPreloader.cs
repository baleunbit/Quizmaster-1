using System.Collections;
using UnityEngine;
using System.Linq;

/// <summary>
/// 문제 사전 로딩 시스템 - 로딩 시간 단축
/// </summary>
public class QuestionPreloader : MonoBehaviour
{
    [Header("사전 로딩 설정")]
    [SerializeField] private bool enablePreloading = true;
    [SerializeField] private int preloadCount = 20; // 사전 로딩 문제 수
    [SerializeField] private float preloadDelay = 2f; // 사전 로딩 지연 시간
    [SerializeField] private string[] preloadTopics = { "일반상식", "과학", "수학", "역사" };

    private ChatGPTClient chatGPTClient;
    private bool isPreloading = false;

    private void Start()
    {
        chatGPTClient = WebBuildBugFixer.SafeFindObjectsOfType<ChatGPTClient>().FirstOrDefault();
        
        if (enablePreloading)
        {
            StartCoroutine(PreloadQuestions());
        }
    }

    private IEnumerator PreloadQuestions()
    {
        yield return new WaitForSeconds(preloadDelay);

        if (isPreloading) yield break;
        isPreloading = true;

        Debug.Log("문제 사전 로딩 시작...");

        foreach (string topic in preloadTopics)
        {
            if (QuestionCache.instance != null && 
                !QuestionCache.instance.HasEnoughQuestions(topic, preloadCount))
            {
                Debug.Log($"사전 로딩: {topic} 주제 문제 생성 중...");
                chatGPTClient.GenerateQuestions(preloadCount, topic);
                
                // 각 주제별로 로딩 완료까지 대기
                yield return new WaitUntil(() => 
                    QuestionCache.instance.HasEnoughQuestions(topic, preloadCount) || 
                    !isPreloading);
                
                yield return new WaitForSeconds(1f); // 다음 주제 로딩 전 대기
            }
        }

        isPreloading = false;
        Debug.Log("문제 사전 로딩 완료!");
    }

    /// <summary>
    /// 특정 주제의 문제를 사전 로딩합니다
    /// </summary>
    public void PreloadTopic(string topic, int count = 10)
    {
        if (QuestionCache.instance != null && 
            !QuestionCache.instance.HasEnoughQuestions(topic, count))
        {
            StartCoroutine(PreloadSingleTopic(topic, count));
        }
    }

    private IEnumerator PreloadSingleTopic(string topic, int count)
    {
        Debug.Log($"사전 로딩: {topic} 주제 {count}개 문제 생성 중...");
        chatGPTClient.GenerateQuestions(count, topic);
        
        yield return new WaitUntil(() => 
            QuestionCache.instance.HasEnoughQuestions(topic, count));
        
        Debug.Log($"사전 로딩 완료: {topic} 주제");
    }

    /// <summary>
    /// 사전 로딩 상태를 반환합니다
    /// </summary>
    public bool IsPreloading()
    {
        return isPreloading;
    }

    /// <summary>
    /// 사전 로딩을 중지합니다
    /// </summary>
    public void StopPreloading()
    {
        isPreloading = false;
        StopAllCoroutines();
    }
}
