using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI finalScoreText;
    [SerializeField] ScoreKeeper scoreKeeper;
    [SerializeField] Button subjectSelectionButton;

    private void Start()
    {
        if (subjectSelectionButton != null)
        {
            subjectSelectionButton.onClick.AddListener(OnSubjectSelectionClicked);
        }
    }

    public void ShowFinalScore()
    {
        finalScoreText.text = "완료되었습니다!\r\n" + $"  최종 점수는 {scoreKeeper.CalculateScore()}%입니다.";
    }

    private void OnSubjectSelectionClicked()
    {
        if (GameManger.instance != null)
        {
            GameManger.instance.OnBackToSubjectSelection();
        }
    }
}