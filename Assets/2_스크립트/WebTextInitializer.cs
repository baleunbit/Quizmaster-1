using UnityEngine;
using TMPro;

/// <summary>
/// 웹빌드에서 텍스트 초기화를 담당하는 클래스
/// </summary>
public class WebTextInitializer : MonoBehaviour
{
    [Header("웹 텍스트 초기화 설정")]
    [SerializeField] private float initializationDelay = 0.5f;
    [SerializeField] private bool forceTextRefresh = true;

    private void Start()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(InitializeWebTexts());
        #endif
    }

    private System.Collections.IEnumerator InitializeWebTexts()
    {
        yield return new WaitForSeconds(initializationDelay);
        
        // 모든 TextMeshPro 컴포넌트 초기화
        TextMeshProUGUI[] allTexts = WebBuildBugFixer.SafeFindObjectsOfType<TextMeshProUGUI>();
        
        foreach (var text in allTexts)
        {
            InitializeSingleText(text);
        }
        
        if (forceTextRefresh)
        {
            yield return new WaitForSeconds(0.2f);
            RefreshAllTexts();
        }
    }

    private void InitializeSingleText(TextMeshProUGUI text)
    {
        if (text == null) return;

        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서 안정적인 폰트 설정
            if (text.font == null)
            {
                // 한글 폰트 사용 (Arial SDF 폴백 제거)
                text.font = Resources.Load<TMP_FontAsset>("NanumPenScript-Regular SDF");
                if (text.font == null)
                {
                    Debug.LogError("NanumPenScript-Regular SDF 폰트를 찾을 수 없습니다! 폰트를 교체하지 않습니다.");
                }
            }

            // 웹에서 텍스트 렌더링 최적화
            text.richText = false;
            text.raycastTarget = false;
            
            // 텍스트 강제 업데이트
            text.SetAllDirty();
            text.ForceMeshUpdate();
            
            // 웹에서 텍스트가 보이도록 강제 렌더링
            StartCoroutine(ForceTextVisibility(text));
        #endif
    }

    private System.Collections.IEnumerator ForceTextVisibility(TextMeshProUGUI text)
    {
        if (text == null) yield break;

        // 텍스트 컴포넌트 재활성화
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
    /// 수동으로 텍스트 초기화 실행
    /// </summary>
    [ContextMenu("Initialize Web Texts")]
    public void ManualInitializeTexts()
    {
        StartCoroutine(InitializeWebTexts());
    }
}
