using UnityEngine;
using TMPro;

public class AnswerDisplay : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject questionPanel; // 문제/선택지 영역
    [SerializeField] private GameObject resultPanel;   // 정답 결과 영역

    [Header("답 표시 UI")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI answer1Text;
    [SerializeField] private TextMeshProUGUI answer2Text;
    [SerializeField] private TextMeshProUGUI answer3Text;
    [SerializeField] private TextMeshProUGUI answer4Text;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("폰트 설정")]
    [SerializeField] private TMP_FontAsset koreanFont;
    [SerializeField] private int questionFontSize = 24;
    [SerializeField] private int answerFontSize = 20;
    [SerializeField] private int resultFontSize = 28;

    private void Start()
    {
        InitializeFonts();
        ShowQuestionUI();
    }

    private void InitializeFonts()
    {
        // 한글 폰트가 없으면 기본 폰트 사용
        if (koreanFont == null)
        {
            koreanFont = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
        }

        // 모든 텍스트에 폰트 적용
        ApplyFontToText(questionText, koreanFont, questionFontSize);
        ApplyFontToText(answer1Text, koreanFont, answerFontSize);
        ApplyFontToText(answer2Text, koreanFont, answerFontSize);
        ApplyFontToText(answer3Text, koreanFont, answerFontSize);
        ApplyFontToText(answer4Text, koreanFont, answerFontSize);
        ApplyFontToText(resultText, koreanFont, resultFontSize);
    }

    private void ApplyFontToText(TextMeshProUGUI textComponent, TMP_FontAsset font, int fontSize)
    {
        if (textComponent != null)
        {
            textComponent.font = font;
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
        }
    }

    public void DisplayQuestion(string question)
    {
        if (questionText != null)
        {
            questionText.text = question;
        }
    }

    public void DisplayAnswers(string[] answers)
    {
        if (answer1Text != null && answers.Length > 0) answer1Text.text = answers[0];
        if (answer2Text != null && answers.Length > 1) answer2Text.text = answers[1];
        if (answer3Text != null && answers.Length > 2) answer3Text.text = answers[2];
        if (answer4Text != null && answers.Length > 3) answer4Text.text = answers[3];
    }

    public void DisplayCorrectAnswer(string correctAnswer)
    {
        if (resultText != null)
        {
            resultText.text = $"정답: {correctAnswer}";
        }
    }

    public void DisplayResult(bool isCorrect, string correctAnswer = "")
    {
        ShowResultUI();
        if (resultText != null)
        {
            if (isCorrect)
            {
                resultText.text = "정답입니다!";
                resultText.color = Color.green;
            }
            else
            {
                resultText.text = $"틀렸습니다. 정답은 {correctAnswer}입니다.";
                resultText.color = Color.red;
            }
        }
    }

    public void ClearDisplay()
    {
        if (questionText != null) questionText.text = "";
        if (answer1Text != null) answer1Text.text = "";
        if (answer2Text != null) answer2Text.text = "";
        if (answer3Text != null) answer3Text.text = "";
        if (answer4Text != null) answer4Text.text = "";
        if (resultText != null) resultText.text = "";
        ShowQuestionUI();
    }

    // 폰트 설정 메서드들
    public void SetKoreanFont(TMP_FontAsset font)
    {
        koreanFont = font;
        InitializeFonts();
    }

    public void SetQuestionFontSize(int size)
    {
        questionFontSize = size;
        if (questionText != null)
        {
            questionText.fontSize = size;
        }
    }

    public void SetAnswerFontSize(int size)
    {
        answerFontSize = size;
        if (answer1Text != null) answer1Text.fontSize = size;
        if (answer2Text != null) answer2Text.fontSize = size;
        if (answer3Text != null) answer3Text.fontSize = size;
        if (answer4Text != null) answer4Text.fontSize = size;
    }

    // UI 토글: 문제 영역 보이기 / 결과 영역 보이기
    public void ShowQuestionUI()
    {
        if (questionPanel != null) questionPanel.SetActive(true);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    public void ShowResultUI()
    {
        if (questionPanel != null) questionPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(true);
    }
}
