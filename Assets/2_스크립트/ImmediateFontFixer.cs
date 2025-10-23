using UnityEngine;
using TMPro;

/// <summary>
/// 즉시 실행되는 폰트 교체 스크립트
/// </summary>
public class ImmediateFontFixer : MonoBehaviour
{
    [Header("즉시 폰트 교체 설정")]
    [SerializeField] private TMP_FontAsset koreanFont;
    [SerializeField] private bool fixOnAwake = true;
    [SerializeField] private bool fixOnStart = true;

    private void Awake()
    {
        if (fixOnAwake)
        {
            FixFontsImmediately();
        }
    }

    private void Start()
    {
        if (fixOnStart)
        {
            FixFontsImmediately();
        }
    }

    /// <summary>
    /// 즉시 폰트 교체
    /// </summary>
    private void FixFontsImmediately()
    {
        Debug.Log("=== 즉시 폰트 교체 시작 ===");
        
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

        // 모든 TextMeshPro 컴포넌트 찾기
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        int fixedCount = 0;
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                // LiberationSans 또는 Maplestory 폰트 사용 시 교체
                if (text.font != null && 
                    (text.font.name.Contains("LiberationSans") || 
                     text.font.name.Contains("Maplestory") ||
                     text.font.name.Contains("Arial")))
                {
                    Debug.Log($"폰트 교체: {text.name} - {text.font.name} → {koreanFont.name}");
                    text.font = koreanFont;
                    fixedCount++;
                }
            }
        }
        
        Debug.Log($"=== 즉시 폰트 교체 완료: {fixedCount}개 교체됨 ===");
    }

    /// <summary>
    /// 수동으로 폰트 교체
    /// </summary>
    [ContextMenu("Fix Fonts Now")]
    public void ManualFixFonts()
    {
        FixFontsImmediately();
    }

    /// <summary>
    /// 모든 텍스트를 한글 폰트로 강제 교체
    /// </summary>
    [ContextMenu("Force All Fonts to Korean")]
    public void ForceAllFontsToKorean()
    {
        if (koreanFont == null)
        {
            koreanFont = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
        }

        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                Debug.Log($"강제 폰트 교체: {text.name} - {text.font?.name} → {koreanFont.name}");
                text.font = koreanFont;
            }
        }
        
        Debug.Log("모든 텍스트를 한글 폰트로 강제 교체 완료");
    }
}
