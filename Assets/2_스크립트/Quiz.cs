using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quiz : MonoBehaviour
{
    [Header("질문")]
    [SerializeField] TextMeshProUGUI questionText;
    QuestionSO currentQuestion;//인스펙터에서 못보게 변경
    [SerializeField] List<QuestionSO> questions = new List<QuestionSO>();

    [Header("보기")]
    [SerializeField] GameObject[] answerButtons;

    [Header("버튼색깔")]
    [SerializeField] Sprite defaultAnswerSprite;
    [SerializeField] Sprite correctAnswerSprite;

    [Header("타이머")]
    [SerializeField] Image timerImage;
    [SerializeField] Sprite problemTimerSprite;
    [SerializeField] Sprite solutionTimerSprite;
    Timer timer;
    bool chooseAnswer = false;

    [Header("점수")]
    [SerializeField] TextMeshProUGUI scoreText;
    ScoreKeeper scoreKeeper;

    [Header("progressBar")]
    [SerializeField] Slider progressBar;

    [Header("Questiongeneration")]
    [SerializeField] ChatGPTClient chatGPTClint;
    [SerializeField] int qestionCount = 3;
    [SerializeField] TextMeshProUGUI londingText;

    bool isGenerateQuestions = false;

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        chatGPTClint.quizGenerateHandler += QuizGeneratedHandler; 

        if (questions.Count <= 0)
        {
            GenerateQuestionsIFNeeded();
        }
        else
        {
            InitalizeProgressBar();
        }
    }
    private void GenerateQuestionsIFNeeded()
    {
        if (isGenerateQuestions) return;

        isGenerateQuestions = true;
        GameManger.instance.ShowlodingSceen();

        string topiToUse = GetTrendingTopic();
        chatGPTClint.GenerateQuestions(qestionCount, topiToUse);
        Debug.Log("GenerateQuestionsIFNeeded:" + topiToUse);
    }

    private string GetTrendingTopic()
    {
        string[] trendingtopics ={
            "영어","미적분", "확율과 통계", "윤리와 사상","한국사",
            "사회탐구","과학탐구","직업탐구"
        };
        int ramdomindex = UnityEngine.Random.Range(0, trendingtopics.Length);
        return trendingtopics[ramdomindex];
    }

    private void QuizGeneratedHandler(List<QuestionSO> generatedQuestions)
    {
        //문제를 받아온뒤
        isGenerateQuestions = false;

        if (generatedQuestions == null || generatedQuestions.Count == 0)
        {
            Debug.LogError("문제 생성 실패");
            londingText.text = "문제 생성 실패. 다시 시도해주세요.";
            return;
        }

        questions.AddRange(generatedQuestions);
        progressBar.maxValue += generatedQuestions.Count;

        GetNextQuestion();

    }
    private void InitalizeProgressBar()
    {
        progressBar.maxValue = questions.Count;
        progressBar.value = 0;
    }

    private void Update()
    {
        // 타이머 이미지 업데이트
        if (timer.isProblemTime)
            timerImage.sprite = problemTimerSprite;
        else
            timerImage.sprite = solutionTimerSprite;
        timerImage.fillAmount = timer.fillAmount;

        // 다음 문제 불러오기
        if (timer.loadNextQuestion)
        {
            if (questions.Count == 0)
            {
                GenerateQuestionsIFNeeded();
                // GameManger.instance.ShowEndScreen();
            }
            else
            {
                //timer.loadNextQuestion = false;
                GetNextQuestion();
            }
        }

        // SolutionTime이고 답을 선택하지 않았을 때 오답 처리
        if (timer.isProblemTime == false && chooseAnswer == false)
        {
            DisplaySolution(-1);
        }
    }

    private void GetNextQuestion()
    {
        if (questions.Count <= 0)
        {
            Debug.Log("문제 없음");
            return;
        }
        
        timer.loadNextQuestion = false;

        GameManger.instance.ShowQuizScreen();
        chooseAnswer = false;
        SetButtonState(true);
        SetDefaultButtonSprites();
        GetRandomQuestion();
        OnDisplayQuestion();
        scoreKeeper.IncrementQuestionSeen();
        progressBar.value++;
    }
    private void GetRandomQuestion()
    {
        int Randomindex = UnityEngine.Random.Range(0, questions.Count);
        currentQuestion = questions[Randomindex];
        questions.RemoveAt(Randomindex); // 중복 출제 방지
    }

    private void SetDefaultButtonSprites()
    {
        foreach (GameObject buttonObj in answerButtons)
        {
            buttonObj.GetComponent<Image>().sprite = defaultAnswerSprite;
        }
    }

    private void OnDisplayQuestion()
    {
        questionText.text = currentQuestion.GetQuestion();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.GetAnswer(i);
        }
    }

    public void OnAnswerButtonClicked(int index)
    {
        chooseAnswer = true;
        DisplaySolution(index);
        timer.CanelTimer();
        scoreText.text = $"Score: {scoreKeeper.CalculateScore()}%";
    }

    private void DisplaySolution(int index)
    {
        if (index == currentQuestion.GetCorrectAnswerIndex())
        {
            questionText.text = "정답입니다!";
            if (index >= 0)
                answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;
            scoreKeeper.IncrementCorrectAnswers();
        }
        else
        {
            questionText.text = "틀렸습니다. 정답은 " + currentQuestion.GetCorrectAnswer();
        }
        SetButtonState(false);
        // timer.CancelTimer = (); // 잘못된 코드이므로 제거
    }

    private void SetButtonState(bool state)
    {
        foreach (GameObject obj in answerButtons)
        {
            obj.GetComponent<Button>().interactable = state;
        }
    }
}
