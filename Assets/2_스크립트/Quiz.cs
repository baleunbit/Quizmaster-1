using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quiz : MonoBehaviour
{
    [Header("문제")]
    [SerializeField] TextMeshProUGUI questionText;
    QuestionSO currentQuestion;//현재문제를 담는 변수
    [SerializeField] List<QuestionSO> questions = new List<QuestionSO>();

    [Header("답")]
    [SerializeField] GameObject[] answerButtons;

    [Header("버튼스프라이트")]
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

        // 과목 선택 메뉴에서 선택된 과목 사용
        string topiToUse = GetSelectedSubject();
        chatGPTClint.GenerateQuestions(qestionCount, topiToUse);
        Debug.Log("GenerateQuestionsIFNeeded:" + topiToUse);
    }

    private string GetSelectedSubject()
    {
        // SubjectSelectionMenu에서 선택된 과목 가져오기
        SubjectSelectionMenu subjectMenu = FindFirstObjectByType<SubjectSelectionMenu>();
        if (subjectMenu != null)
        {
            return subjectMenu.GetSelectedSubject();
        }
        
        // 기본값으로 랜덤 과목 사용
        return GetTrendingTopic();
    }

    private string GetTrendingTopic()
    {
        string[] trendingtopics ={
            "일반상식","과학", "확률과 통계", "수학과 과학","한국사",
            "사회정치","경제정치","역사정치"
        };
        int ramdomindex = UnityEngine.Random.Range(0, trendingtopics.Length);
        return trendingtopics[ramdomindex];
    }

    private void QuizGeneratedHandler(List<QuestionSO> generatedQuestions)
    {
        //생성된 문제를 받아옴
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

        // 다음 문제 로드 처리
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

        // SolutionTime이고 답을 선택하지 않았을 때 자동 처리
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
        
        // 문제가 표시된 후 타이머 시작
        if (timer != null)
        {
            timer.StartTimer();
        }
    }
    private void GetRandomQuestion()
    {
        int Randomindex = UnityEngine.Random.Range(0, questions.Count);
        currentQuestion = questions[Randomindex];
        questions.RemoveAt(Randomindex); // 중복 방지
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
        // 문제 표시 (색상 초기화)
        questionText.text = currentQuestion.GetQuestion();
        questionText.color = Color.white;

        // 답 버튼들에 텍스트 설정
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = currentQuestion.GetAnswer(i);
                    // 폰트 설정 (한글 지원)
                    if (buttonText.font == null)
                    {
                        buttonText.font = Resources.GetBuiltinResource<TMP_FontAsset>("Arial SDF");
                    }
                }
            }
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
            // 정답 처리
            questionText.text = "정답입니다!";
            questionText.color = Color.green;
            
            if (index >= 0 && answerButtons[index] != null)
            {
                answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;
            }
            scoreKeeper.IncrementCorrectAnswers();
        }
        else
        {
            // 오답 처리
            questionText.text = "틀렸습니다.\n정답은 " + currentQuestion.GetCorrectAnswer() + "입니다.";
            questionText.color = Color.red;
            
            // 정답 버튼도 정답 스프라이트로 표시
            int correctIndex = currentQuestion.GetCorrectAnswerIndex();
            if (correctIndex >= 0 && correctIndex < answerButtons.Length && answerButtons[correctIndex] != null)
            {
                answerButtons[correctIndex].GetComponent<Image>().sprite = correctAnswerSprite;
            }
        }
        SetButtonState(false);
    }

    private void SetButtonState(bool state)
    {
        foreach (GameObject obj in answerButtons)
        {
            obj.GetComponent<Button>().interactable = state;
        }
    }
}