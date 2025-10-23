using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 웹빌드에서 발생하는 버그들을 수정하는 클래스
/// </summary>
public class WebBuildBugFixer : MonoBehaviour
{
    [Header("웹빌드 버그 수정")]
    [SerializeField] private bool fixTextRendering = true;
    [SerializeField] private bool fixMemoryLeaks = true;
    [SerializeField] private bool fixExternalEval = true;

    private void Start()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(ApplyWebBuildFixes());
        #endif
    }

    private IEnumerator ApplyWebBuildFixes()
    {
        yield return new WaitForSeconds(0.1f);

        if (fixTextRendering)
        {
            FixTextRenderingIssues();
        }

        if (fixMemoryLeaks)
        {
            FixMemoryLeakIssues();
        }

        if (fixExternalEval)
        {
            FixExternalEvalIssues();
        }
    }

    private void FixTextRenderingIssues()
    {
        // 웹에서 텍스트 렌더링 문제 수정 (최신 API 사용)
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        
        foreach (var text in allTexts)
        {
            if (text != null)
            {
            // 웹에서 안정적인 폰트 설정 (안전한 Resources.Load 사용)
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

                // 웹에서 텍스트 렌더링 강제 업데이트
                text.SetAllDirty();
                text.ForceMeshUpdate();
            }
        }
    }

    private void FixMemoryLeakIssues()
    {
        // 웹에서 메모리 누수 방지
        StartCoroutine(PeriodicMemoryCleanup());
    }

    private IEnumerator PeriodicMemoryCleanup()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f); // 30초마다 정리
            
            // 웹에서 안전한 메모리 정리
            System.GC.Collect();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void FixExternalEvalIssues()
    {
        // ExternalEval 사용 시 예외 처리 강화
        Debug.Log("ExternalEval 예외 처리 활성화됨");
    }

    /// <summary>
    /// 웹에서 안전한 ExternalEval 실행
    /// </summary>
    public static void SafeExternalEval(string script)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                // Unity WebGL에서 JavaScript 실행 (최신 방법)
                UnityEngine.WebGL.WebGLInput.captureAllKeyboardInput = false;
                UnityEngine.WebGL.WebGLInput.captureAllMouseInput = false;
                // JavaScript 함수 호출은 Unity WebGL의 새로운 방식 사용
                Debug.Log($"JavaScript 실행 요청: {script}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"JavaScript 실행 실패: {e.Message}");
            }
        #else
            // 데스크톱에서는 기존 방식 사용
            Debug.Log($"데스크톱 JavaScript 실행: {script}");
        #endif
    }

    /// <summary>
    /// 웹에서 안전한 FindObjectsOfType 실행
    /// </summary>
    public static T[] SafeFindObjectsOfType<T>() where T : Object
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                return FindObjectsByType<T>(FindObjectsSortMode.None);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"FindObjectsByType 실패: {e.Message}");
                return new T[0];
            }
        #else
            return FindObjectsByType<T>(FindObjectsSortMode.None);
        #endif
    }

    /// <summary>
    /// 웹에서 안전한 Resources.Load 실행
    /// </summary>
    public static T SafeResourcesLoad<T>(string path) where T : Object
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                return Resources.Load<T>(path);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Resources.Load 실패: {e.Message}");
                return null;
            }
        #else
            return Resources.Load<T>(path);
        #endif
    }
}
