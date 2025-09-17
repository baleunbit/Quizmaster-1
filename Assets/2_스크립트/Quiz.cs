using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quiz : MonoBehaviour
{
    [Header("����")]
    [SerializeField] TextMeshProUGUI questionText;
    QuestionSO currentQuestion;//�ν����Ϳ��� ������ ����
    [SerializeField] List<QuestionSO> questions = new List<QuestionSO>();

    [Header("����")]
    [SerializeField] GameObject[] answerButtons;

    [Header("��ư����")]
    [SerializeField] Sprite defaultAnswerSprite;
    [SerializeField] Sprite correctAnswerSprite;

    [Header("Ÿ�̸�")]
    [SerializeField] Image timerImage;
    [SerializeField] Sprite problemTimerSprite;
    [SerializeField] Sprite solutionTimerSprite;
    Timer timer;
    bool chooseAnswer = false;

    [Header("����")]
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
            "����","������", "Ȯ���� ���", "������ ���","�ѱ���",
            "��ȸŽ��","����Ž��","����Ž��"
        };
        int ramdomindex = UnityEngine.Random.Range(0, trendingtopics.Length);
        return trendingtopics[ramdomindex];
    }

    private void QuizGeneratedHandler(List<QuestionSO> generatedQuestions)
    {
        //������ �޾ƿµ�
        isGenerateQuestions = false;

        if (generatedQuestions == null || generatedQuestions.Count == 0)
        {
            Debug.LogError("���� ���� ����");
            londingText.text = "���� ���� ����. �ٽ� �õ����ּ���.";
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
        // Ÿ�̸� �̹��� ������Ʈ
        if (timer.isProblemTime)
            timerImage.sprite = problemTimerSprite;
        else
            timerImage.sprite = solutionTimerSprite;
        timerImage.fillAmount = timer.fillAmount;

        // ���� ���� �ҷ�����
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

        // SolutionTime�̰� ���� �������� �ʾ��� �� ���� ó��
        if (timer.isProblemTime == false && chooseAnswer == false)
        {
            DisplaySolution(-1);
        }
    }

    private void GetNextQuestion()
    {
        if (questions.Count <= 0)
        {
            Debug.Log("���� ����");
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
        questions.RemoveAt(Randomindex); // �ߺ� ���� ����
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
            questionText.text = "�����Դϴ�!";
            if (index >= 0)
                answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;
            scoreKeeper.IncrementCorrectAnswers();
        }
        else
        {
            questionText.text = "Ʋ�Ƚ��ϴ�. ������ " + currentQuestion.GetCorrectAnswer();
        }
        SetButtonState(false);
        // timer.CancelTimer = (); // �߸��� �ڵ��̹Ƿ� ����
    }

    private void SetButtonState(bool state)
    {
        foreach (GameObject obj in answerButtons)
        {
            obj.GetComponent<Button>().interactable = state;
        }
    }
}
