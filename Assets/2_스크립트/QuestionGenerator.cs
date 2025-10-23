using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 문제 생성 최적화 시스템
/// </summary>
public class QuestionGenerator : MonoBehaviour
{
    [Header("생성 최적화")]
    [SerializeField] private int defaultQuestionCount = 5; // 기본 문제 수
    [SerializeField] private bool enableBatchGeneration = true; // 배치 생성 활성화

    private ChatGPTClient chatGPTClient;
    private QuestionCache questionCache;

    private void Start()
    {
        chatGPTClient = WebBuildBugFixer.SafeFindObjectsOfType<ChatGPTClient>().FirstOrDefault();
        questionCache = QuestionCache.instance;
    }

    /// <summary>
    /// 최적화된 문제 생성
    /// </summary>
    public void GenerateOptimizedQuestions(string topic, int count)
    {
        // 캐시에서 문제 확인
        if (questionCache != null && questionCache.HasEnoughQuestions(topic, count))
        {
            Debug.Log($"캐시에서 {count}개 문제 로드: {topic}");
            return;
        }

        // 배치 생성으로 효율성 향상
        if (enableBatchGeneration && count > defaultQuestionCount)
        {
            GenerateBatchQuestions(topic, count);
        }
        else
        {
            chatGPTClient.GenerateQuestions(count, topic);
        }
    }

    private void GenerateBatchQuestions(string topic, int totalCount)
    {
        int batchSize = Mathf.Min(defaultQuestionCount, totalCount);
        int batches = Mathf.CeilToInt((float)totalCount / batchSize);

        for (int i = 0; i < batches; i++)
        {
            int currentBatchSize = Mathf.Min(batchSize, totalCount - (i * batchSize));
            chatGPTClient.GenerateQuestions(currentBatchSize, topic);
        }
    }

    /// <summary>
    /// 문제 생성 상태 확인
    /// </summary>
    public bool IsGenerating(string topic)
    {
        return questionCache != null && !questionCache.HasEnoughQuestions(topic, 1);
    }
}
