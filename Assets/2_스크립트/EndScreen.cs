using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI finalScoreText;
    [SerializeField] ScoreKeeper scoreKeeper;

    public void ShowFinalScore()
    {
        finalScoreText.text = "축하합니다!\r\n" + $" 당신은 점수는 {scoreKeeper.CalculateScore()}%입니다.";
    }
}
