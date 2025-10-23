using UnityEngine;
using TMPro;

/// <summary>
/// 간단한 폰트 수정 스크립트
/// </summary>
public class SimpleFontFixer : MonoBehaviour
{
    [Header("간단한 폰트 수정")]
    [SerializeField] private TMP_FontAsset koreanFont;
    [SerializeField] private bool fixOnStart = true;

    private void Awake()
    {
        // Awake에서 즉시 실행
        FixFonts();
    }

    private void Start()
    {
        if (fixOnStart)
        {
            // 여러 번 실행
            Invoke(nameof(FixFonts), 0.5f);
            Invoke(nameof(FixFonts), 1.0f);
            Invoke(nameof(FixFonts), 3.0f);
        }
    }

    /// <summary>
    /// 폰트 수정
    /// </summary>
    private void FixFonts()
    {
        Debug.Log("폰트 수정 시작");
        
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

        // 모든 텍스트 찾기
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        int fixedCount = 0;
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                // LiberationSans 폰트만 수정 (Maplestory는 유지)
                if (text.font != null && text.font.name.Contains("LiberationSans"))
                {
                    Debug.Log($"폰트 수정: {text.name}");
                    text.font = koreanFont;
                    fixedCount++;
                }
            }
        }
        
        Debug.Log($"폰트 수정 완료: {fixedCount}개 수정됨");
    }

    /// <summary>
    /// 수동으로 폰트 수정
    /// </summary>
    [ContextMenu("Fix Fonts")]
    public void ManualFixFonts()
    {
        FixFonts();
    }

    /// <summary>
    /// SubtitleText만 수정
    /// </summary>
    [ContextMenu("Fix SubtitleText Only")]
    public void FixSubtitleTextOnly()
    {
        if (koreanFont == null)
        {
            koreanFont = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
        }

        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        foreach (var text in allTexts)
        {
            if (text != null && text.name.Contains("SubtitleText"))
            {
                Debug.Log($"SubtitleText 폰트 수정: {text.name}");
                text.font = koreanFont;
            }
        }
        
        Debug.Log("SubtitleText 폰트 수정 완료");
    }
}
