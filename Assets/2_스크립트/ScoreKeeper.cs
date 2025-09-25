using UnityEngine;
using System;

public class ScoreKeeper : MonoBehaviour
{
    [Header("현재 게임 점수")]
    private int correctAnswers = 0;
    private int questionSeen = 0;
    private int totalScore = 0;
    private float timeBonus = 0f;
    private int wrongAnswers = 0;
    
    [Header("콤보 시스템")]
    private int currentCombo = 0;
    private int maxCombo = 0;
    private int comboStreak = 0;

    [Header("점수 설정")]
    [SerializeField] private int baseScorePerQuestion = 100;
    [SerializeField] private int timeBonusMultiplier = 10;
    [SerializeField] private int perfectBonus = 500;
    
    [Header("콤보 설정")]
    [SerializeField] private float[] comboMultipliers = { 1.0f, 1.2f, 1.5f, 2.0f, 2.5f, 3.0f };
    [SerializeField] private int[] comboThresholds = { 0, 2, 4, 6, 8, 10 };
    
    [Header("게임 종료 설정")]
    [SerializeField] private int maxWrongAnswers = 3;

    [Header("등급 시스템 (점수 기반)")]
    [SerializeField] private int[] gradeThresholds = { 1000, 5000, 100000, 150000, 300000, 10000000 };
    [SerializeField] private string[] gradeNames = { "F", "D", "C", "B", "A", "S" };

    // 이벤트
    public static event Action<int> OnScoreChanged;
    public static event Action<string> OnGradeChanged;
    public static event Action<int, float> OnComboChanged;

    private void Start()
    {
        LoadHighScore();
    }

    // 기본 점수 관리
    public int GetCorrectAnswers() => correctAnswers;
    public int GetQuestionSeen() => questionSeen;
    public int GetTotalScore() => totalScore;
    public int GetWrongAnswers() => wrongAnswers;
    
    // 콤보 관리
    public int GetCurrentCombo() => currentCombo;
    public int GetMaxCombo() => maxCombo;
    public float GetComboMultiplier() => GetComboMultiplierForStreak(currentCombo);

    public void IncrementCorrectAnswers()
    {
        Debug.Log("=== IncrementCorrectAnswers 호출됨 ===");
        correctAnswers++;
        currentCombo++;
        comboStreak++;
        
        Debug.Log($"정답 수 증가: {correctAnswers}, 콤보: {currentCombo}");
        
        // 최대 콤보 업데이트
        if (currentCombo > maxCombo)
        {
            maxCombo = currentCombo;
        }
        
        UpdateTotalScore();
        OnComboChanged?.Invoke(currentCombo, GetComboMultiplier());
        
        Debug.Log($"콤보! {currentCombo}연속 (x{GetComboMultiplier():F1})");
        Debug.Log("=== IncrementCorrectAnswers 완료 ===");
    }
    
    public void BreakCombo()
    {
        if (currentCombo > 0)
        {
            Debug.Log($"콤보 브레이크! {currentCombo}연속에서 중단");
            currentCombo = 0;
            comboStreak = 0;
            OnComboChanged?.Invoke(0, 1.0f);
        }
    }
    
    public void IncrementWrongAnswers()
    {
        wrongAnswers++;
        Debug.Log($"틀린 답: {wrongAnswers}/{maxWrongAnswers}");
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
        // 콤보 배수 적용
        float comboMultiplier = GetComboMultiplier();
        int comboScore = Mathf.RoundToInt((correctAnswers * baseScorePerQuestion) * comboMultiplier);
        
        totalScore = comboScore + Mathf.RoundToInt(timeBonus);
        
        Debug.Log($"점수 계산: 정답={correctAnswers}, 기본점수={baseScorePerQuestion}, 콤보배수={comboMultiplier:F1}, 콤보점수={comboScore}, 시간보너스={timeBonus:F1}, 총점수={totalScore}");
        
        OnScoreChanged?.Invoke(totalScore);
    }
    
    private float GetComboMultiplierForStreak(int streak)
    {
        for (int i = comboThresholds.Length - 1; i >= 0; i--)
        {
            if (streak >= comboThresholds[i])
            {
                return comboMultipliers[i];
            }
        }
        return 1.0f;
    }

    // 등급 시스템 (점수 기반)
    public string GetGrade()
    {
        int currentScore = totalScore;
        
        Debug.Log($"등급 계산: 현재 점수 {currentScore}점");
        
        // 점수에 따른 등급 결정 (사용자 요청 기준)
        if (currentScore >= 10000000)
        {
            Debug.Log($"등급 결정: S등급 ({currentScore}점 >= 10000000점)");
            return "S";
        }
        else if (currentScore >= 300000)
        {
            Debug.Log($"등급 결정: A등급 ({currentScore}점 >= 300000점, < 10000000점)");
            return "A";
        }
        else if (currentScore >= 150000)
        {
            Debug.Log($"등급 결정: B등급 ({currentScore}점 >= 150000점, < 300000점)");
            return "B";
        }
        else if (currentScore >= 100000)
        {
            Debug.Log($"등급 결정: C등급 ({currentScore}점 >= 100000점, < 150000점)");
            return "C";
        }
        else if (currentScore >= 5000)
        {
            Debug.Log($"등급 결정: D등급 ({currentScore}점 >= 5000점, < 100000점)");
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
        correctAnswers = 0;
        questionSeen = 0;
        totalScore = 0;
        timeBonus = 0f;
        wrongAnswers = 0;
        currentCombo = 0;
        comboStreak = 0;
        OnScoreChanged?.Invoke(0);
        OnComboChanged?.Invoke(0, 1.0f);
        Debug.Log("=== ScoreKeeper.ResetScore() 완료 - 모든 값이 0으로 초기화됨 ===");
    }

    // 완벽한 점수 보너스 (모든 문제를 맞췄을 때)
    public void AddPerfectBonus()
    {
        if (wrongAnswers == 0 && questionSeen > 0)
        {
            totalScore += perfectBonus;
            OnScoreChanged?.Invoke(totalScore);
            Debug.Log($"완벽한 점수 보너스: +{perfectBonus}점");
        }
    }

    // 점수 정보 문자열
    public string GetScoreInfo()
    {
        return $"정답: {correctAnswers}/{questionSeen} | 점수: {totalScore} | 등급: {GetGrade()} | 최대콤보: {maxCombo}";
    }
    
    // 콤보 정보 문자열
    public string GetComboInfo()
    {
        if (currentCombo > 0)
        {
            return $"콤보 {currentCombo}연속! (x{GetComboMultiplier():F1})";
        }
        return "";
    }
}
