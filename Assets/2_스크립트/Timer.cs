using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] float problemTime = 10f;   // ���� Ǫ�� �ð�
    [SerializeField] float solutionTime = 3f;   // ����, Ʋ�Ƚ��ϴ� �ð�
    float time = 0;

    [HideInInspector] public bool isProblemTime = true;
    [HideInInspector] public float fillAmount;
    [HideInInspector] public bool loadNextQuestion;

    private void Start()
    {
        time = problemTime;
        loadNextQuestion = true;
    }

    private void Update()
    {
        TimerCountDown();
        UpdateFillAmount();
    }

    private void UpdateFillAmount()
    {
        if (isProblemTime)
            fillAmount = time / problemTime;
        else
            fillAmount = time / solutionTime;
    }

    private void TimerCountDown()
    {
        time -= Time.deltaTime;
        if (time <= 0)
        {
            if (isProblemTime)
            {
                isProblemTime = false;
                time = solutionTime;
            }
            else
            {
                isProblemTime = true;
                time = problemTime;
                loadNextQuestion = true;
            }
        }
    }
    public void CanelTimer()
    {
        time = 0;
    }
}
