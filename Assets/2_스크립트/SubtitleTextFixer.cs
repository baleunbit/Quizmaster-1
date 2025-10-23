using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// SubtitleText (1)의 한글 폰트 문제를 해결하는 전용 스크립트
/// </summary>
public class SubtitleTextFixer : MonoBehaviour
{
    [Header("SubtitleText 수정 설정")]
    [SerializeField] private TMP_FontAsset koreanFont;
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private float fixDelay = 1.0f;

    private void Awake()
    {
        // Awake에서 즉시 실행
        SimpleFixSubtitleText();
    }

    private void Start()
    {
        if (autoFixOnStart)
        {
            // Start에서도 실행
            Invoke(nameof(SimpleFixSubtitleText), 0.5f);
            Invoke(nameof(SimpleFixSubtitleText), 2.0f);
            Invoke(nameof(SimpleFixSubtitleText), 5.0f);
        }
    }
    
    /// <summary>
    /// 간단한 SubtitleText 수정
    /// </summary>
    private void SimpleFixSubtitleText()
    {
        Debug.Log("SubtitleText 간단 수정 시작");
        
        // 한글 폰트 설정
        if (koreanFont == null)
        {
            koreanFont = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
            if (koreanFont == null)
            {
                Debug.LogError("한글 폰트를 찾을 수 없습니다!");
                return;
            }
        }

        // SubtitleText 찾기
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        foreach (var text in allTexts)
        {
            if (text != null && text.name.Contains("SubtitleText"))
            {
                Debug.Log($"SubtitleText 발견: {text.name}");
                text.font = koreanFont;
                Debug.Log($"폰트 교체: {text.font.name}");
            }
        }
        
        Debug.Log("SubtitleText 간단 수정 완료");
    }

    /// <summary>
    /// SubtitleText (1)의 한글 폰트 문제를 수정합니다
    /// </summary>
    private IEnumerator FixSubtitleText()
    {
        yield return new WaitForSeconds(fixDelay);
        
        // 한글 폰트 설정
        if (koreanFont == null)
        {
            koreanFont = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
            if (koreanFont == null)
            {
                Debug.LogWarning("한글 폰트를 찾을 수 없습니다. 기본 폰트를 사용합니다.");
                koreanFont = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
            }
        }

        // SubtitleText (1) 찾기 - 여러 번 시도
        TextMeshProUGUI subtitleText = null;
        for (int attempt = 0; attempt < 10; attempt++)
        {
            subtitleText = FindSubtitleText();
            if (subtitleText != null) 
            {
                Debug.Log($"SubtitleText 발견 (시도 {attempt + 1}): {subtitleText.name}");
                break;
            }
            yield return new WaitForSeconds(0.2f);
        }
        
        if (subtitleText != null)
        {
            Debug.Log($"SubtitleText 발견: {subtitleText.name}");
            
            // 여러 번 폰트 수정
            for (int i = 0; i < 3; i++)
            {
                FixSubtitleTextFont(subtitleText);
                yield return new WaitForSeconds(0.5f);
            }
            
            Debug.Log("SubtitleText (1) 한글 폰트 수정 완료");
        }
        else
        {
            Debug.LogWarning("SubtitleText (1)을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// SubtitleText (1) 컴포넌트를 찾습니다
    /// </summary>
    private TextMeshProUGUI FindSubtitleText()
    {
        // 모든 TextMeshPro 컴포넌트 검색 (최신 API 사용)
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        foreach (var text in allTexts)
        {
            if (text != null && text.name.Contains("SubtitleText"))
            {
                Debug.Log($"SubtitleText 발견: {text.name}");
                return text;
            }
        }
        
        return null;
    }

    /// <summary>
    /// SubtitleText의 한글 폰트를 수정합니다
    /// </summary>
    private void FixSubtitleTextFont(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        Debug.Log($"SubtitleText 폰트 수정 시작: {textComponent.name}");
        Debug.Log($"현재 폰트: {textComponent.font?.name}");

        // 한글 폰트 적용
        textComponent.font = koreanFont;
        Debug.Log($"한글 폰트 적용됨: {koreanFont?.name}");
        
        // 폰트 fallback 설정
        if (textComponent.font != null)
        {
            textComponent.font.fallbackFontAssetTable.Clear();
            
            // 기본 폰트를 fallback으로 추가
            var defaultFont = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
            if (defaultFont != null)
            {
                textComponent.font.fallbackFontAssetTable.Add(defaultFont);
                Debug.Log("Arial SDF 폰트를 fallback으로 추가");
            }
        }
        
        // 폰트 강제 재설정
        textComponent.font = koreanFont;
        Debug.Log($"폰트 강제 재설정: {textComponent.font?.name}");

        // 텍스트 강제 업데이트
        textComponent.SetAllDirty();
        textComponent.ForceMeshUpdate();
        
        // 컴포넌트 재활성화로 렌더링 강제 업데이트
        StartCoroutine(ReenableSubtitleText(textComponent));
    }

    /// <summary>
    /// SubtitleText 컴포넌트를 재활성화합니다
    /// </summary>
    private IEnumerator ReenableSubtitleText(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) yield break;

        // 여러 번 재활성화하여 폰트 적용 강화
        for (int i = 0; i < 3; i++)
        {
            textComponent.enabled = false;
            yield return new WaitForEndOfFrame();
            textComponent.enabled = true;
            yield return new WaitForEndOfFrame();
            
            // 텍스트 강제 업데이트
            textComponent.SetAllDirty();
            textComponent.ForceMeshUpdate();
        }
        
        Debug.Log("SubtitleText 재활성화 완료");
    }

    /// <summary>
    /// 수동으로 SubtitleText 수정 실행
    /// </summary>
    [ContextMenu("Fix SubtitleText")]
    public void ManualFixSubtitleText()
    {
        SimpleFixSubtitleText();
    }
    
    /// <summary>
    /// 간단한 수동 수정
    /// </summary>
    [ContextMenu("Simple Fix")]
    public void SimpleManualFix()
    {
        SimpleFixSubtitleText();
    }

    /// <summary>
    /// 모든 SubtitleText 관련 텍스트를 새로고침합니다
    /// </summary>
    public void RefreshAllSubtitleTexts()
    {
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var text in allTexts)
        {
            if (text != null && text.name.Contains("SubtitleText"))
            {
                text.SetAllDirty();
                text.ForceMeshUpdate();
            }
        }
    }

    /// <summary>
    /// 강제로 모든 SubtitleText 폰트를 한글 폰트로 변경합니다
    /// </summary>
    [ContextMenu("Force Fix All SubtitleTexts")]
    public void ForceFixAllSubtitleTexts()
    {
        StartCoroutine(ForceFixAllSubtitleTextsCoroutine());
    }

    private IEnumerator ForceFixAllSubtitleTextsCoroutine()
    {
        // 한글 폰트 설정
        if (koreanFont == null)
        {
            koreanFont = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
            if (koreanFont == null)
            {
                koreanFont = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
            }
        }

        // 모든 SubtitleText 찾기
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var text in allTexts)
        {
            if (text != null && text.name.Contains("SubtitleText"))
            {
                Debug.Log($"강제 폰트 수정: {text.name}");
                
                // 폰트 강제 변경
                text.font = koreanFont;
                
                // 폰트 fallback 설정
                if (text.font != null)
                {
                    text.font.fallbackFontAssetTable.Clear();
                    var defaultFont = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
                    if (defaultFont != null)
                    {
                        text.font.fallbackFontAssetTable.Add(defaultFont);
                    }
                }
                
                // 강제 업데이트
                text.SetAllDirty();
                text.ForceMeshUpdate();
                
                yield return new WaitForEndOfFrame();
            }
        }
        
        Debug.Log("모든 SubtitleText 강제 폰트 수정 완료");
    }

    /// <summary>
    /// SubtitleText 폰트 상태 확인
    /// </summary>
    [ContextMenu("Check SubtitleText Font Status")]
    public void CheckSubtitleTextFontStatus()
    {
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var text in allTexts)
        {
            if (text != null && text.name.Contains("SubtitleText"))
            {
                Debug.Log($"SubtitleText 폰트 상태: {text.name} - 폰트: {text.font?.name}");
            }
        }
    }
}
