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
        // 게임 시작 시 시작화면 표시
        ShowStartScreen();
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
