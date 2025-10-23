using UnityEngine;
using System;

public class ScoreKeeper : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static ScoreKeeper instance;
    
    [Header("현재 게임 점수")]
    private int correctAnswers = 0;
    private int questionSeen = 0;
    private int totalScore = 0;
    private float timeBonus = 0f;
    private int wrongAnswers = 0;
    
    [Header("점수 설정")]
    [SerializeField] private int baseScorePerQuestion = 1000;
    [SerializeField] private int timeBonusMultiplier = 10;
    
    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
            Debug.Log("ScoreKeeper 싱글톤 인스턴스 생성됨");
        }
        else if (instance != this)
        {
            Debug.LogWarning("ScoreKeeper 중복 인스턴스 발견! 기존 인스턴스 유지");
            Destroy(gameObject);
            return;
        }
        
        // Inspector에서 값이 변경되었을 경우를 대비해 강제로 1000으로 설정
        baseScorePerQuestion = 1000;
        Debug.Log($"baseScorePerQuestion 강제 설정: {baseScorePerQuestion}");
    }
    
    [Header("게임 종료 설정")]
    [SerializeField] private int maxWrongAnswers = 3;

    [Header("등급 시스템 (점수 기반)")]
    [SerializeField] private int[] gradeThresholds = { 1000, 5000, 100000, 150000, 300000, 10000000 };
    [SerializeField] private string[] gradeNames = { "F", "D", "C", "B", "A", "S" };

    // 이벤트
    public static event Action<int> OnScoreChanged;

    private void Start()
    {
        LoadHighScore();
    }

    // 기본 점수 관리
    public int GetCorrectAnswers() => correctAnswers;
    public int GetQuestionSeen() => questionSeen;
    public int GetTotalScore() => totalScore;
    public int GetWrongAnswers() => wrongAnswers;
    

    public void IncrementCorrectAnswers()
    {
        Debug.Log("=== IncrementCorrectAnswers 호출됨 ===");
        correctAnswers++;
        
        Debug.Log($"정답 수 증가: {correctAnswers} (총 정답 수)");
        
        UpdateTotalScore();
        
        Debug.Log("=== IncrementCorrectAnswers 완료 ===");
    }
    
    
    public void IncrementWrongAnswers()
    {
        wrongAnswers++;
        Debug.Log($"=== IncrementWrongAnswers 호출됨 ===");
        Debug.Log($"틀린 답: {wrongAnswers}/{maxWrongAnswers}");
        Debug.Log($"호출 스택: {System.Environment.StackTrace}");
        Debug.Log("=== IncrementWrongAnswers 완료 ===");
    }
    
    public bool IsGameOver()
    {
        return wrongAnswers >= maxWrongAnswers;
    }
    
    public int GetRemainingLives()
    {
        return maxWrongAnswers - wrongAnswers;
    }

    public void IncrementQuestionSeen()
    {
        questionSeen++;
    }

    // 점수 계산
    public int CalculateScore()
    {
        if (questionSeen == 0) return 0;
        return Mathf.RoundToInt((float)correctAnswers / questionSeen * 100);
    }

    public void AddTimeBonus(float remainingTime)
    {
        timeBonus += remainingTime * timeBonusMultiplier;
        UpdateTotalScore();
    }

    private void UpdateTotalScore()
    {
        // 기본 점수 계산 (문제당 1000점)
        int baseScore = correctAnswers * baseScorePerQuestion;
        
        Debug.Log($"UpdateTotalScore 호출 전: totalScore={totalScore}");
        totalScore = baseScore + Mathf.RoundToInt(timeBonus);
        Debug.Log($"UpdateTotalScore 호출 후: totalScore={totalScore}");
        
        Debug.Log($"점수 계산: 정답={correctAnswers}, 기본점수={baseScore}, 시간보너스={timeBonus:F1}, 총점수={totalScore}");
        
        OnScoreChanged?.Invoke(totalScore);
    }
    

    // 등급 시스템 (점수 기반)
    public string GetGrade()
    {
        int currentScore = totalScore;
        
        Debug.Log($"등급 계산: 현재 점수 {currentScore}점");
        
        // 점수에 따른 등급 결정 (새로운 기준)
        if (currentScore >= 30000)
        {
            Debug.Log($"등급 결정: S등급 ({currentScore}점 >= 30000점)");
            return "S";
        }
        else if (currentScore >= 20000)
        {
            Debug.Log($"등급 결정: A등급 ({currentScore}점 >= 20000점, < 30000점)");
            return "A";
        }
        else if (currentScore >= 15000)
        {
            Debug.Log($"등급 결정: B등급 ({currentScore}점 >= 15000점, < 20000점)");
            return "B";
        }
        else if (currentScore >= 10000)
        {
            Debug.Log($"등급 결정: C등급 ({currentScore}점 >= 10000점, < 15000점)");
            return "C";
        }
        else if (currentScore >= 5000)
        {
            Debug.Log($"등급 결정: D등급 ({currentScore}점 >= 5000점, < 10000점)");
            return "D";
        }
        else if (currentScore >= 1000)
        {
            Debug.Log($"등급 결정: F등급 ({currentScore}점 >= 1000점, < 5000점)");
            return "F";
        }
        else
        {
            Debug.Log($"등급 결정: F등급 ({currentScore}점 < 1000점)");
            return "F";
        }
    }

    public Color GetGradeColor()
    {
        string grade = GetGrade();
        switch (grade)
        {
            case "S": return Color.yellow;
            case "A": return Color.green;
            case "B": return Color.blue;
            case "C": return Color.cyan;
            case "D": return Color.red;
            case "F": return Color.gray;
            default: return Color.gray;
        }
    }

    // 최고 점수 관리
    public void SaveHighScore()
    {
        int currentScore = totalScore;
        int highScore = GetHighScore();
        
        Debug.Log($"SaveHighScore: 현재점수={currentScore}, 최고점수={highScore}");
        Debug.Log($"ScoreKeeper 상태 확인: correctAnswers={correctAnswers}, questionSeen={questionSeen}, totalScore={totalScore}");
        
        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.SetString("HighScoreDate", DateTime.Now.ToString("yyyy-MM-dd"));
            PlayerPrefs.SetString("HighScoreGrade", GetGrade());
            PlayerPrefs.Save();
            Debug.Log($"새로운 최고 점수 저장: {currentScore}점 ({GetGrade()}등급)");
        }
        else
        {
            Debug.Log($"최고 점수 갱신 없음: 현재점수 {currentScore} <= 최고점수 {highScore}");
        }
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }

    public string GetHighScoreDate()
    {
        return PlayerPrefs.GetString("HighScoreDate", "");
    }

    public string GetHighScoreGrade()
    {
        return PlayerPrefs.GetString("HighScoreGrade", "");
    }

    private void LoadHighScore()
    {
        // 최고 점수 로드 (필요시)
    }

    // 게임 리셋
    public void ResetScore()
    {
        Debug.Log("=== ScoreKeeper.ResetScore() 시작 ===");
        Debug.Log($"초기화 전 상태: correctAnswers={correctAnswers}, totalScore={totalScore}");
        Debug.Log($"호출 스택: {System.Environment.StackTrace}");
        
        // 게임 오버 후에는 점수를 유지하므로 초기화하지 않음
        if (correctAnswers > 0 || questionSeen > 0)
        {
            Debug.Log("게임 오버 후 점수 유지 - ResetScore 무시됨");
            return;
        }
        
        correctAnswers = 0;
        questionSeen = 0;
        totalScore = 0;
        timeBonus = 0f;
        wrongAnswers = 0;
        OnScoreChanged?.Invoke(0);
        Debug.Log("=== ScoreKeeper.ResetScore() 완료 - 모든 값이 0으로 초기화됨 ===");
    }


    // 점수 정보 문자열
    public string GetScoreInfo()
    {
        return $"정답: {correctAnswers}/{questionSeen} | 점수: {totalScore} | 등급: {GetGrade()}";
    }

    /// <summary>
    /// 힌트 사용 시 점수를 차감하는 메서드
    /// </summary>
    /// <param name="amount">차감할 점수</param>
    public void DeductScore(int amount)
    {
        totalScore = Mathf.Max(0, totalScore - amount); // 점수는 0 이하로 내려가지 않음
        OnScoreChanged?.Invoke(totalScore);
        Debug.Log($"힌트 사용으로 {amount}점 차감. 현재 점수: {totalScore}");
    }
}
