using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubjectSelectionMenu : MonoBehaviour
{
    [Header("UI 요소들")]
    [SerializeField] private GameObject subjectMenuPanel;
    [SerializeField] private Button[] subjectButtons = new Button[4];
    [SerializeField] private TextMeshProUGUI[] subjectTexts = new TextMeshProUGUI[4];
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button backButton;

    [Header("과목 설정")]
    [SerializeField] private string[] subjects = {
        "일반상식",
        "과학",
        "역사",
        "스포츠"
    };

    [Header("과목 설명")]
    [SerializeField] private string[] subjectDescriptions = {
        "다양한 분야의 상식 문제",
        "과학, 수학, 물리학 관련 문제",
        "한국사, 세계사 관련 문제",
        "축구, 야구, 올림픽 등 스포츠 문제"
    };

    private int selectedSubjectIndex = -1;
    private ChatGPTClient chatGPTClient;
    private Quiz quiz;

    private void Start()
    {
        InitializeUI();
        SetupButtons();
        chatGPTClient = FindFirstObjectByType<ChatGPTClient>();
        quiz = FindFirstObjectByType<Quiz>();
    }

    private void InitializeUI()
    {
        // 제목 설정
        if (titleText != null)
        {
            titleText.text = "퀴즈 과목을 선택하세요";
        }

        // 과목 버튼 텍스트 설정
        for (int i = 0; i < subjectButtons.Length && i < subjects.Length; i++)
        {
            if (subjectTexts[i] != null)
            {
                subjectTexts[i].text = subjects[i];
            }
        }

        // 시작 버튼 비활성화
        if (startButton != null)
        {
            startButton.interactable = false;
        }
    }

    private void SetupButtons()
    {
        // 과목 선택 버튼들 설정
        for (int i = 0; i < subjectButtons.Length; i++)
        {
            int index = i; // 클로저를 위한 지역 변수
            if (subjectButtons[i] != null)
            {
                subjectButtons[i].onClick.AddListener(() => SelectSubject(index));
            }
        }

        // 시작 버튼 설정
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartQuiz);
        }

        // 뒤로가기 버튼 설정
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBack);
        }
    }

    private void SelectSubject(int subjectIndex)
    {
        selectedSubjectIndex = subjectIndex;
        
        // 모든 버튼의 선택 상태 초기화
        for (int i = 0; i < subjectButtons.Length; i++)
        {
            if (subjectButtons[i] != null)
            {
                // 버튼 색상으로 선택 상태 표시
                ColorBlock colors = subjectButtons[i].colors;
                if (i == subjectIndex)
                {
                    colors.normalColor = Color.yellow;
                    colors.selectedColor = Color.yellow;
                }
                else
                {
                    colors.normalColor = Color.white;
                    colors.selectedColor = Color.white;
                }
                subjectButtons[i].colors = colors;
            }
        }

        // 시작 버튼 활성화
        if (startButton != null)
        {
            startButton.interactable = true;
        }

        Debug.Log($"선택된 과목: {subjects[subjectIndex]}");
    }

    private void StartQuiz()
    {
        if (selectedSubjectIndex == -1)
        {
            Debug.LogWarning("과목을 선택해주세요!");
            return;
        }

        string selectedSubject = subjects[selectedSubjectIndex];
        Debug.Log($"퀴즈 시작: {selectedSubject}");

        // 과목 선택 메뉴 숨기기
        if (subjectMenuPanel != null)
        {
            subjectMenuPanel.SetActive(false);
        }

        // ChatGPT로 퀴즈 생성 시작
        if (chatGPTClient != null)
        {
            chatGPTClient.GenerateQuestions(3, selectedSubject);
        }

        // 로딩 화면 표시
        if (GameManger.instance != null)
        {
            GameManger.instance.ShowlodingSceen();
        }
    }

    private void GoBack()
    {
        // 메인 메뉴로 돌아가기 (필요시 구현)
        Debug.Log("메인 메뉴로 돌아가기");
    }

    public void ShowSubjectMenu()
    {
        if (subjectMenuPanel != null)
        {
            subjectMenuPanel.SetActive(true);
        }
        
        // 선택 상태 초기화
        selectedSubjectIndex = -1;
        InitializeUI();
    }

    public void HideSubjectMenu()
    {
        if (subjectMenuPanel != null)
        {
            subjectMenuPanel.SetActive(false);
        }
    }

    public string GetSelectedSubject()
    {
        if (selectedSubjectIndex >= 0 && selectedSubjectIndex < subjects.Length)
        {
            return subjects[selectedSubjectIndex];
        }
        return "일반상식"; // 기본값
    }
}
