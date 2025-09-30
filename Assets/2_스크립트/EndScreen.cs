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
        
        // ScoreKeeper 싱글톤 인스턴스 사용
        scoreKeeper = ScoreKeeper.instance;
        
        // ScoreKeeper가 제대로 연결되어 있는지 확인
        if (scoreKeeper == null)
        {
            Debug.LogError("ScoreKeeper 싱글톤 인스턴스가 없습니다!");
            return;
        }
        
        // ScoreKeeper 상태 확인
        Debug.Log($"EndScreen에서 ScoreKeeper 상태: totalScore={scoreKeeper.GetTotalScore()}, correctAnswers={scoreKeeper.GetCorrectAnswers()}");
        
        // 싱글톤 패턴으로 ScoreKeeper 인스턴스 검색 로직 제거
        
        
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
        
        Debug.Log($"=== EndScreen 점수 정보 ===");
        Debug.Log($"finalScore: {finalScore}");
        Debug.Log($"correctAnswers: {correctAnswers}");
        Debug.Log($"questionSeen: {questionSeen}");
        Debug.Log($"wrongAnswers: {wrongAnswers}");
        Debug.Log($"grade: {grade}");
        
        Debug.Log($"점수 정보 - 정답: {correctAnswers}, 문제수: {questionSeen}, 틀린답: {wrongAnswers}, 점수: {finalScore}, 등급: {grade}");
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
                             $"등급: {grade}";
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
        // 버튼 클릭 사운드 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayButtonClickSound();
        }
        
        if (GameManger.instance != null)
        {
            GameManger.instance.OnBackToSubjectSelection();
        }
    }
}