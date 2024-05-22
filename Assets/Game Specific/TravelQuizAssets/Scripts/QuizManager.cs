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
		private List<Question> initialQuestions;
		[SerializeField] private GameObject resultsPanel;
		[SerializeField] private GameObject menuCanvas;
		[SerializeField] private GameObject settingsPanel;

		private int gameScore;
		private Question selectedQuestion;

		void Start()
		{
			initialQuestions = new List<Question>(questions);
			SelectQuestion();
			gameScore = 0;
		}

		void SelectQuestion()
		{
			if (questions.Count > 0)
			{
				int random = Random.Range(0, questions.Count);
				selectedQuestion = questions[random];
				quizUI.SetQuestion(initialQuestions.ToArray(), selectedQuestion);
				questions.RemoveAt(random);
			}
			else
			{
				resultsPanel.SetActive(true);
				menuCanvas.SetActive(true);
				settingsPanel.SetActive(false);
				string scoreText;
				switch (LocalizationManager.CurrentLanguage)
				{
				case "Bulgarian":
					scoreText = "Точки: " + gameScore;
					break;
				case "English":
					scoreText = "Points: " + gameScore;
					break;
				case "Russian":
					scoreText = "Очков: " + gameScore;
					break;
				case "Ukrainian":
					scoreText = "Очків: " + gameScore;
					break;
				default:
					scoreText = "Score: " + gameScore;
					break;
				}
				quizUI.GetFinalScoreText().text = scoreText;
			}
		}

		public bool Answer(string answered)
		{
			bool isCorrectAnswer = false;
			if (answered == selectedQuestion.answerEn)
			{
				isCorrectAnswer = true;
				gameScore += 20;

				string scoreText;
				switch (LocalizationManager.CurrentLanguage)
				{
				case "Bulgarian":
					scoreText = "Точки: " + gameScore;
					break;
				case "English":
					scoreText = "Points: " + gameScore;
					break;
				case "Russian":
					scoreText = "Очков: " + gameScore;
					break;
				case "Ukrainian":
					scoreText = "Очків: " + gameScore;
					break;
				default:
					scoreText = "Score: " + gameScore;
					break;
				}
				quizUI.GetScoreText().text = scoreText;
			}

			Invoke("SelectQuestion", 0.6F);

			return isCorrectAnswer;
		}

		[System.Serializable]
		public class Question
		{
			public string answerEn;
			public string answerBg;
			public string answerUk;
			public string answerRu;
			public Sprite image;
		}
	}
}
