using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 웹빌드에서 API 호출 실패 시 사용할 대체 문제 시스템
/// </summary>
public class WebFallbackQuestions : MonoBehaviour
{
    [Header("대체 문제 설정")]
    [SerializeField] private bool enableFallbackQuestions = true;

    private Dictionary<string, List<QuestionSO>> fallbackQuestions = new Dictionary<string, List<QuestionSO>>();

    public static WebFallbackQuestions instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFallbackQuestions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFallbackQuestions()
    {
        if (!enableFallbackQuestions) return;

        // 일반상식 문제들
        CreateGeneralKnowledgeQuestions();
        
        // 과학 문제들
        CreateScienceQuestions();
        
        // 수학 문제들
        CreateMathQuestions();
        
        // 역사 문제들
        CreateHistoryQuestions();
    }

    private void CreateGeneralKnowledgeQuestions()
    {
        List<QuestionSO> questions = new List<QuestionSO>();
        
        // 문제 1
        QuestionSO q1 = ScriptableObject.CreateInstance<QuestionSO>();
        q1.setData("태양계에서 가장 큰 행성은?", 
                   new string[] {"지구", "목성", "토성", "화성"}, 
                   1, "태양계의 거대한 가스 행성입니다.");
        questions.Add(q1);

        // 문제 2
        QuestionSO q2 = ScriptableObject.CreateInstance<QuestionSO>();
        q2.setData("한국의 수도는?", 
                   new string[] {"부산", "서울", "대구", "인천"}, 
                   1, "한반도 중앙에 위치한 도시입니다.");
        questions.Add(q2);

        // 문제 3
        QuestionSO q3 = ScriptableObject.CreateInstance<QuestionSO>();
        q3.setData("지구의 위성은?", 
                   new string[] {"화성", "달", "태양", "목성"}, 
                   1, "지구 주위를 도는 천체입니다.");
        questions.Add(q3);

        fallbackQuestions["일반상식"] = questions;
    }

    private void CreateScienceQuestions()
    {
        List<QuestionSO> questions = new List<QuestionSO>();
        
        // 문제 1
        QuestionSO q1 = ScriptableObject.CreateInstance<QuestionSO>();
        q1.setData("물의 화학식은?", 
                   new string[] {"H2O", "CO2", "O2", "H2"}, 
                   0, "수소 2개와 산소 1개로 구성됩니다.");
        questions.Add(q1);

        // 문제 2
        QuestionSO q2 = ScriptableObject.CreateInstance<QuestionSO>();
        q2.setData("중력의 법칙을 발견한 사람은?", 
                   new string[] {"아인슈타인", "뉴턴", "갈릴레이", "코페르니쿠스"}, 
                   1, "사과가 떨어지는 것을 보고 발견했습니다.");
        questions.Add(q2);

        fallbackQuestions["과학"] = questions;
    }

    private void CreateMathQuestions()
    {
        List<QuestionSO> questions = new List<QuestionSO>();
        
        // 문제 1
        QuestionSO q1 = ScriptableObject.CreateInstance<QuestionSO>();
        q1.setData("2 + 2 = ?", 
                   new string[] {"3", "4", "5", "6"}, 
                   1, "기본적인 덧셈입니다.");
        questions.Add(q1);

        // 문제 2
        QuestionSO q2 = ScriptableObject.CreateInstance<QuestionSO>();
        q2.setData("원주율의 값은?", 
                   new string[] {"3.14", "2.71", "1.41", "2.14"}, 
                   0, "원의 둘레를 지름으로 나눈 값입니다.");
        questions.Add(q2);

        fallbackQuestions["수학"] = questions;
    }

    private void CreateHistoryQuestions()
    {
        List<QuestionSO> questions = new List<QuestionSO>();
        
        // 문제 1
        QuestionSO q1 = ScriptableObject.CreateInstance<QuestionSO>();
        q1.setData("조선을 건국한 왕은?", 
                   new string[] {"세종대왕", "태조", "정조", "광해군"}, 
                   1, "이성계가 조선을 건국했습니다.");
        questions.Add(q1);

        // 문제 2
        QuestionSO q2 = ScriptableObject.CreateInstance<QuestionSO>();
        q2.setData("한글을 창제한 왕은?", 
                   new string[] {"세종대왕", "태조", "정조", "광해군"}, 
                   0, "1443년에 훈민정음을 창제했습니다.");
        questions.Add(q2);

        fallbackQuestions["역사"] = questions;
    }

    /// <summary>
    /// 대체 문제를 가져옵니다
    /// </summary>
    public List<QuestionSO> GetFallbackQuestions(string topic, int count)
    {
        if (!fallbackQuestions.ContainsKey(topic))
        {
            // 주제가 없으면 일반상식으로 대체
            topic = "일반상식";
        }

        List<QuestionSO> questions = fallbackQuestions[topic];
        List<QuestionSO> result = new List<QuestionSO>();

        // 요청한 수만큼 문제 반환 (순환)
        for (int i = 0; i < count; i++)
        {
            if (questions.Count > 0)
            {
                int index = i % questions.Count;
                result.Add(questions[index]);
            }
        }

        return result;
    }

    /// <summary>
    /// 대체 문제가 있는지 확인합니다
    /// </summary>
    public bool HasFallbackQuestions(string topic)
    {
        return fallbackQuestions.ContainsKey(topic) && fallbackQuestions[topic].Count > 0;
    }

    /// <summary>
    /// 사용 가능한 주제 목록을 반환합니다
    /// </summary>
    public string[] GetAvailableTopics()
    {
        return new string[] { "일반상식", "과학", "수학", "역사" };
    }
}
