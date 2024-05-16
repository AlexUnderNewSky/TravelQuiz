using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using UnityEngine.UI;

namespace FM_FruitQuiz
{
    public class QuizUI : MonoBehaviour
    {
        [SerializeField] private QuizManager quizManager;
        [SerializeField] private Image image;
        [SerializeField] private List<Button> options;
        [SerializeField] private Color correctColor, wrongColor, normalColor;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip correctAudio, wrongAudio;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text finalScoreText;

        private QuizManager.Question question;
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

        public void SetQuestion(QuizManager.Question[] allQuestions, QuizManager.Question selectedQuestion)
        {
            question = selectedQuestion;
            List<QuizManager.Question> questionsList = new List<QuizManager.Question>(allQuestions);
            
            questionsList.Remove(selectedQuestion);
            
            image.transform.parent.gameObject.SetActive(true);
            image.transform.gameObject.SetActive(true);
            image.sprite = question.image;

            questionsList = ShuffleList.ShuffleListItems<QuizManager.Question>(questionsList);
            int rightQuestion = Random.Range(0, 4);

            // make sure the right question number does not repeat
            if (previousRightQuestionNum == rightQuestion)
            {
                rightQuestion = (rightQuestion + 1) %  4;
            }

            previousRightQuestionNum = rightQuestion;
            
            for (int i = 0; i < options.Count; i++)
            {
                QuizManager.Question currentQuestion = questionsList[i];
                if (i == rightQuestion)
                {
                    currentQuestion = selectedQuestion;
                }

                if (LocalizationManager.CurrentLanguage == "Bulgarian")
                {
                    options[i].GetComponentInChildren<Text>().text = currentQuestion.answerBg;
                }
                else if (LocalizationManager.CurrentLanguage == "English")
                {
                    options[i].GetComponentInChildren<Text>().text = currentQuestion.answerEn;
                }
                
                options[i].name = currentQuestion.answerEn;
                options[i].image.color = normalColor;
            }
            
            answered = false;
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