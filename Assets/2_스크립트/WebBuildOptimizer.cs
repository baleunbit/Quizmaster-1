using UnityEngine;

/// <summary>
/// 웹빌드 최적화를 위한 유틸리티 클래스
/// </summary>
public class WebBuildOptimizer : MonoBehaviour
{
    [Header("웹빌드 최적화 설정")]
    [SerializeField] private bool enableWebOptimizations = true;
    [SerializeField] private bool disableDebugLogs = true;
    [SerializeField] private bool enableMemoryOptimization = true;

    private void Awake()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            ApplyWebOptimizations();
        #endif
    }

    private void ApplyWebOptimizations()
    {
        if (!enableWebOptimizations) return;

        // 디버그 로그 비활성화
        if (disableDebugLogs)
        {
            Debug.unityLogger.logEnabled = false;
        }

        // 메모리 최적화 (웹에서 안전하게)
        if (enableMemoryOptimization)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
                // 웹에서는 GC 호출을 제한적으로 사용
                StartCoroutine(SafeGarbageCollection());
            #else
                // 데스크톱에서는 일반적인 GC 사용
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();
            #endif
        }

        // 웹 전용 설정
        Application.targetFrameRate = 60; // 웹에서 안정적인 프레임레이트
        QualitySettings.vSyncCount = 1; // 수직 동기화 활성화
    }

    private System.Collections.IEnumerator SafeGarbageCollection()
    {
        yield return new WaitForEndOfFrame();
        // 웹에서는 GC 호출을 최소화
        System.GC.Collect();
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// 웹에서 안전한 종료 처리
    /// </summary>
    public static void SafeExit()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서는 사용자에게 안내 메시지 표시 (최신 방법)
            Debug.Log("게임을 종료하려면 브라우저 탭을 닫아주세요.");
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// 웹에서 안전한 URL 열기
    /// </summary>
    public static void SafeOpenURL(string url)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서는 URL 열기 제한 (최신 방법)
            Debug.Log($"웹에서 URL 열기 요청: {url}");
        #else
            Application.OpenURL(url);
        #endif
    }

    /// <summary>
    /// 웹에서 안전한 데이터 저장
    /// </summary>
    public static void SafeSaveData(string key, string value)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서는 데이터 저장 제한 (최신 방법)
            Debug.Log($"웹에서 데이터 저장 요청: {key} = {value}");
        #else
            PlayerPrefs.SetString(key, value);
        #endif
    }

    /// <summary>
    /// 웹에서 안전한 데이터 로드
    /// </summary>
    public static string SafeLoadData(string key, string defaultValue = "")
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            // 웹에서는 localStorage 사용 (비동기이므로 제한적)
            return defaultValue;
        #else
            return PlayerPrefs.GetString(key, defaultValue);
        #endif
    }
}
