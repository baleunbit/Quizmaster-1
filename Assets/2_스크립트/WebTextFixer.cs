using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 웹빌드에서 텍스트가 안 보이는 문제를 해결하는 스크립트
/// </summary>
public class WebTextFixer : MonoBehaviour
{
    [Header("웹 텍스트 수정 설정")]
    [SerializeField] private float fixDelay = 0.1f;
    [SerializeField] private bool enableTextRefresh = true;

    private void Start()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(FixWebTexts());
        #endif
    }

    private IEnumerator FixWebTexts()
    {
        yield return new WaitForSeconds(fixDelay);
        
        // 모든 TextMeshPro 컴포넌트 찾기 (웹에서 안전하게)
        TextMeshProUGUI[] allTexts = WebBuildBugFixer.SafeFindObjectsOfType<TextMeshProUGUI>();
        
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                FixSingleText(text);
            }
        }
        
        // 추가로 텍스트 새로고침
        if (enableTextRefresh)
        {
            yield return new WaitForSeconds(0.5f);
            RefreshAllTexts();
        }
    }

    private void FixSingleText(TextMeshProUGUI text)
    {
        if (text == null) return;

        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서 안정적인 폰트 설정 (한글 폰트 우선 사용)
            // LiberationSans 폰트만 한글 폰트로 교체 (Maplestory는 유지)
            if (text.font == null || text.font.name.Contains("LiberationSans"))
            {
                // 한글 폰트만 사용 (Arial SDF 폴백 제거)
                text.font = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
                if (text.font == null)
                {
                    Debug.LogError("NanumPenScript-Regular SDF 폰트를 찾을 수 없습니다! 폰트를 교체하지 않습니다.");
                }
            }

            // 한글 문자 지원을 위한 fallback 폰트 설정
            if (text.font != null)
            {
                text.font.fallbackFontAssetTable.Clear();
                var defaultFont = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
                if (defaultFont != null)
                {
                    text.font.fallbackFontAssetTable.Add(defaultFont);
                }
            }

            // 웹에서 텍스트 렌더링 강제 업데이트
            text.SetAllDirty();
            text.ForceMeshUpdate();
            
            // 웹에서 텍스트가 보이도록 컴포넌트 재활성화
            StartCoroutine(ReenableTextComponent(text));
        #endif
    }

    private IEnumerator ReenableTextComponent(TextMeshProUGUI text)
    {
        if (text == null) yield break;

        text.enabled = false;
        yield return null;
        text.enabled = true;
        yield return null;
        
        // 추가로 텍스트 강제 업데이트
        text.SetAllDirty();
        text.ForceMeshUpdate();
    }

    private void RefreshAllTexts()
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
    /// 수동으로 텍스트 수정 실행
    /// </summary>
    [ContextMenu("Fix Web Texts")]
    public void ManualFixTexts()
    {
        StartCoroutine(FixWebTexts());
    }

    /// <summary>
    /// 특정 텍스트만 수정
    /// </summary>
    public void FixSpecificText(TextMeshProUGUI text)
    {
        if (text != null)
        {
            FixSingleText(text);
        }
    }
}
