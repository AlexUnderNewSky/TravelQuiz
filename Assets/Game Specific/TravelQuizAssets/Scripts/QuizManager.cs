using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using UnityEngine.Serialization;

namespace FM_TravelQuiz
{
    public class QuizManager : MonoBehaviour
    {
		[SerializeField] private QuizUI quizUI;
        [SerializeField] private List<Question> questions;
        private Question[] initialQuestions;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject menuCanvas;
        [SerializeField] private GameObject settingsPanel;

        private int gameScore;
        private Question selectedQuestion;

        void Start()
        {
            initialQuestions = new Question[questions.Count];
            questions.CopyTo(initialQuestions);
            SelectQuestion();
            gameScore = 0;
        }

        void SelectQuestion()
        {
            if (questions.Count > 0)
            {
                int random = Random.Range(0, questions.Count);
                selectedQuestion = questions[random];
                quizUI.SetQuestion(initialQuestions, selectedQuestion);
                questions.RemoveAt(random);
            }
            else
            {
                resultsPanel.SetActive(true);
                menuCanvas.SetActive(true);
                settingsPanel.SetActive(false);
                if (LocalizationManager.CurrentLanguage == "Bulgarian")
                {
                    quizUI.GetFinalScoreText().text = "Точки: " + gameScore;
                }
                else if (LocalizationManager.CurrentLanguage == "English")
                {
                    quizUI.GetFinalScoreText().text = "Points: " + gameScore;
                }
				else if (LocalizationManager.CurrentLanguage == "Russian")
				{
					quizUI.GetFinalScoreText().text = "Очков: " + gameScore;
				}
				else if (LocalizationManager.CurrentLanguage == "Ukrainian")
				{
					quizUI.GetFinalScoreText().text = "Очків: " + gameScore;
				}
            }
        }

        public bool Answer(string answered)
        {
            bool isCorrectAnswer = false;
            if (answered == selectedQuestion.answerEn)
            {
                isCorrectAnswer = true;
                gameScore += 20;
                if (LocalizationManager.CurrentLanguage == "Bulgarian")
                {
                    quizUI.GetScoreText().text = "Точки: " + gameScore;
                }
                else if (LocalizationManager.CurrentLanguage == "English")
                {
                    quizUI.GetScoreText().text = "Points: " + gameScore;
                }
				else if (LocalizationManager.CurrentLanguage == "Russian")
				{
					quizUI.GetFinalScoreText().text = "Очков: " + gameScore;
				}
				else if (LocalizationManager.CurrentLanguage == "Ukrainian")
				{
					quizUI.GetFinalScoreText().text = "Очків: " + gameScore;
				}
            }

            Invoke("SelectQuestion", 0.6F);

            return isCorrectAnswer;
        }

        [System.Serializable]
        public class Question
        {
			[FormerlySerializedAs("answer")] public string answerUk;
			[FormerlySerializedAs("answer")] public string answerRu;
			[FormerlySerializedAs("answer")] public string answerBg;
            public string answerEn;
            public Sprite image;
        }
    }
}