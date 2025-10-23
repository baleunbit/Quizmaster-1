using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 한글 폰트 렌더링 문제를 해결하는 스크립트
/// LiberationSans SDF 폰트에서 한글 문자가 표시되지 않는 문제를 해결
/// </summary>
public class KoreanFontFixer : MonoBehaviour
{
    [Header("한글 폰트 설정")]
    [SerializeField] private TMP_FontAsset koreanFont;
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private float fixDelay = 0.5f;

    private void Start()
    {
        if (autoFixOnStart)
        {
            StartCoroutine(FixKoreanFonts());
        }
    }

    /// <summary>
    /// 한글 폰트 문제를 수정합니다
    /// </summary>
    private IEnumerator FixKoreanFonts()
    {
        yield return new WaitForSeconds(fixDelay);
        
        // 한글 폰트가 설정되지 않은 경우 기본 폰트 사용
        if (koreanFont == null)
        {
            koreanFont = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
            if (koreanFont == null)
            {
                Debug.LogWarning("한글 폰트를 찾을 수 없습니다. 기본 폰트를 사용합니다.");
                koreanFont = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
            }
        }

        // 모든 TextMeshPro 컴포넌트 찾기 (최신 API 사용)
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                // SubtitleText는 우선적으로 처리
                if (text.name.Contains("SubtitleText"))
                {
                    Debug.Log($"SubtitleText 발견: {text.name} - 우선 처리");
                    FixSingleText(text);
                }
                else
                {
                    FixSingleText(text);
                }
            }
        }
        
        Debug.Log($"한글 폰트 수정 완료: {allTexts.Length}개의 텍스트 컴포넌트 처리됨");
    }

    /// <summary>
    /// 개별 텍스트 컴포넌트의 한글 폰트 문제를 수정합니다
    /// </summary>
    private void FixSingleText(TextMeshProUGUI text)
    {
        if (text == null) return;

        // 한글 폰트로 교체
        if (koreanFont != null)
        {
            text.font = koreanFont;
        }

        // 폰트 fallback 설정
        if (text.font != null)
        {
            // 한글 문자 지원을 위한 fallback 폰트 설정
            text.font.fallbackFontAssetTable.Clear();
            
            // 기본 폰트를 fallback으로 추가
            var defaultFont = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
            if (defaultFont != null)
            {
                text.font.fallbackFontAssetTable.Add(defaultFont);
            }
        }

        // 텍스트 강제 업데이트
        text.SetAllDirty();
        text.ForceMeshUpdate();
        
        // 컴포넌트 재활성화로 렌더링 강제 업데이트
        StartCoroutine(ReenableTextComponent(text));
    }

    /// <summary>
    /// 텍스트 컴포넌트를 재활성화하여 렌더링을 강제 업데이트합니다
    /// </summary>
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

    /// <summary>
    /// 수동으로 한글 폰트 수정 실행
    /// </summary>
    [ContextMenu("Fix Korean Fonts")]
    public void ManualFixKoreanFonts()
    {
        StartCoroutine(FixKoreanFonts());
    }

    /// <summary>
    /// 특정 텍스트만 한글 폰트 수정
    /// </summary>
    public void FixSpecificText(TextMeshProUGUI text)
    {
        if (text != null)
        {
            FixSingleText(text);
        }
    }

    /// <summary>
    /// 모든 텍스트를 새로고침합니다
    /// </summary>
    public void RefreshAllTexts()
    {
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                text.SetAllDirty();
                text.ForceMeshUpdate();
            }
        }
    }
}
