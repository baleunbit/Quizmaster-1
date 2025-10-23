using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI 요소들")]
    [SerializeField] public GameObject settingsPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    [SerializeField] private Button muteButton;
    [SerializeField] private TextMeshProUGUI muteButtonText;
    [SerializeField] private Button restartButton;
    [SerializeField] private TextMeshProUGUI restartButtonText;
    [SerializeField] private Button backButton;

    [Header("애니메이션 설정")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private CanvasGroup canvasGroup;
    private AudioManager audioManager;

    private void Start()
    {
        InitializeUI();
        SetupButtons();
        SetupAnimations();
        FindAudioManager();
        
        // 초기에는 숨김
        HideSettingsMenu();
    }

    private void InitializeUI()
    {
        // 제목 설정
        if (titleText != null)
        {
            titleText.text = "설정";
            // Unity에서 설정한 색깔과 크기 유지
        }

        // BGM 볼륨 텍스트 설정
        if (bgmVolumeText != null)
        {
            bgmVolumeText.text = "BGM 볼륨: 7";
            // Unity에서 설정한 색깔과 크기 유지
        }

        // 음소거 버튼 텍스트 설정
        if (muteButtonText != null)
        {
            muteButtonText.text = "음소거";
            // Unity에서 설정한 색깔과 크기 유지
        }

        // 다시하기 버튼 텍스트 설정
        if (restartButtonText != null)
        {
            restartButtonText.text = "다시하기";
            // Unity에서 설정한 색깔과 크기 유지
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
        // BGM 볼륨 슬라이더 설정
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.minValue = 0f;
            bgmVolumeSlider.maxValue = 10f;
            bgmVolumeSlider.value = 7f; // 기본값 7 (70%)
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        // 음소거 버튼 설정
        if (muteButton != null)
        {
            muteButton.onClick.AddListener(OnMuteButtonClicked);
        }

        // 다시하기 버튼 설정
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        // 뒤로가기 버튼 설정
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            SetupButtonText(backButton, "뒤로가기");
        }

    }

    private void SetupButtonText(Button button, string text)
    {
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = text;
            // Unity에서 설정한 색깔과 크기 유지
        }
    }

    private void SetupAnimations()
    {
        // 초기 상태 설정
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void FindAudioManager()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            audioManager = WebBuildBugFixer.SafeFindObjectsOfType<AudioManager>().FirstOrDefault();
        }

        // AudioManager가 있으면 현재 설정으로 UI 업데이트
        if (audioManager != null)
        {
            UpdateUIFromAudioManager();
        }
    }

    private void UpdateUIFromAudioManager()
    {
        if (audioManager == null) return;

        // 현재 볼륨으로 슬라이더 업데이트 (0-1을 0-10으로 변환)
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = audioManager.GetVolume() * 10f;
        }

        // 볼륨 텍스트 업데이트 (0-1을 0-10으로 변환)
        UpdateVolumeText(audioManager.GetVolume() * 10f);

        // 음소거 상태에 따라 버튼 텍스트 업데이트
        UpdateMuteButtonText(audioManager.IsMuted());
    }

    private void OnBGMVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            // 0-10 범위를 0-1 범위로 변환하여 AudioManager에 전달
            audioManager.SetVolume(value / 10f);
        }
        UpdateVolumeText(value);
    }

    private void UpdateVolumeText(float volume)
    {
        if (bgmVolumeText != null)
        {
            // 0-10 범위의 값을 정수로 표시
            int volumeValue = Mathf.RoundToInt(volume);
            bgmVolumeText.text = $"BGM 볼륨: {volumeValue}";
        }
    }

    private void OnMuteButtonClicked()
    {
        // 버튼 클릭 사운드 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayButtonClickSound();
        }
        
        if (audioManager != null)
        {
            audioManager.ToggleMute();
            UpdateMuteButtonText(audioManager.IsMuted());
        }
    }

    private void UpdateMuteButtonText(bool isMuted)
    {
        if (muteButtonText != null)
        {
            muteButtonText.text = isMuted ? "음소거 해제" : "음소거";
        }
    }

    private void OnRestartButtonClicked()
    {
        // 버튼 클릭 사운드 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayButtonClickSound();
        }
        
        Debug.Log("설정에서 다시하기");
        HideSettingsMenu();
        
        // GameManger를 통해 과목 선택 화면으로 이동
        if (GameManger.instance != null)
        {
            GameManger.instance.OnBackToSubjectSelection();
        }
    }

    private void OnBackButtonClicked()
    {
        // 버튼 클릭 사운드 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayButtonClickSound();
        }
        
        Debug.Log("설정에서 뒤로가기");
        HideSettingsMenu();
    }

    private void PauseGameTimer()
    {
        // 퀴즈 화면에서만 타이머 일시정지
        Timer timer = WebBuildBugFixer.SafeFindObjectsOfType<Timer>().FirstOrDefault();
        if (timer != null)
        {
            timer.PauseTimer();
            Debug.Log("게임 타이머 일시정지됨");
        }
    }

    private void ResumeGameTimer()
    {
        // 퀴즈 화면에서만 타이머 재개
        Timer timer = WebBuildBugFixer.SafeFindObjectsOfType<Timer>().FirstOrDefault();
        if (timer != null)
        {
            timer.ResumeTimer();
            Debug.Log("게임 타이머 재개됨");
        }
    }


    public void ShowSettingsMenu()
    {
        Debug.Log("SettingsMenu.ShowSettingsMenu() 호출됨");
        
        // 타이머 일시정지
        PauseGameTimer();
        
        if (settingsPanel != null)
        {
            Debug.Log("SettingsPanel 활성화 중...");
            
            // Canvas를 최상위로 이동 (다른 UI 위에 표시)
            Canvas settingsCanvas = settingsPanel.GetComponentInParent<Canvas>();
            if (settingsCanvas != null)
            {
                settingsCanvas.sortingOrder = 100; // 높은 값으로 설정하여 최상위에 표시
            }
            
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("SettingsPanel이 연결되지 않았습니다! SettingsMenu의 Settings Panel 필드를 확인하세요.");
        }

        // AudioManager 설정으로 UI 업데이트
        UpdateUIFromAudioManager();

        // 페이드 인 애니메이션
        StartCoroutine(FadeInAnimation());
    }

    public void HideSettingsMenu()
    {
        Debug.Log("HideSettingsMenu() 호출됨");
        
        // 타이머 재개
        ResumeGameTimer();
        
        // 페이드 아웃 애니메이션 후 숨김
        StartCoroutine(FadeOutAndHide());
    }

    private System.Collections.IEnumerator FadeInAnimation()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            }
            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    private System.Collections.IEnumerator FadeOutAndHide()
    {
        Debug.Log("FadeOutAndHide() 시작");
        float elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            }
            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (settingsPanel != null)
        {
            Debug.Log("SettingsPanel 비활성화");
            settingsPanel.SetActive(false);
        }
        
        Debug.Log("FadeOutAndHide() 완료");
    }

    // GameManger에서 호출할 수 있는 메서드
    public void OnBackToStartScreen()
    {
        HideSettingsMenu();
    }
}
