using UnityEngine;

[CreateAssetMenu(menuName = "Quiz Question", fileName = "New Question")]
public class QuestionSO : ScriptableObject
{
    [TextArea(2, 6)]
    [SerializeField] string question = "���⿡ ������ �����ּ���.";
    [SerializeField] string[] answers = new string[4];
    [SerializeField] int correctAnswerIndex;
    [SerializeField] string hint = ""; // 힌트 필드 추가

    public string GetQuestion()
    {
        return question;
    }

    public string GetAnswer(int i)
    {
        return answers[i];
    }

    public string GetCorrectAnswer()
    {
        return answers[correctAnswerIndex];
    }

    public int GetCorrectAnswerIndex()
    {
        return correctAnswerIndex;
    }

    public string GetHint()
    {
        return hint;
    }

    public void setData(string q, string[] a, int correctindex)
    {
        question = q;
        answers = a;
        correctAnswerIndex = correctindex;
    }

    public void setData(string q, string[] a, int correctindex, string h)
    {
        question = q;
        answers = a;
        correctAnswerIndex = correctindex;
        hint = h;
    }
}