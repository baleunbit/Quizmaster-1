using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StartScreen : MonoBehaviour
{
    [Header("UI 요소들")]
    [SerializeField] private GameObject startScreenPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("애니메이션 설정")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float buttonDelay = 0.5f;

    private CanvasGroup canvasGroup;

    private void Start()
    {
        InitializeUI();
        SetupButtons();
        SetupAnimations();
        ShowStartScreen();
    }

    private void InitializeUI()
    {
        // 제목 설정
        if (titleText != null)
        {
            titleText.text = "Quiz Master";
            titleText.fontSize = 72;
            titleText.color = Color.white;
        }

        // 부제목 설정
        if (subtitleText != null)
        {
            subtitleText.text = "지식을 테스트해보세요!";
            subtitleText.fontSize = 24;
            subtitleText.color = Color.yellow;
        }

        // CanvasGroup 컴포넌트 가져오기 또는 추가
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void SetupButtons()
    {
        // 시작 버튼 설정
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
            SetupButtonText(startButton, "게임 시작");
        }

        // 설정 버튼 설정
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            SetupButtonText(settingsButton, "설정");
        }

        // 종료 버튼 설정
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
            SetupButtonText(exitButton, "게임 종료");
        }
    }

    private void SetupButtonText(Button button, string text)
    {
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = text;
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
        }
    }

    private void SetupAnimations()
    {
        // 초기 상태 설정
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        // 버튼들 초기에는 비활성화
        SetButtonsInteractable(false);
    }


    private System.Collections.IEnumerator FadeInAnimation()
    {
        float elapsedTime = 0f;

        // 페이드 인 효과
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            }
            yield return null;
        }

        // 버튼 활성화 지연
        yield return new WaitForSeconds(buttonDelay);
        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (startButton != null)
            startButton.interactable = interactable;
        if (settingsButton != null)
            settingsButton.interactable = interactable;
        if (exitButton != null)
            exitButton.interactable = interactable;
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("게임 시작 버튼 클릭");
        
        // 페이드 아웃 후 과목 선택 메뉴로 이동
        StartCoroutine(FadeOutAndStartGame());
    }

    private void OnSettingsButtonClicked()
    {
        Debug.Log("설정 버튼 클릭");
        // 향후 설정 메뉴 구현
        ShowMessage("설정 기능은 추후 업데이트 예정입니다.");
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("게임 종료 버튼 클릭");
        
        // 확인 대화상자 표시
        ShowExitConfirmation();
    }

    private System.Collections.IEnumerator FadeOutAndStartGame()
    {
        SetButtonsInteractable(false);
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeInDuration);
            }
            yield return null;
        }

        // 시작화면 숨기기
        HideStartScreen();

        // 과목 선택 메뉴 표시
        if (GameManger.instance != null)
        {
            GameManger.instance.ShowSubjectSelectionMenu();
        }
    }

    private void ShowExitConfirmation()
    {
        // 간단한 확인 메시지 (향후 개선 가능)
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            ShowMessage("웹에서는 브라우저를 닫아주세요.");
        }
        else
        {
            Application.Quit();
        }
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message);
        // 향후 토스트 메시지나 팝업으로 개선 가능
    }

    public void ShowStartScreen()
    {
        if (startScreenPanel != null)
        {
            startScreenPanel.SetActive(true);
        }
        
        // 애니메이션 재시작
        SetupAnimations();
        StartCoroutine(FadeInAnimation());
    }

    public void HideStartScreen()
    {
        if (startScreenPanel != null)
        {
            startScreenPanel.SetActive(false);
        }
    }

    // GameManger에서 호출할 수 있는 메서드
    public void OnBackToStartScreen()
    {
        ShowStartScreen();
    }
}
