using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 폰트 문제를 진단하는 스크립트
/// </summary>
public class FontDiagnostic : MonoBehaviour
{
    [Header("진단 설정")]
    [SerializeField] private bool runDiagnosticOnStart = true;
    [SerializeField] private float diagnosticInterval = 5.0f;

    private void Start()
    {
        if (runDiagnosticOnStart)
        {
            StartCoroutine(RunDiagnostic());
        }
    }

    /// <summary>
    /// 폰트 진단 실행
    /// </summary>
    private IEnumerator RunDiagnostic()
    {
        while (true)
        {
            yield return new WaitForSeconds(diagnosticInterval);
            DiagnoseFonts();
        }
    }

    /// <summary>
    /// 폰트 상태 진단
    /// </summary>
    private void DiagnoseFonts()
    {
        Debug.Log("=== 폰트 진단 시작 ===");
        
        // 모든 TextMeshPro 컴포넌트 찾기
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        int liberationSansCount = 0;
        int koreanFontCount = 0;
        int nullFontCount = 0;
        
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                if (text.font == null)
                {
                    nullFontCount++;
                    Debug.LogWarning($"폰트 없음: {text.name}");
                }
                else if (text.font.name.Contains("LiberationSans"))
                {
                    liberationSansCount++;
                    Debug.LogWarning($"문제 폰트 사용: {text.name} - {text.font.name}");
                }
                else if (text.font.name.Contains("NanumPenScript") || text.font.name.Contains("Maplestory"))
                {
                    koreanFontCount++;
                    Debug.Log($"한글 폰트 사용: {text.name} - {text.font.name}");
                }
                else
                {
                    Debug.Log($"기타 폰트 사용: {text.name} - {text.font.name}");
                }
            }
        }
        
        Debug.Log($"=== 폰트 진단 결과 ===");
        Debug.Log($"문제 폰트 사용: {liberationSansCount}개");
        Debug.Log($"한글 폰트 사용: {koreanFontCount}개");
        Debug.Log($"폰트 없음: {nullFontCount}개");
        Debug.Log($"총 텍스트: {allTexts.Length}개");
    }

    /// <summary>
    /// 수동으로 진단 실행
    /// </summary>
    [ContextMenu("Run Font Diagnostic")]
    public void ManualRunDiagnostic()
    {
        DiagnoseFonts();
    }

    /// <summary>
    /// SubtitleText만 진단
    /// </summary>
    [ContextMenu("Diagnose SubtitleText Only")]
    public void DiagnoseSubtitleTextOnly()
    {
        Debug.Log("=== SubtitleText 진단 ===");
        
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        foreach (var text in allTexts)
        {
            if (text != null && text.name.Contains("SubtitleText"))
            {
                Debug.Log($"SubtitleText 발견: {text.name}");
                Debug.Log($"  폰트: {text.font?.name ?? "없음"}");
                Debug.Log($"  활성화: {text.enabled}");
                Debug.Log($"  텍스트: {text.text}");
            }
        }
    }
}
