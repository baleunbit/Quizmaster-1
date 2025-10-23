using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    [SerializeField] TextMeshProUGUI livesText;
    ScoreKeeper scoreKeeper;

    [Header("progressBar")]
    [SerializeField] Slider progressBar;
    [SerializeField] TextMeshProUGUI progressText;

    [Header("Questiongeneration")]
    [SerializeField] ChatGPTClient chatGPTClint;
    [SerializeField] int qestionCount = 10;
    [SerializeField] TextMeshProUGUI londingText;

    [Header("힌트 시스템")]
    [SerializeField] Button hintButton;
    [SerializeField] TextMeshProUGUI hintText;
    [SerializeField] int hintCost = 200; // 힌트 사용 시 차감될 점수
    private bool hintUsed = false; // 현재 문제에서 힌트를 사용했는지
    private bool timeOverProcessed = false; // 시간 초과 처리 완료 여부


    bool isGenerateQuestions = false;

    void Start()
    {
        timer = WebBuildBugFixer.SafeFindObjectsOfType<Timer>().FirstOrDefault();
        
        // ScoreKeeper 싱글톤 인스턴스 사용
        scoreKeeper = ScoreKeeper.instance;
        
        chatGPTClint.quizGenerateHandler += QuizGeneratedHandler;

        // 힌트 버튼 초기화
        if (hintButton != null)
        {
            hintButton.onClick.AddListener(OnHintButtonClicked);
        }

        // 기존 문제들을 제거하지만 자동으로 생성하지는 않음
        questions.Clear(); // 기존 문제들 제거
        
        // 점수 초기화는 GameManger.ShowStartScreen()에서 처리
        
        // 초기에는 문제풀기 화면을 숨김 (과목 선택 후에만 표시)
        gameObject.SetActive(false);
        
        // 자동으로 문제 생성하지 않음 - 과목 선택 후에만 생성
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제 (필요시 추가)
    }
    private void GenerateQuestionsIFNeeded()
    {
        if (isGenerateQuestions) return;

        isGenerateQuestions = true;
        GameManger.instance.ShowlodingSceen();

        // 과목 선택 메뉴에서 선택된 과목 사용
        string topiToUse = GetSelectedSubject();
        
        // 캐시에서 문제 확인
        if (QuestionCache.instance != null && QuestionCache.instance.HasEnoughQuestions(topiToUse, qestionCount))
        {
            List<QuestionSO> cachedQuestions = QuestionCache.instance.GetCachedQuestions(topiToUse, qestionCount);
            if (cachedQuestions.Count > 0)
            {
                QuizGeneratedHandler(cachedQuestions);
                return;
            }
        }
        
        chatGPTClint.GenerateQuestions(qestionCount, topiToUse);
    }

    private string GetSelectedSubject()
    {
        // SubjectSelectionMenu에서 선택된 과목 가져오기
        SubjectSelectionMenu subjectMenu = WebBuildBugFixer.SafeFindObjectsOfType<SubjectSelectionMenu>().FirstOrDefault();
        if (subjectMenu != null)
        {
            string selectedSubject = subjectMenu.GetSelectedSubject();
            return selectedSubject;
        }
        
        // 기본값으로 랜덤 과목 사용
        string defaultSubject = GetTrendingTopic();
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
            // 게임 오버 체크 (3번 틀렸을 때)
            if (scoreKeeper.IsGameOver())
            {
                GameManger.instance.ShowEndScreen();
                return;
            }
            
            if (questions.Count == 0)
            {
                // 문제가 없으면 자동으로 새로운 문제 생성 (같은 과목)
                GenerateQuestionsIFNeeded();
            }
            else
            {
                GetNextQuestion();
            }
        }

        // SolutionTime이고 답을 선택하지 않았을 때 자동 처리 (한 번만)
        if (timer.isProblemTime == false && chooseAnswer == false && !timeOverProcessed)
        {
            DisplayTimeOver();
        }
    }

    private void GetNextQuestion()
    {
        if (questions.Count <= 0)
        {
            return;
        }
        
        timer.loadNextQuestion = false;

        // 화면 전환은 QuizGeneratedHandler에서 이미 수행됨
        
        chooseAnswer = false;
        hintUsed = false; // 힌트 사용 상태 초기화
        timeOverProcessed = false; // 시간 초과 처리 상태 초기화
        SetButtonState(true);
        SetDefaultButtonSprites();
        GetRandomQuestion();
        OnDisplayQuestion();
        UpdateHintButton(); // 힌트 버튼 상태 업데이트
        
        // 힌트 텍스트 숨기기
        if (hintText != null)
        {
            hintText.gameObject.SetActive(false);
        }
        // IncrementQuestionSeen()은 DisplaySolution() 또는 DisplayTimeOver()에서 호출됨
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
        WebBuildSettings.SetWebText(questionText, currentQuestion.GetQuestion());
        questionText.color = Color.white;

        // 답 버튼들에 텍스트 설정
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    WebBuildSettings.SetWebText(buttonText, currentQuestion.GetAnswer(i));
                    // 웹에서 안전한 폰트 설정
                    WebBuildSettings.SetWebFont(buttonText, null);
                }
            }
        }
    }

    public void OnAnswerButtonClicked(int index)
    {
        // 버튼 클릭 사운드 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayButtonClickSound();
        }
        
        chooseAnswer = true;
        
        // 힌트 텍스트 숨기기
        if (hintText != null)
        {
            hintText.gameObject.SetActive(false);
        }
        
        DisplaySolution(index);
        timer.CanelTimer();
        UpdateScoreDisplay();
        UpdateProgressDisplay();
    }

    private void DisplayTimeOver()
    {
        // 시간 초과 처리 완료 표시 (중복 처리 방지)
        timeOverProcessed = true;
        
        // 문제를 봤으므로 카운트 증가
        scoreKeeper.IncrementQuestionSeen();
        
        // 시간 초과 처리 (목숨 감소)
        questionText.text = "시간 초과!\n정답은 " + currentQuestion.GetCorrectAnswer() + "입니다.";
        questionText.color = new Color(1f, 0.5f, 0f); // 주황색 (RGB: 255, 128, 0)
        
        // 틀린 답 카운트 증가 (시간 초과도 목숨 감소) - 한 번만 실행
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
        
        // 정답 버튼 표시
        int correctIndex = currentQuestion.GetCorrectAnswerIndex();
        if (correctIndex >= 0 && correctIndex < answerButtons.Length && answerButtons[correctIndex] != null)
        {
            answerButtons[correctIndex].GetComponent<Image>().sprite = correctAnswerSprite;
        }
        
        // 점수 표시 업데이트
        UpdateScoreDisplay();
        SetButtonState(false);
    }

    private void DisplaySolution(int index)
    {
        // 문제를 봤으므로 카운트 증가 (정답/오답 관계없이)
        scoreKeeper.IncrementQuestionSeen();
        
        if (index == currentQuestion.GetCorrectAnswerIndex())
        {
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
            }
            
            scoreKeeper.IncrementCorrectAnswers();
        }
        else
        {
            // 오답 처리
            questionText.text = "틀렸습니다.\n정답은 " + currentQuestion.GetCorrectAnswer() + "입니다.";
            questionText.color = Color.red;
            
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
        
        // 점수 표시 업데이트 (정답/오답 처리 후)
        UpdateScoreDisplay();
        
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
            int totalScore = scoreKeeper.GetTotalScore();
            string grade = scoreKeeper.GetGrade();
            Color gradeColor = scoreKeeper.GetGradeColor();
            
            Debug.Log($"점수 표시 업데이트: {totalScore}점, 등급: {grade}");
            
            WebBuildSettings.SetWebText(scoreText, $"점수: {totalScore} | 등급: {grade}");
            scoreText.color = gradeColor;
        }
        
        // 생명 표시 업데이트
        if (livesText != null && scoreKeeper != null)
        {
            int remainingLives = scoreKeeper.GetRemainingLives();
            WebBuildSettings.SetWebText(livesText, $"생명: {remainingLives}");
            
            // 생명에 따른 색상 변경
            if (remainingLives <= 1)
                livesText.color = Color.red;
            else if (remainingLives <= 2)
                livesText.color = Color.yellow;
            else
                livesText.color = Color.green;
        }
    }
    
    
    private void UpdateProgressDisplay()
    {
        if (progressText != null && scoreKeeper != null)
        {
            int correctAnswers = scoreKeeper.GetCorrectAnswers();
            int totalQuestions = scoreKeeper.GetQuestionSeen();
            
            WebBuildSettings.SetWebText(progressText, $"정답: {correctAnswers}개");
            
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

    /// <summary>
    /// 힌트 버튼 클릭 시 호출되는 메서드
    /// </summary>
    private void OnHintButtonClicked()
    {
        Debug.Log("=== 힌트 버튼 클릭됨 ===");
        Debug.Log($"hintUsed: {hintUsed}, chooseAnswer: {chooseAnswer}");
        Debug.Log($"currentQuestion: {currentQuestion != null}");
        Debug.Log($"hintText: {hintText != null}");
        
        // 버튼 클릭 사운드 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayButtonClickSound();
        }

        // 이미 힌트를 사용했거나 답을 선택했으면 무시
        if (hintUsed || chooseAnswer)
        {
            Debug.Log("힌트 사용 불가: 이미 사용했거나 답을 선택함");
            return;
        }

        // 미리 로드된 힌트 사용
        UsePreloadedHint();
    }

    /// <summary>
    /// 미리 로드된 힌트를 사용하는 메서드
    /// </summary>
    private void UsePreloadedHint()
    {
        if (currentQuestion == null) 
        {
            Debug.LogError("currentQuestion이 null입니다!");
            return;
        }

        hintUsed = true;
        
        // 힌트 사용 시 점수 차감
        if (scoreKeeper != null)
        {
            scoreKeeper.DeductScore(hintCost);
            UpdateScoreDisplay();
        }
        
        // 미리 로드된 힌트 표시
        string hint = currentQuestion.GetHint();
        Debug.Log($"힌트 내용: '{hint}' (길이: {hint?.Length ?? 0})");
        
        if (string.IsNullOrEmpty(hint))
        {
            Debug.LogWarning("힌트가 비어있습니다!");
            hint = "힌트를 사용할 수 없습니다.";
        }
        
        if (hintText != null)
        {
            hintText.text = $"💡 힌트: {hint}";
            hintText.gameObject.SetActive(true);
            Debug.Log($"힌트 텍스트 설정 완료: {hintText.text}");
        }
        else
        {
            Debug.LogError("hintText가 null입니다!");
        }
        
        // 힌트 버튼 상태 업데이트
        UpdateHintButton();
        
        Debug.Log($"힌트 사용! {hintCost}점 차감, 힌트: {hint}");
    }


    /// <summary>
    /// 힌트 버튼 상태를 업데이트하는 메서드
    /// </summary>
    private void UpdateHintButton()
    {
        if (hintButton == null) return;

        // 힌트를 이미 사용했거나 답을 선택했으면 버튼 비활성화
        bool canUseHint = !hintUsed && !chooseAnswer;
        hintButton.interactable = canUseHint;
        
        // 힌트 버튼 텍스트 업데이트
        if (hintButton.GetComponentInChildren<TextMeshProUGUI>() != null)
        {
            string buttonText = hintUsed ? "힌트 사용됨" : $"힌트 ({hintCost}점)";
            hintButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
        }
    }
}