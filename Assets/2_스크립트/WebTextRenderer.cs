using UnityEngine;
using TMPro;

/// <summary>
/// 웹빌드에서 텍스트 렌더링 문제를 해결하는 유틸리티 클래스
/// </summary>
public class WebTextRenderer : MonoBehaviour
{
    [Header("웹 텍스트 렌더링 설정")]
    [SerializeField] private TMP_FontAsset fallbackFont;

    private void Awake()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            InitializeWebTextRendering();
        #endif
    }

    private void InitializeWebTextRendering()
    {
        // 웹에서 안정적인 폰트 설정
        if (fallbackFont == null)
        {
            fallbackFont = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
        }

        // 모든 TextMeshPro 컴포넌트에 웹 최적화 적용 (웹에서 안전하게)
        TextMeshProUGUI[] allTexts = WebBuildBugFixer.SafeFindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            OptimizeTextForWeb(text);
        }
    }

    /// <summary>
    /// 웹에서 텍스트 렌더링을 최적화합니다
    /// </summary>
    public static void OptimizeTextForWeb(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서 안정적인 폰트 설정
            // LiberationSans 폰트만 한글 폰트로 교체 (Maplestory는 유지)
            if (textComponent.font == null || textComponent.font.name.Contains("LiberationSans"))
            {
                // 한글 폰트만 사용 (Arial SDF 폴백 제거)
                textComponent.font = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
                if (textComponent.font == null)
                {
                    Debug.LogError("NanumPenScript-Regular SDF 폰트를 찾을 수 없습니다! 폰트를 교체하지 않습니다.");
                }
            }

            // 웹에서 텍스트 렌더링 최적화
            textComponent.raycastTarget = false; // 웹에서 성능 향상
            textComponent.richText = false; // 웹에서 안정성 향상
            
            // 텍스트 강제 업데이트
            textComponent.SetAllDirty();
            textComponent.ForceMeshUpdate();
            
            // 웹에서 텍스트가 보이도록 강제 렌더링
            textComponent.enabled = false;
            textComponent.enabled = true;
        #endif
    }

    /// <summary>
    /// 웹에서 텍스트를 안전하게 설정합니다
    /// </summary>
    public static void SetTextSafely(TextMeshProUGUI textComponent, string text)
    {
        if (textComponent == null) return;

        textComponent.text = text;
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서 텍스트 강제 업데이트
            textComponent.SetAllDirty();
            textComponent.ForceMeshUpdate();
            
            // 웹에서 텍스트가 보이도록 강제 렌더링
            StartCoroutine(ForceTextUpdate(textComponent));
        #endif
    }

    /// <summary>
    /// 웹에서 텍스트 업데이트를 강제합니다
    /// </summary>
    private static System.Collections.IEnumerator ForceTextUpdate(TextMeshProUGUI textComponent)
    {
        yield return new WaitForEndOfFrame();
        
        if (textComponent != null)
        {
            textComponent.enabled = false;
            yield return null;
            textComponent.enabled = true;
        }
    }

    /// <summary>
    /// 웹에서 모든 텍스트를 새로고침합니다
    /// </summary>
    public static void RefreshAllTexts()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            TextMeshProUGUI[] allTexts = WebBuildBugFixer.SafeFindObjectsOfType<TextMeshProUGUI>();
            foreach (var text in allTexts)
            {
                if (text != null)
                {
                    text.SetAllDirty();
                    text.ForceMeshUpdate();
                }
            }
        #endif
    }

    /// <summary>
    /// 웹에서 한글 폰트를 안전하게 설정합니다
    /// </summary>
    public static void SetKoreanFontSafely(TextMeshProUGUI textComponent, TMP_FontAsset koreanFont)
    {
        if (textComponent == null) return;

        // 한글 폰트가 없는 경우 NanumPenScript-Regular SDF 사용
        if (koreanFont == null)
        {
            koreanFont = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
            if (koreanFont == null)
            {
                Debug.LogWarning("한글 폰트를 찾을 수 없습니다. 기본 폰트를 사용합니다.");
                koreanFont = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
            }
        }

        textComponent.font = koreanFont;
        
        // 한글 문자 지원을 위한 fallback 폰트 설정
        if (textComponent.font != null)
        {
            textComponent.font.fallbackFontAssetTable.Clear();
            var defaultFont = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
            if (defaultFont != null)
            {
                textComponent.font.fallbackFontAssetTable.Add(defaultFont);
            }
        }
        
        OptimizeTextForWeb(textComponent);
    }
}
