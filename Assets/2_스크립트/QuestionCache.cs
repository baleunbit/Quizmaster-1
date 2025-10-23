using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 문제 캐싱 시스템 - 로딩 시간 단축
/// </summary>
public class QuestionCache : MonoBehaviour
{
    [Header("캐시 설정")]
    [SerializeField] private int maxCacheSize = 50; // 최대 캐시 문제 수

    private Dictionary<string, List<QuestionSO>> questionCache = new Dictionary<string, List<QuestionSO>>();
    private Dictionary<string, float> lastCacheTime = new Dictionary<string, float>();
    private float cacheExpireTime = 300f; // 5분 후 캐시 만료

    public static QuestionCache instance { get; private set; }

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

    /// <summary>
    /// 캐시된 문제를 가져옵니다
    /// </summary>
    public List<QuestionSO> GetCachedQuestions(string topic, int count)
    {
        if (!questionCache.ContainsKey(topic))
        {
            return new List<QuestionSO>();
        }

        // 캐시 만료 확인
        if (lastCacheTime.ContainsKey(topic) && 
            Time.time - lastCacheTime[topic] > cacheExpireTime)
        {
            questionCache.Remove(topic);
            lastCacheTime.Remove(topic);
            return new List<QuestionSO>();
        }

        List<QuestionSO> cachedQuestions = questionCache[topic];
        List<QuestionSO> result = new List<QuestionSO>();

        // 요청한 수만큼 문제 반환
        for (int i = 0; i < Mathf.Min(count, cachedQuestions.Count); i++)
        {
            result.Add(cachedQuestions[i]);
        }

        // 사용된 문제는 캐시에서 제거
        for (int i = 0; i < result.Count; i++)
        {
            cachedQuestions.RemoveAt(0);
        }

        return result;
    }

    /// <summary>
    /// 문제를 캐시에 저장합니다
    /// </summary>
    public void CacheQuestions(string topic, List<QuestionSO> questions)
    {
        if (!questionCache.ContainsKey(topic))
        {
            questionCache[topic] = new List<QuestionSO>();
        }

        questionCache[topic].AddRange(questions);
        lastCacheTime[topic] = Time.time;

        // 캐시 크기 제한
        if (questionCache[topic].Count > maxCacheSize)
        {
            questionCache[topic].RemoveRange(0, questionCache[topic].Count - maxCacheSize);
        }
    }

    /// <summary>
    /// 캐시에 문제가 충분한지 확인합니다
    /// </summary>
    public bool HasEnoughQuestions(string topic, int count)
    {
        if (!questionCache.ContainsKey(topic))
            return false;

        return questionCache[topic].Count >= count;
    }

    /// <summary>
    /// 특정 주제의 캐시를 초기화합니다
    /// </summary>
    public void ClearCache(string topic)
    {
        if (questionCache.ContainsKey(topic))
        {
            questionCache.Remove(topic);
            lastCacheTime.Remove(topic);
        }
    }

    /// <summary>
    /// 모든 캐시를 초기화합니다
    /// </summary>
    public void ClearAllCache()
    {
        questionCache.Clear();
        lastCacheTime.Clear();
    }

    /// <summary>
    /// 캐시 상태를 반환합니다
    /// </summary>
    public string GetCacheStatus()
    {
        string status = "캐시 상태:\n";
        foreach (var kvp in questionCache)
        {
            status += $"{kvp.Key}: {kvp.Value.Count}개 문제\n";
        }
        return status;
    }
}
