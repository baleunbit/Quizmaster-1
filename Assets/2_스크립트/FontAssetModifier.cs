using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 폰트 에셋 자체를 수정하여 한글 문자 지원을 강화하는 스크립트
/// </summary>
public class FontAssetModifier : MonoBehaviour
{
    [Header("폰트 에셋 수정 설정")]
    [SerializeField] private TMP_FontAsset liberationSansFont;
    [SerializeField] private TMP_FontAsset koreanFont;
    [SerializeField] private bool modifyOnStart = true;

    private void Start()
    {
        if (modifyOnStart)
        {
            StartCoroutine(ModifyFontAssets());
        }
    }

    /// <summary>
    /// 폰트 에셋을 수정합니다
    /// </summary>
    private IEnumerator ModifyFontAssets()
    {
        yield return new WaitForSeconds(1.0f);
        
        // 한글 폰트 설정
        if (koreanFont == null)
        {
            koreanFont = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
        }

        // LiberationSans 폰트 찾기
        if (liberationSansFont == null)
        {
            liberationSansFont = Resources.Load<TMP_FontAsset>("LiberationSans SDF");
        }

        if (liberationSansFont != null && koreanFont != null)
        {
            // LiberationSans 폰트에 한글 폰트를 fallback으로 추가
            ModifyFontFallback(liberationSansFont, koreanFont);
            Debug.Log("LiberationSans 폰트에 한글 폰트 fallback 추가 완료");
        }
        else
        {
            Debug.LogWarning("필요한 폰트를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 폰트 fallback 수정
    /// </summary>
    private void ModifyFontFallback(TMP_FontAsset targetFont, TMP_FontAsset fallbackFont)
    {
        if (targetFont != null && fallbackFont != null)
        {
            // 기존 fallback 폰트 목록에 한글 폰트 추가
            if (!targetFont.fallbackFontAssetTable.Contains(fallbackFont))
            {
                targetFont.fallbackFontAssetTable.Add(fallbackFont);
                Debug.Log($"폰트 fallback 추가: {targetFont.name} → {fallbackFont.name}");
            }
        }
    }

    /// <summary>
    /// 수동으로 폰트 에셋 수정
    /// </summary>
    [ContextMenu("Modify Font Assets")]
    public void ManualModifyFontAssets()
    {
        StartCoroutine(ModifyFontAssets());
    }

    /// <summary>
    /// 모든 폰트의 fallback 설정 확인
    /// </summary>
    [ContextMenu("Check Font Fallbacks")]
    public void CheckFontFallbacks()
    {
        TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        
        Debug.Log("=== 폰트 Fallback 설정 확인 ===");
        foreach (var font in allFonts)
        {
            if (font != null)
            {
                Debug.Log($"폰트: {font.name} - Fallback 개수: {font.fallbackFontAssetTable.Count}");
                for (int i = 0; i < font.fallbackFontAssetTable.Count; i++)
                {
                    if (font.fallbackFontAssetTable[i] != null)
                    {
                        Debug.Log($"  Fallback {i}: {font.fallbackFontAssetTable[i].name}");
                    }
                }
            }
        }
    }
}
