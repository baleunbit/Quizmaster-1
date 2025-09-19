using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManger : MonoBehaviour
{
    public static GameManger instance {get; private set;}

    [SerializeField]private Quiz quiz;
    [SerializeField]private EndScreen endScreen;
    [SerializeField] private GameObject lodingCanvas;
    [SerializeField] private SubjectSelectionMenu subjectSelectionMenu;
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
        // 게임 시작 시 과목 선택 메뉴 표시
        ShowSubjectSelectionMenu();
    }
    public void ShowSubjectSelectionMenu()
    {
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
        quiz.gameObject.SetActive(false);
        endScreen.gameObject.SetActive(true);
        endScreen.ShowFinalScore();
        lodingCanvas.SetActive(false);
    }

    public void ShowlodingSceen()
    {
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
}
