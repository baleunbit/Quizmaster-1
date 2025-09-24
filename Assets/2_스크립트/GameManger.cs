using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManger : MonoBehaviour
{
    public static GameManger instance {get; private set;}

    [SerializeField]private Quiz quiz;
    [SerializeField]private EndScreen endScreen;
    [SerializeField] private GameObject lodingCanvas;
    [SerializeField] private SubjectSelectionMenu subjectSelectionMenu;
    [SerializeField] private StartScreen startScreen;
    [SerializeField] private AudioManager audioManager;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance)
        {
            Destroy(gameObject);  
        }
    }

    void Start()
    {
        // AudioManager 초기화
        InitializeAudioManager();
        
        // 게임 시작 시 시작화면 표시
        ShowStartScreen();
    }

    private void InitializeAudioManager()
    {
        // AudioManager가 없으면 찾아서 연결
        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioManager>();
        }
        
        // AudioManager가 여전히 없으면 생성
        if (audioManager == null)
        {
            GameObject audioManagerObj = new GameObject("AudioManager");
            audioManager = audioManagerObj.AddComponent<AudioManager>();
        }
    }
    public void ShowStartScreen()
    {
        if (startScreen != null)
        {
            startScreen.ShowStartScreen();
        }
        if (subjectSelectionMenu != null)
        {
            subjectSelectionMenu.HideSubjectMenu();
        }
        quiz.gameObject.SetActive(false);
        endScreen.gameObject.SetActive(false);
        lodingCanvas.SetActive(false);
        
        // 시작화면 BGM 재생
        if (audioManager != null)
        {
            audioManager.PlayBGM(AudioManager.BGMType.StartScreen);
        }
    }

    public void ShowSubjectSelectionMenu()
    {
        if (startScreen != null)
        {
            startScreen.HideStartScreen();
        }
        if (subjectSelectionMenu != null)
        {
            subjectSelectionMenu.ShowSubjectMenu();
        }
        quiz.gameObject.SetActive(false);
        endScreen.gameObject.SetActive(false);
        lodingCanvas.SetActive(false);
        
        // 과목 선택 BGM 재생
        if (audioManager != null)
        {
            audioManager.PlayBGM(AudioManager.BGMType.SubjectSelection);
        }
    }

    public void ShowQuizScreen()
    {
        if (startScreen != null)
        {
            startScreen.HideStartScreen();
        }
        if (subjectSelectionMenu != null)
        {
            subjectSelectionMenu.HideSubjectMenu();
        }
        quiz.gameObject.SetActive(true);
        endScreen.gameObject.SetActive(false);
        lodingCanvas.SetActive(false);
        
        // 퀴즈 BGM 재생
        if (audioManager != null)
        {
            audioManager.PlayBGM(AudioManager.BGMType.Quiz);
        }
    }
    public void ShowEndScreen()
    {
        if (startScreen != null)
        {
            startScreen.HideStartScreen();
        }
        quiz.gameObject.SetActive(false);
        endScreen.gameObject.SetActive(true);
        endScreen.ShowFinalScore();
        lodingCanvas.SetActive(false);
        
        // 결과 화면 BGM 재생
        if (audioManager != null)
        {
            audioManager.PlayBGM(AudioManager.BGMType.EndScreen);
        }
    }

    public void ShowlodingSceen()
    {
        if (startScreen != null)
        {
            startScreen.HideStartScreen();
        }
        quiz.gameObject.SetActive(false);
        endScreen.gameObject.SetActive(false);
        lodingCanvas.SetActive(true);
        
        // 로딩 BGM 재생
        if (audioManager != null)
        {
            audioManager.PlayBGM(AudioManager.BGMType.Loading);
        }
    }
    public void OnReplayLevel()
    {
       SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnBackToSubjectSelection()
    {
        ShowSubjectSelectionMenu();
    }

    public void OnBackToStartScreen()
    {
        ShowStartScreen();
    }
}
