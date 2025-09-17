using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManger : MonoBehaviour
{
    public static GameManger instance {get; private set;}

    [SerializeField]private Quiz quiz;
    [SerializeField]private EndScreen endScreen;
    [SerializeField] private GameObject lodingCanvas;
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
   public void ShowQuizScreen()
    {
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
}
