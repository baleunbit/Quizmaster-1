//using System.Collections;
//using UnityEngine;
//using System.Collections.Generic;
//using System;
//using static ChatGPTClint;
//using NUnit.Framework;


//public class ChatGPTClint : MonoBehaviour
//{
//    public delegate void QuizGeneratedHandler(List<QuestionSO> questions);
//    public event QuizGeneratedHandler quizGenerateHandler;

//    public void GenerateQuestions(int questionCount, string topiToUse)
//    {
//        StartCoroutine(GenerateWithDelay());
//    }
//    private IEnumerator GenerateWithDelay()
//    {
//        yield return new WaitForSeconds(3f);
//        List<QuestionSO> questions = new List<QuestionSO>();
//        QuestionSO so1 = CreateQuestion("GPT생성질문 1",
//            new string[] { "답변1(정답)", "답변2", "답변3", "답변4" }, 0);
//        questions.Add(so1);
//        QuestionSO so2 = CreateQuestion("GPT생성질문 2",
//        new string[] { "답변1", "답변2(정답)", "답변3", "답변4" }, 1);
//        questions.Add(so2);
//        QuestionSO so3 = CreateQuestion("GPT생성질문 3",
//            new string[] { "답변1", "답변2", "답변3(정답)", "답변4" }, 2);
//        questions.Add(so3);

//        quizGenerateHandler?.Invoke(questions);
//        Debug.Log("GenerateQuizQuestions Work Done");
//    }
//    QuestionSO CreateQuestion(string q, string[] a, int correctindex)
//    {
//        QuestionSO so = ScriptableObject.CreateInstance<QuestionSO>();
//        so.setData(q, a, correctindex);

//        return so;
//    }
//}
