using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using UnityEngine.UI;

namespace FM_TravelQuiz
{
    public class QuizWordUI : MonoBehaviour
    {
        [SerializeField] private QuizWordManager quizManager;
        [SerializeField] private Text questionText;
        [SerializeField] private List<Button> options;
        [SerializeField] private Color correctColor, wrongColor, normalColor;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip correctAudio, wrongAudio;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text finalScoreText;

        private QuizWordManager.WordQuestion question;
        private bool answered;
        private int previousRightQuestionNum = -1;

        public Text GetScoreText()
        {
            return scoreText;
        }

        public Text GetFinalScoreText()
        {
            return finalScoreText;
        }

        void Awake()
        {
            for (int i = 0; i < options.Count; i++)
            {
                Button localButton = options[i];
                localButton.onClick.AddListener(() => OnClick(localButton));
            }
        }

        public void SetQuestion(QuizWordManager.WordQuestion[] allQuestions, QuizWordManager.WordQuestion selectedQuestion)
        {
            question = selectedQuestion;

            // Устанавливаем текст вопроса
            questionText.text = question.questionText;

            // Создаем список из всех вариантов ответов, включая правильный и неправильные ответы
            List<string> allAnswers = new List<string>();
            allAnswers.Add(selectedQuestion.correctAnswer);
            allAnswers.AddRange(selectedQuestion.wrongAnswers);

            // Перемешиваем варианты ответов
            allAnswers = ShuffleList.ShuffleListItems<string>(allAnswers);

            // Проходим по всем кнопкам и устанавливаем для них текст ответов
            for (int i = 0; i < options.Count; i++)
            {
                options[i].GetComponentInChildren<Text>().text = allAnswers[i];
                options[i].name = allAnswers[i]; // Имя кнопки равно тексту ответа
                options[i].image.color = normalColor;
            }

            answered = false;
        }


        private string GetWrongAnswer(List<QuizWordManager.WordQuestion> questionsList)
        {
            // Выбираем случайный неправильный ответ из списка вопросов
            int randomIndex = Random.Range(0, questionsList.Count);
            QuizWordManager.WordQuestion randomQuestion = questionsList[randomIndex];

            // Удаляем выбранный вопрос из списка, чтобы он не повторялся
            questionsList.RemoveAt(randomIndex);

            // Возвращаем неправильный ответ из выбранного вопроса
            return randomQuestion.wrongAnswers[Random.Range(0, randomQuestion.wrongAnswers.Count)];
        }


        public void OnClick(Button clickedButton)
        {
            if (!answered)
            {
                answered = true;
                bool val = quizManager.Answer(clickedButton.name);

                if (val)
                {
                    clickedButton.image.color = correctColor;
                    audioSource.PlayOneShot(correctAudio, 0.6F);
                }
                else
                {
                    clickedButton.image.color = wrongColor;
                    audioSource.PlayOneShot(wrongAudio);
                }
            }
        }
    }
}
