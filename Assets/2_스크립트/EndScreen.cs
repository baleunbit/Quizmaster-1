using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI finalScoreText;
    [SerializeField] TextMeshProUGUI highScoreText;
    [SerializeField] ScoreKeeper scoreKeeper;
    [SerializeField] Button subjectSelectionButton;

    private void Start()
    {
        if (subjectSelectionButton != null)
        {
            subjectSelectionButton.onClick.AddListener(OnSubjectSelectionClicked);
        }
    }

    public void ShowFinalScore()
    {
        Debug.Log("=== EndScreen.ShowFinalScore() 시작 ===");
        
        // ScoreKeeper가 제대로 연결되어 있는지 확인
        if (scoreKeeper == null)
        {
            Debug.LogError("ScoreKeeper가 연결되지 않았습니다!");
            return;
        }
        
        // 완벽한 점수 보너스 추가 (저장 전에)
        Debug.Log("완벽한 점수 보너스 추가 전 점수: " + scoreKeeper.GetTotalScore());
        scoreKeeper.AddPerfectBonus();
        Debug.Log("완벽한 점수 보너스 추가 후 점수: " + scoreKeeper.GetTotalScore());
        
        // 최고 점수 저장
        scoreKeeper.SaveHighScore();
        
        // 최종 점수 표시
        string grade = scoreKeeper.GetGrade();
        Color gradeColor = scoreKeeper.GetGradeColor();
        
        // 게임 오버 여부 확인
        bool isGameOver = scoreKeeper.GetWrongAnswers() >= 3;
        string gameResult = isGameOver ? "게임 오버!" : "완료되었습니다!";
        
        // 최종 점수 정보
        int finalScore = scoreKeeper.GetTotalScore();
        int correctAnswers = scoreKeeper.GetCorrectAnswers();
        int questionSeen = scoreKeeper.GetQuestionSeen();
        int wrongAnswers = scoreKeeper.GetWrongAnswers();
        int maxCombo = scoreKeeper.GetMaxCombo();
        
        Debug.Log($"=== EndScreen 점수 정보 ===");
        Debug.Log($"finalScore: {finalScore}");
        Debug.Log($"correctAnswers: {correctAnswers}");
        Debug.Log($"questionSeen: {questionSeen}");
        Debug.Log($"wrongAnswers: {wrongAnswers}");
        Debug.Log($"maxCombo: {maxCombo}");
        Debug.Log($"grade: {grade}");
        
        Debug.Log($"점수 정보 - 정답: {correctAnswers}, 문제수: {questionSeen}, 틀린답: {wrongAnswers}, 점수: {finalScore}, 등급: {grade}, 최대콤보: {maxCombo}");
        Debug.Log($"ScoreKeeper 상태 - correctAnswers: {scoreKeeper.GetCorrectAnswers()}, questionSeen: {scoreKeeper.GetQuestionSeen()}, totalScore: {scoreKeeper.GetTotalScore()}");
        
        // finalScoreText가 연결되어 있는지 확인
        if (finalScoreText == null)
        {
            Debug.LogError("finalScoreText가 연결되지 않았습니다!");
            return;
        }
        
        finalScoreText.text = $"{gameResult}\n" +
                             $"정답: {correctAnswers}/{questionSeen}\n" +
                             $"틀린 답: {wrongAnswers}\n" +
                             $"점수: {finalScore}점\n" +
                             $"등급: {grade}\n" +
                             $"최대 콤보: {maxCombo}연속";
        finalScoreText.color = gradeColor;
        
        // 최고 점수 표시
        if (highScoreText != null)
        {
            int highScore = scoreKeeper.GetHighScore();
            string highScoreDate = scoreKeeper.GetHighScoreDate();
            string highScoreGrade = scoreKeeper.GetHighScoreGrade();
            
            highScoreText.text = $"최고 점수: {highScore}점 ({highScoreGrade}등급)\n" +
                                $"기록일: {highScoreDate}";
        }
        
        Debug.Log($"=== EndScreen.ShowFinalScore() 완료 - 최종 점수: {finalScore}점, 등급: {grade} ===");
    }

    private void OnSubjectSelectionClicked()
    {
        if (GameManger.instance != null)
        {
            GameManger.instance.OnBackToSubjectSelection();
        }
    }
}