using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Unity JobTempAlloc 메모리 누수 문제를 해결하는 스크립트
/// </summary>
public class MemoryLeakFixer : MonoBehaviour
{
    [Header("메모리 누수 수정 설정")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private float fixInterval = 5.0f; // 5초마다 메모리 정리
    [SerializeField] private bool enableGarbageCollection = true;

    private void Start()
    {
        if (autoFixOnStart)
        {
            StartCoroutine(FixMemoryLeaks());
        }
    }

    /// <summary>
    /// 메모리 누수 문제를 주기적으로 수정합니다
    /// </summary>
    private IEnumerator FixMemoryLeaks()
    {
        while (true)
        {
            yield return new WaitForSeconds(fixInterval);
            
            // 메모리 정리 실행
            CleanupMemory();
        }
    }

    /// <summary>
    /// 메모리를 정리합니다
    /// </summary>
    private void CleanupMemory()
    {
        // 가비지 컬렉션 강제 실행
        if (enableGarbageCollection)
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
        }

        // Unity 리소스 정리
        Resources.UnloadUnusedAssets();

        // 텍스트 메시 정리
        CleanupTextMeshes();

        Debug.Log("메모리 정리 완료");
    }

    /// <summary>
    /// TextMeshPro 관련 메모리 정리
    /// </summary>
    private void CleanupTextMeshes()
    {
        // 모든 TextMeshPro 컴포넌트 찾기
        var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                // 텍스트 메시 강제 업데이트
                text.SetAllDirty();
                text.ForceMeshUpdate();
            }
        }
    }

    /// <summary>
    /// 수동으로 메모리 정리 실행
    /// </summary>
    [ContextMenu("Cleanup Memory")]
    public void ManualCleanupMemory()
    {
        CleanupMemory();
    }

    /// <summary>
    /// 메모리 사용량 정보 출력
    /// </summary>
    [ContextMenu("Show Memory Info")]
    public void ShowMemoryInfo()
    {
        long totalMemory = System.GC.GetTotalMemory(false);
        long totalMemoryKB = totalMemory / 1024;
        long totalMemoryMB = totalMemoryKB / 1024;
        
        Debug.Log($"메모리 사용량: {totalMemoryMB}MB ({totalMemoryKB}KB)");
    }

    private void OnDestroy()
    {
        // 객체 파괴 시 메모리 정리
        CleanupMemory();
    }
}
