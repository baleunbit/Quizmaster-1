using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("배경음악 설정")]
    [SerializeField] private AudioClip startScreenBGM;
    [SerializeField] private AudioClip subjectSelectionBGM;
    [SerializeField] private AudioClip quizBGM;
    [SerializeField] private AudioClip endScreenBGM;
    [SerializeField] private AudioClip loadingBGM;

    [Header("오디오 설정")]
    [SerializeField] private float defaultVolume = 0.7f;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private bool isMuted = false;

    private AudioSource audioSource;
    private AudioClip currentBGM;
    private Coroutine fadeCoroutine;

    // BGM 타입 열거형
    public enum BGMType
    {
        StartScreen,
        SubjectSelection,
        Quiz,
        EndScreen,
        Loading
    }

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSource();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSource()
    {
        // AudioSource 컴포넌트 가져오기 또는 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // AudioSource 설정
        audioSource.loop = true;
        audioSource.volume = defaultVolume;
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // 게임 시작 시 시작화면 BGM 재생
        PlayBGM(BGMType.StartScreen);
    }

    /// <summary>
    /// 지정된 타입의 BGM을 재생합니다.
    /// </summary>
    /// <param name="bgmType">재생할 BGM 타입</param>
    /// <param name="fadeTransition">페이드 전환 사용 여부</param>
    public void PlayBGM(BGMType bgmType, bool fadeTransition = true)
    {
        AudioClip targetBGM = GetBGMClip(bgmType);
        
        if (targetBGM == null)
        {
            // BGM 클립이 없어도 게임 진행에 문제없도록 경고만 출력
            Debug.Log($"BGM 클립이 설정되지 않았습니다: {bgmType} (정상 동작)");
            return;
        }

        // 같은 BGM이 이미 재생 중이면 무시
        if (currentBGM == targetBGM && audioSource.isPlaying)
        {
            return;
        }

        if (fadeTransition)
        {
            StartCoroutine(FadeToNewBGM(targetBGM));
        }
        else
        {
            PlayBGMImmediate(targetBGM);
        }
    }

    /// <summary>
    /// BGM 타입에 따른 AudioClip을 반환합니다.
    /// </summary>
    private AudioClip GetBGMClip(BGMType bgmType)
    {
        switch (bgmType)
        {
            case BGMType.StartScreen:
                return startScreenBGM;
            case BGMType.SubjectSelection:
                return subjectSelectionBGM;
            case BGMType.Quiz:
                return quizBGM;
            case BGMType.EndScreen:
                return endScreenBGM;
            case BGMType.Loading:
                return loadingBGM;
            default:
                return null;
        }
    }

    /// <summary>
    /// 페이드 전환으로 새로운 BGM을 재생합니다.
    /// </summary>
    private IEnumerator FadeToNewBGM(AudioClip newBGM)
    {
        // 기존 페이드 코루틴이 실행 중이면 중지
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeOutAndPlayNewBGM(newBGM));
        yield return fadeCoroutine;
    }

    /// <summary>
    /// 현재 BGM을 페이드 아웃하고 새로운 BGM을 페이드 인합니다.
    /// </summary>
    private IEnumerator FadeOutAndPlayNewBGM(AudioClip newBGM)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        // 페이드 아웃
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        // 새로운 BGM으로 전환
        PlayBGMImmediate(newBGM);

        // 페이드 인
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, defaultVolume, elapsedTime / fadeDuration);
            yield return null;
        }

        audioSource.volume = defaultVolume;
        fadeCoroutine = null;
    }

    /// <summary>
    /// 즉시 BGM을 재생합니다.
    /// </summary>
    private void PlayBGMImmediate(AudioClip newBGM)
    {
        currentBGM = newBGM;
        audioSource.clip = newBGM;
        
        if (!isMuted)
        {
            audioSource.Play();
        }
    }

    /// <summary>
    /// BGM을 일시정지합니다.
    /// </summary>
    public void PauseBGM()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    /// <summary>
    /// BGM을 재개합니다.
    /// </summary>
    public void ResumeBGM()
    {
        if (!audioSource.isPlaying && !isMuted)
        {
            audioSource.UnPause();
        }
    }

    /// <summary>
    /// BGM을 정지합니다.
    /// </summary>
    public void StopBGM()
    {
        audioSource.Stop();
        currentBGM = null;
    }

    /// <summary>
    /// BGM 음소거를 토글합니다.
    /// </summary>
    public void ToggleMute()
    {
        isMuted = !isMuted;
        
        if (isMuted)
        {
            audioSource.volume = 0f;
        }
        else
        {
            audioSource.volume = defaultVolume;
        }
    }

    /// <summary>
    /// BGM 볼륨을 설정합니다.
    /// </summary>
    /// <param name="volume">볼륨 (0.0 ~ 1.0)</param>
    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        defaultVolume = volume;
        
        if (!isMuted)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// 현재 BGM 볼륨을 반환합니다.
    /// </summary>
    public float GetVolume()
    {
        return defaultVolume;
    }

    /// <summary>
    /// 음소거 상태를 반환합니다.
    /// </summary>
    public bool IsMuted()
    {
        return isMuted;
    }

    /// <summary>
    /// 현재 재생 중인 BGM 타입을 반환합니다.
    /// </summary>
    public BGMType GetCurrentBGMType()
    {
        if (currentBGM == startScreenBGM) return BGMType.StartScreen;
        if (currentBGM == subjectSelectionBGM) return BGMType.SubjectSelection;
        if (currentBGM == quizBGM) return BGMType.Quiz;
        if (currentBGM == endScreenBGM) return BGMType.EndScreen;
        if (currentBGM == loadingBGM) return BGMType.Loading;
        
        return BGMType.StartScreen; // 기본값
    }

    /// <summary>
    /// BGM이 재생 중인지 확인합니다.
    /// </summary>
    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }

    // 디버그용 메서드
    [ContextMenu("Test Start Screen BGM")]
    private void TestStartScreenBGM()
    {
        PlayBGM(BGMType.StartScreen);
    }

    [ContextMenu("Test Subject Selection BGM")]
    private void TestSubjectSelectionBGM()
    {
        PlayBGM(BGMType.SubjectSelection);
    }

    [ContextMenu("Test Quiz BGM")]
    private void TestQuizBGM()
    {
        PlayBGM(BGMType.Quiz);
    }

    [ContextMenu("Test End Screen BGM")]
    private void TestEndScreenBGM()
    {
        PlayBGM(BGMType.EndScreen);
    }

    [ContextMenu("Test Loading BGM")]
    private void TestLoadingBGM()
    {
        PlayBGM(BGMType.Loading);
    }
}
