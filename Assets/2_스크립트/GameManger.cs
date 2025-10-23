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
    [SerializeField] private SettingsMenu settingsMenu;
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

    void Update()
    {
        // ESC키로 설정 메뉴 열기/닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC키 감지됨!");
            ToggleSettingsMenu();
        }
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
        if (settingsMenu != null)
        {
            settingsMenu.HideSettingsMenu();
        }
        quiz.gameObject.SetActive(false);
        endScreen.gameObject.SetActive(false);
        lodingCanvas.SetActive(false);
        
        // 점수 초기화는 게임 시작할 때만 (퀴즈 화면 진입 시에는 제거)
        // 게임 오버 후에는 점수를 유지하므로 초기화하지 않음
        
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
        if (settingsMenu != null)
        {
            settingsMenu.HideSettingsMenu();
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
        if (settingsMenu != null)
        {
            settingsMenu.HideSettingsMenu();
        }
        quiz.gameObject.SetActive(true);
        endScreen.gameObject.SetActive(false);
        lodingCanvas.SetActive(false);
        
        // 점수 초기화는 게임 시작할 때만 (퀴즈 화면 진입 시에는 제거)
        
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
        if (settingsMenu != null)
        {
            settingsMenu.HideSettingsMenu();
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
        if (settingsMenu != null)
        {
            settingsMenu.HideSettingsMenu();
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

    public void ShowSettingsMenu()
    {
        Debug.Log("GameManger.ShowSettingsMenu() 호출됨");
        
        if (settingsMenu != null)
        {
            Debug.Log("SettingsMenu가 연결되어 있음 - ShowSettingsMenu() 호출");
            settingsMenu.ShowSettingsMenu();
        }
        else
        {
            Debug.LogError("SettingsMenu가 연결되지 않았습니다! GameManger의 Settings Menu 필드를 확인하세요.");
        }
        
        // 다른 화면들은 숨기지 않고 설정 메뉴만 오버레이로 표시
        // (설정은 다른 화면 위에 표시되는 오버레이)
    }

    public void ToggleSettingsMenu()
    {
        Debug.Log("ToggleSettingsMenu() 호출됨");
        
        if (settingsMenu != null)
        {
            Debug.Log("SettingsMenu가 연결되어 있음");
            
            // SettingsPanel이 활성화되어 있는지 확인
            if (settingsMenu.settingsPanel != null && settingsMenu.settingsPanel.activeInHierarchy)
            {
                Debug.Log("ESC키로 설정 메뉴 닫기");
                settingsMenu.HideSettingsMenu();
            }
            else
            {
                Debug.Log("ESC키로 설정 메뉴 열기");
                settingsMenu.ShowSettingsMenu();
            }
        }
        else
        {
            Debug.LogError("SettingsMenu가 연결되지 않았습니다!");
        }
    }
}
