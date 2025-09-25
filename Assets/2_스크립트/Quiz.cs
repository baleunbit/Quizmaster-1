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
    [SerializeField] TextMeshProUGUI comboText;
    [SerializeField] TextMeshProUGUI livesText;
    ScoreKeeper scoreKeeper;

    [Header("progressBar")]
    [SerializeField] Slider progressBar;
    [SerializeField] TextMeshProUGUI progressText;

    [Header("Questiongeneration")]
    [SerializeField] ChatGPTClient chatGPTClint;
    [SerializeField] int qestionCount = 5;
    [SerializeField] TextMeshProUGUI londingText;


    bool isGenerateQuestions = false;

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        chatGPTClint.quizGenerateHandler += QuizGeneratedHandler; 

        // 콤보 이벤트 구독
        ScoreKeeper.OnComboChanged += UpdateComboDisplay;

        // 기존 문제들을 제거하지만 자동으로 생성하지는 않음
        Debug.Log($"기존 문제 개수: {questions.Count}개");
        questions.Clear(); // 기존 문제들 제거
        Debug.Log("기존 문제들을 제거했습니다. 과목 선택 후 문제가 생성됩니다.");
        
        // 점수 초기화는 GameManger.ShowStartScreen()에서 처리
        
        // 초기에는 문제풀기 화면을 숨김 (과목 선택 후에만 표시)
        gameObject.SetActive(false);
        
        // 자동으로 문제 생성하지 않음 - 과목 선택 후에만 생성
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        ScoreKeeper.OnComboChanged -= UpdateComboDisplay;
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
            string selectedSubject = subjectMenu.GetSelectedSubject();
            Debug.Log($"선택된 과목: {selectedSubject}");
            return selectedSubject;
        }
        
        // 기본값으로 랜덤 과목 사용
        string defaultSubject = GetTrendingTopic();
        Debug.Log($"기본 과목 사용: {defaultSubject}");
        return defaultSubject;
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
        // 진행률 바는 무한 게임이므로 고정값 사용
        if (progressBar.maxValue < 100) progressBar.maxValue = 100;
        UpdateProgressDisplay();

        Debug.Log($"문제 생성 완료: {generatedQuestions.Count}개 문제 추가됨. 총 문제 수: {questions.Count}");
        
        // 문제가 생성된 후에만 화면 전환
        gameObject.SetActive(true);
        GameManger.instance.ShowQuizScreen();
        GetNextQuestion();

    }
    private void InitalizeProgressBar()
    {
        // 무한 게임이므로 진행률 바를 고정값으로 설정
        progressBar.maxValue = 100;
        progressBar.value = 0;
        UpdateProgressDisplay();
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
            Debug.Log($"loadNextQuestion이 true입니다. questions.Count: {questions.Count}");
            
            // 게임 오버 체크 (3번 틀렸을 때)
            if (scoreKeeper.IsGameOver())
            {
                Debug.Log("게임 오버! 3번 틀렸습니다. 엔드 스크린으로 이동");
                GameManger.instance.ShowEndScreen();
                return;
            }
            
            if (questions.Count == 0)
            {
                // 문제가 없으면 자동으로 새로운 문제 생성 (같은 과목)
                Debug.Log("문제가 소진되어 새로운 문제를 자동 생성합니다.");
                GenerateQuestionsIFNeeded();
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
        
        Debug.Log($"GetNextQuestion 호출됨. 남은 문제 수: {questions.Count}");
        timer.loadNextQuestion = false;

        // 화면 전환은 QuizGeneratedHandler에서 이미 수행됨
        
        chooseAnswer = false;
        SetButtonState(true);
        SetDefaultButtonSprites();
        GetRandomQuestion();
        OnDisplayQuestion();
        scoreKeeper.IncrementQuestionSeen();
        progressBar.value++;
        UpdateProgressDisplay();
        
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
        Debug.Log($"OnAnswerButtonClicked 호출됨! 선택한 답: {index}");
        chooseAnswer = true;
        
        DisplaySolution(index);
        timer.CanelTimer();
        UpdateScoreDisplay();
        UpdateProgressDisplay();
    }

    private void DisplaySolution(int index)
    {
        Debug.Log($"DisplaySolution 호출됨! 선택한 답: {index}, 정답: {currentQuestion.GetCorrectAnswerIndex()}");
        
        if (index == currentQuestion.GetCorrectAnswerIndex())
        {
            Debug.Log("정답입니다! 점수 계산 시작");
            
            // 정답 처리
            questionText.text = "정답입니다!";
            questionText.color = Color.green;
            
            if (index >= 0 && answerButtons[index] != null)
            {
                answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;
            }
            
            // 시간 보너스 추가 (정답일 때만)
            if (timer != null)
            {
                float remainingTime = timer.time;
                scoreKeeper.AddTimeBonus(remainingTime);
                Debug.Log($"정답! 시간 보너스 추가: {remainingTime}초");
            }
            
            scoreKeeper.IncrementCorrectAnswers();
            Debug.Log("IncrementCorrectAnswers 호출 완료");
        }
        else
        {
            // 오답 처리 - 콤보 브레이크
            questionText.text = "틀렸습니다.\n정답은 " + currentQuestion.GetCorrectAnswer() + "입니다.";
            questionText.color = Color.red;
            
            // 콤보 브레이크
            scoreKeeper.BreakCombo();
            
            // 틀린 답 카운트 증가
            scoreKeeper.IncrementWrongAnswers();
            
            // 남은 생명 표시
            int remainingLives = scoreKeeper.GetRemainingLives();
            if (remainingLives > 0)
            {
                questionText.text += $"\n남은 기회: {remainingLives}번";
            }
            else
            {
                questionText.text += "\n게임 오버!";
            }
            
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

    private void UpdateScoreDisplay()
    {
        if (scoreText != null && scoreKeeper != null)
        {
            scoreText.text = $"점수: {scoreKeeper.GetTotalScore()} | 등급: {scoreKeeper.GetGrade()}";
            scoreText.color = scoreKeeper.GetGradeColor();
        }
        
        // 생명 표시 업데이트
        if (livesText != null && scoreKeeper != null)
        {
            int remainingLives = scoreKeeper.GetRemainingLives();
            livesText.text = $"생명: {remainingLives}";
            
            // 생명에 따른 색상 변경
            if (remainingLives <= 1)
                livesText.color = Color.red;
            else if (remainingLives <= 2)
                livesText.color = Color.yellow;
            else
                livesText.color = Color.green;
        }
    }
    
    private void UpdateComboDisplay(int combo, float multiplier)
    {
        if (comboText != null)
        {
            if (combo > 0)
            {
                comboText.text = $"콤보 {combo}연속! (x{multiplier:F1})";
                comboText.color = GetComboColor(combo);
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }
    
    private Color GetComboColor(int combo)
    {
        if (combo >= 10) return Color.red;
        if (combo >= 6) return Color.magenta;
        if (combo >= 4) return Color.yellow;
        if (combo >= 2) return Color.green;
        return Color.white;
    }
    
    private void UpdateProgressDisplay()
    {
        if (progressText != null && scoreKeeper != null)
        {
            int correctAnswers = scoreKeeper.GetCorrectAnswers();
            int totalQuestions = scoreKeeper.GetQuestionSeen();
            
            progressText.text = $"정답: {correctAnswers}개";
            
            // 정답에 따른 색상 변경
            if (correctAnswers >= 5)
                progressText.color = Color.green;
            else if (correctAnswers >= 3)
                progressText.color = Color.yellow;
            else if (correctAnswers >= 1)
                progressText.color = Color.white;
            else
                progressText.color = Color.gray;
        }
        
        // 진행률 바 업데이트 (무한 게임용)
        if (progressBar != null && scoreKeeper != null)
        {
            int correctAnswers = scoreKeeper.GetCorrectAnswers();
            // 100개마다 한 바퀴 돌도록 설정
            progressBar.value = correctAnswers % 100;
        }
    }
}