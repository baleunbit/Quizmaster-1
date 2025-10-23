using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Unity 에디터에서 직접 폰트를 교체하는 스크립트
/// LiberationSans SDF를 NanumPenScript-Regular SDF로 교체
/// </summary>
public class FontReplacer : MonoBehaviour
{
    [Header("폰트 교체 설정")]
    [SerializeField] private TMP_FontAsset targetFont; // 교체할 폰트
    [SerializeField] private bool replaceOnStart = true;

    private void Start()
    {
        if (replaceOnStart)
        {
            StartCoroutine(ReplaceFonts());
        }
    }

    /// <summary>
    /// 모든 폰트를 교체합니다
    /// </summary>
    private IEnumerator ReplaceFonts()
    {
        yield return new WaitForSeconds(1.0f);
        
        // 교체할 폰트 설정
        if (targetFont == null)
        {
            targetFont = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
            if (targetFont == null)
            {
                Debug.LogError("NanumPenScript-Regular SDF 폰트를 찾을 수 없습니다!");
                yield break;
            }
        }

        Debug.Log($"폰트 교체 시작: {targetFont.name}");

        // 모든 TextMeshPro 컴포넌트 찾기
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        int replacedCount = 0;
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                // LiberationSans SDF 폰트만 교체 (Maplestory는 유지)
                if (text.font != null && text.font.name.Contains("LiberationSans"))
                {
                    Debug.Log($"폰트 교체: {text.name} - {text.font.name} → {targetFont.name}");
                    
                    // 폰트 교체
                    text.font = targetFont;
                    
                    // 폰트 fallback 설정
                    SetupFontFallback(text);
                    
                    // 텍스트 강제 업데이트
                    text.SetAllDirty();
                    text.ForceMeshUpdate();
                    
                    replacedCount++;
                }
            }
        }
        
        Debug.Log($"폰트 교체 완료: {replacedCount}개의 텍스트 컴포넌트 교체됨");
    }

    /// <summary>
    /// 폰트 fallback 설정
    /// </summary>
    private void SetupFontFallback(TextMeshProUGUI textComponent)
    {
        if (textComponent.font != null)
        {
            textComponent.font.fallbackFontAssetTable.Clear();
            
            // Arial SDF를 fallback으로 추가
            var arialFont = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
            if (arialFont != null)
            {
                textComponent.font.fallbackFontAssetTable.Add(arialFont);
            }
        }
    }

    /// <summary>
    /// 수동으로 폰트 교체 실행
    /// </summary>
    [ContextMenu("Replace All Fonts")]
    public void ManualReplaceFonts()
    {
        StartCoroutine(ReplaceFonts());
    }

    /// <summary>
    /// 특정 폰트만 교체
    /// </summary>
    [ContextMenu("Replace LiberationSans Only")]
    public void ReplaceLiberationSansOnly()
    {
        StartCoroutine(ReplaceLiberationSansOnlyCoroutine());
    }

    private IEnumerator ReplaceLiberationSansOnlyCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (targetFont == null)
        {
            targetFont = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
        }

        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        foreach (var text in allTexts)
        {
            if (text != null && text.font != null && text.font.name.Contains("LiberationSans"))
            {
                Debug.Log($"LiberationSans 폰트 교체: {text.name}");
                text.font = targetFont;
                SetupFontFallback(text);
                text.SetAllDirty();
                text.ForceMeshUpdate();
            }
        }
        
        Debug.Log("LiberationSans 폰트 교체 완료");
    }

    /// <summary>
    /// 현재 사용 중인 폰트 확인
    /// </summary>
    [ContextMenu("Check Current Fonts")]
    public void CheckCurrentFonts()
    {
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        Debug.Log("=== 현재 사용 중인 폰트 목록 ===");
        foreach (var text in allTexts)
        {
            if (text != null && text.font != null)
            {
                Debug.Log($"텍스트: {text.name} - 폰트: {text.font.name}");
            }
        }
    }
}
