using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] float problemTime = 10f;   // 문제 풀이 시간
    [SerializeField] float solutionTime = 3f;   // 정답, 틀렸습니다 시간
    [HideInInspector] public float time = 0;

    [Header("UI Text 연결")]
    [SerializeField] TextMeshProUGUI problemTimeText;    // 문제 풀이 시간 표시용
    [SerializeField] TextMeshProUGUI solutionTimeText;   // 정답 표시 시간용
    [SerializeField] TextMeshProUGUI countdownText;      // 카운트다운 표시용

    [HideInInspector] public bool isProblemTime = true;
    [HideInInspector] public float fillAmount;
    [HideInInspector] public bool loadNextQuestion;
    
    [Header("타이머 제어")]
    [SerializeField] private bool isTimerActive = false;  // 타이머 활성화 상태
    private bool isPaused = false;  // 일시정지 상태
    private float pausedTime = 0f;  // 일시정지 시점의 시간

    private void Start()
    {
        time = problemTime;
        loadNextQuestion = false;
        isTimerActive = false;  // 시작 시 타이머 비활성화
    }

    private void Update()
    {
        if (isTimerActive && !isPaused)
        {
            TimerCountDown();
            UpdateFillAmount();
        }
        UpdateUIText();
    }

    private void UpdateFillAmount()
    {
        if (isProblemTime)
            fillAmount = time / problemTime;
        else
            fillAmount = time / solutionTime;
    }

    private void TimerCountDown()
    {
        time -= Time.deltaTime;
        if (time <= 0)
        {
            if (isProblemTime)
            {
                isProblemTime = false;
                time = solutionTime;
            }
            else
            {
                isProblemTime = true;
                time = problemTime;
                loadNextQuestion = true;
            }
        }
    }

    public void CanelTimer()
    {
        time = 0;
    }

    private void UpdateUIText()
    {
        if (!isTimerActive)
        {
            // 타이머가 비활성화일 때 UI 숨기기
            if (problemTimeText != null)
            {
                problemTimeText.text = "";
            }
            if (solutionTimeText != null)
            {
                solutionTimeText.text = "";
            }
            if (countdownText != null)
            {
                countdownText.text = "";
            }
            return;
        }

        int displayTime = Mathf.CeilToInt(time);
        
        if (isProblemTime)
        {
            // 문제 풀이 시간일 때
            if (problemTimeText != null)
            {
                WebBuildSettings.SetWebText(problemTimeText, $"문제 시간: {displayTime}");
            }
            if (solutionTimeText != null)
            {
                WebBuildSettings.SetWebText(solutionTimeText, "");
            }
            if (countdownText != null)
            {
                WebBuildSettings.SetWebText(countdownText, displayTime.ToString());
                // 시간이 3초 이하일 때 빨간색으로 강조
                if (displayTime <= 3)
                {
                    countdownText.color = Color.red;
                }
                else
                {
                    countdownText.color = Color.white;
                }
            }
        }
        else
        {
            // 정답 표시 시간일 때
            if (problemTimeText != null)
            {
                WebBuildSettings.SetWebText(problemTimeText, "");
            }
            if (solutionTimeText != null)
            {
                WebBuildSettings.SetWebText(solutionTimeText, $"정답 표시: {displayTime}");
            }
            if (countdownText != null)
            {
                WebBuildSettings.SetWebText(countdownText, displayTime.ToString());
                countdownText.color = Color.yellow;
            }
        }
    }

    // 타이머 활성화/비활성화 메서드들
    public void StartTimer()
    {
        isTimerActive = true;
        time = problemTime;
        isProblemTime = true;
        loadNextQuestion = false; // 새 문제 시작 시 즉시 넘기지 않도록
    }

    public void StopTimer()
    {
        isTimerActive = false;
    }

    public void PauseTimer()
    {
        if (isTimerActive && !isPaused)
        {
            isPaused = true;
            pausedTime = time;
            Debug.Log($"타이머 일시정지: {time:F1}초 남음");
        }
    }

    public void ResumeTimer()
    {
        if (isTimerActive && isPaused)
        {
            isPaused = false;
            time = pausedTime;
            Debug.Log($"타이머 재개: {time:F1}초 남음");
        }
    }

    public void ResetTimer()
    {
        time = problemTime;
        isProblemTime = true;
        loadNextQuestion = false;
    }
}