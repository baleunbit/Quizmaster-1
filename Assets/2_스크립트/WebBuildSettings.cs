using UnityEngine;
using TMPro;

/// <summary>
/// 웹빌드 전용 설정을 관리하는 클래스
/// </summary>
public class WebBuildSettings : MonoBehaviour
{
    [Header("웹빌드 텍스트 설정")]
    [SerializeField] private bool enableWebTextOptimization = true;
    [SerializeField] private bool forceTextMeshUpdate = true;
    [SerializeField] private bool disableRichText = true;
    [SerializeField] private bool enableTextRaycast = false;

    [Header("폰트 설정")]
    [SerializeField] private TMP_FontAsset webFallbackFont;
    [SerializeField] private bool useBuiltinFont = true;

    private void Awake()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            ApplyWebBuildSettings();
        #endif
    }

    private void ApplyWebBuildSettings()
    {
        if (!enableWebTextOptimization) return;

        // 모든 TextMeshPro 컴포넌트에 웹 최적화 적용 (웹에서 안전하게)
        TextMeshProUGUI[] allTexts = WebBuildBugFixer.SafeFindObjectsOfType<TextMeshProUGUI>();
        
        foreach (var text in allTexts)
        {
            ApplyWebOptimizations(text);
        }

        // 웹에서 안정적인 렌더링을 위한 추가 설정
        if (forceTextMeshUpdate)
        {
            StartCoroutine(ForceTextMeshUpdate());
        }
    }

    private void ApplyWebOptimizations(TextMeshProUGUI text)
    {
        if (text == null) return;

        // 웹에서 안정적인 폰트 설정
        if (text.font == null || useBuiltinFont)
        {
            text.font = webFallbackFont != null ? webFallbackFont : 
                       Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
        }

        // 웹에서 성능 최적화
        if (disableRichText)
        {
            text.richText = false;
        }

        if (!enableTextRaycast)
        {
            text.raycastTarget = false;
        }

        // 웹에서 텍스트 렌더링 강제 업데이트
        text.SetAllDirty();
        text.ForceMeshUpdate();
    }

    private System.Collections.IEnumerator ForceTextMeshUpdate()
    {
        yield return new WaitForEndOfFrame();
        
        TextMeshProUGUI[] allTexts = WebBuildBugFixer.SafeFindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                text.SetAllDirty();
                text.ForceMeshUpdate();
            }
        }
    }

    /// <summary>
    /// 웹에서 텍스트를 안전하게 설정
    /// </summary>
    public static void SetWebText(TextMeshProUGUI textComponent, string text)
    {
        if (textComponent == null) return;

        textComponent.text = text;
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서 텍스트 강제 업데이트
            textComponent.SetAllDirty();
            textComponent.ForceMeshUpdate();
            
            // 웹에서 텍스트가 보이도록 강제 렌더링
            textComponent.enabled = false;
            textComponent.enabled = true;
        #endif
    }

    /// <summary>
    /// 웹에서 폰트를 안전하게 설정
    /// </summary>
    public static void SetWebFont(TextMeshProUGUI textComponent, TMP_FontAsset font)
    {
        if (textComponent == null) return;

        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서는 기본 폰트 사용 (폰트가 없을 경우)
            if (font == null)
            {
                font = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
            }
        #endif

        textComponent.font = font;
        textComponent.SetAllDirty();
        textComponent.ForceMeshUpdate();
    }
}
