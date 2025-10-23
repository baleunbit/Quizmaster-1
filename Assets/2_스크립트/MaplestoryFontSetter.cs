using UnityEngine;
using TMPro;

/// <summary>
/// Maplestory Light SDF 폰트를 강제로 설정하는 스크립트
/// </summary>
public class MaplestoryFontSetter : MonoBehaviour
{
    [Header("Maplestory 폰트 설정")]
    [SerializeField] private TMP_FontAsset maplestoryFont;
    [SerializeField] private bool setOnStart = true;

    private void Start()
    {
        if (setOnStart)
        {
            SetMaplestoryFonts();
        }
    }

    /// <summary>
    /// Maplestory 폰트 설정
    /// </summary>
    private void SetMaplestoryFonts()
    {
        Debug.Log("=== Maplestory 폰트 설정 시작 ===");
        
        // Maplestory 폰트 설정
        if (maplestoryFont == null)
        {
            maplestoryFont = Resources.Load<TMP_FontAsset>("Maplestory Light SDF");
            if (maplestoryFont == null)
            {
                Debug.LogError("Maplestory Light SDF 폰트를 찾을 수 없습니다!");
                return;
            }
        }

        // 모든 TextMeshPro 컴포넌트 찾기
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        int setCount = 0;
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                // LiberationSans 폰트를 Maplestory로 교체
                if (text.font != null && text.font.name.Contains("LiberationSans"))
                {
                    Debug.Log($"폰트 교체: {text.name} - {text.font.name} → {maplestoryFont.name}");
                    text.font = maplestoryFont;
                    setCount++;
                }
                // 폰트가 null인 경우도 Maplestory로 설정
                else if (text.font == null)
                {
                    Debug.Log($"폰트 설정: {text.name} - null → {maplestoryFont.name}");
                    text.font = maplestoryFont;
                    setCount++;
                }
            }
        }
        
        Debug.Log($"=== Maplestory 폰트 설정 완료: {setCount}개 설정됨 ===");
    }

    /// <summary>
    /// 수동으로 Maplestory 폰트 설정
    /// </summary>
    [ContextMenu("Set Maplestory Fonts")]
    public void ManualSetMaplestoryFonts()
    {
        SetMaplestoryFonts();
    }

    /// <summary>
    /// 모든 텍스트를 Maplestory 폰트로 강제 설정
    /// </summary>
    [ContextMenu("Force All Text to Maplestory")]
    public void ForceAllTextToMaplestory()
    {
        if (maplestoryFont == null)
        {
            maplestoryFont = Resources.Load<TMP_FontAsset>("Maplestory Light SDF");
        }

        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                Debug.Log($"강제 폰트 설정: {text.name} - {text.font?.name} → {maplestoryFont.name}");
                text.font = maplestoryFont;
            }
        }
        
        Debug.Log("모든 텍스트를 Maplestory 폰트로 강제 설정 완료");
    }
}
