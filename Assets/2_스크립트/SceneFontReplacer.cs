using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 씬의 모든 텍스트를 직접 찾아서 폰트를 교체하는 스크립트
/// </summary>
public class SceneFontReplacer : MonoBehaviour
{
    [Header("씬 폰트 교체 설정")]
    [SerializeField] private TMP_FontAsset replacementFont;
    [SerializeField] private bool replaceOnStart = true;
    [SerializeField] private float searchInterval = 2.0f;

    private void Start()
    {
        if (replaceOnStart)
        {
            StartCoroutine(ContinuousFontReplacement());
        }
    }

    /// <summary>
    /// 지속적으로 폰트를 교체합니다
    /// </summary>
    private IEnumerator ContinuousFontReplacement()
    {
        while (true)
        {
            yield return new WaitForSeconds(searchInterval);
            ReplaceAllTextFonts();
        }
    }

    /// <summary>
    /// 모든 텍스트의 폰트를 교체합니다
    /// </summary>
    private void ReplaceAllTextFonts()
    {
        // 교체할 폰트 설정
        if (replacementFont == null)
        {
            replacementFont = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
            if (replacementFont == null)
            {
                Debug.LogWarning("교체할 폰트를 찾을 수 없습니다.");
                return;
            }
        }

        // 모든 TextMeshPro 컴포넌트 찾기
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        int replacedCount = 0;
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                // LiberationSans SDF 폰트를 사용하는 텍스트 찾기
                if (text.font != null && text.font.name.Contains("LiberationSans"))
                {
                    // 폰트 교체
                    text.font = replacementFont;
                    
                    // 폰트 fallback 설정
                    SetupFontFallback(text);
                    
                    // 텍스트 강제 업데이트
                    text.SetAllDirty();
                    text.ForceMeshUpdate();
                    
                    replacedCount++;
                }
            }
        }
        
        if (replacedCount > 0)
        {
            Debug.Log($"폰트 교체 완료: {replacedCount}개의 텍스트 교체됨");
        }
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
    [ContextMenu("Replace All Fonts Now")]
    public void ManualReplaceAllFonts()
    {
        ReplaceAllTextFonts();
    }

    /// <summary>
    /// SubtitleText만 교체
    /// </summary>
    [ContextMenu("Replace SubtitleText Only")]
    public void ReplaceSubtitleTextOnly()
    {
        if (replacementFont == null)
        {
            replacementFont = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
        }

        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        foreach (var text in allTexts)
        {
            if (text != null && text.name.Contains("SubtitleText"))
            {
                Debug.Log($"SubtitleText 폰트 교체: {text.name}");
                text.font = replacementFont;
                SetupFontFallback(text);
                text.SetAllDirty();
                text.ForceMeshUpdate();
            }
        }
        
        Debug.Log("SubtitleText 폰트 교체 완료");
    }

    /// <summary>
    /// 현재 폰트 상태 확인
    /// </summary>
    [ContextMenu("Check Current Font Status")]
    public void CheckCurrentFontStatus()
    {
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        Debug.Log("=== 현재 폰트 상태 ===");
        foreach (var text in allTexts)
        {
            if (text != null && text.font != null)
            {
                string status = text.font.name.Contains("LiberationSans") ? "⚠️ LiberationSans" : "✅ 다른 폰트";
                Debug.Log($"{status}: {text.name} - {text.font.name}");
            }
        }
    }
}
