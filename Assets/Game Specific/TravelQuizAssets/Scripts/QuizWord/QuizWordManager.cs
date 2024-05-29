using System.Collections.Generic;
using System.IO;
using UnityEngine;
using I2.Loc;

namespace FM_TravelQuiz
{
    public class QuizWordManager : MonoBehaviour
    {
        [SerializeField] private QuizWordUI quizWordUI;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject menuCanvas;
        [SerializeField] private GameObject settingsPanel;

        private List<WordQuestion> questions;
        private List<WordQuestion> initialQuestions;
        private int gameScore;
        private WordQuestion selectedQuestion;

        void Start()
        {
            string questionsPath = Path.Combine(Application.dataPath, "Game Specific/TravelQuizAssets/Scripts/questionsWord.json");
            LoadQuestionsFromJSON(questionsPath);
            initialQuestions = new List<WordQuestion>(questions);
            SelectQuestion();
            gameScore = 0;
        }

        void LoadQuestionsFromJSON(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("JSON file not found: " + filePath);
                return;
            }

            string json = File.ReadAllText(filePath);
            Debug.Log("Loaded JSON: " + json); // Debugging line to check JSON content

            ContinentData continentData = JsonUtility.FromJson<ContinentData>(json);
            if (continentData == null)
            {
                Debug.LogError("Failed to deserialize JSON");
                return;
            }

            string activeContinent = GetActiveContinent();
            Debug.Log("Active Continent: " + activeContinent);

            questions = continentData.GetQuestionsForContinent(activeContinent);
            if (questions == null || questions.Count == 0)
            {
                Debug.LogWarning("No questions found for the continent: " + activeContinent);
            }
        }

        string GetActiveContinent()
        {
            GameObject asiaGameObject = GameObject.Find("Canvas_game_WordAsia");
            if (asiaGameObject != null && asiaGameObject.activeSelf)
                return "Word_asia";

            GameObject europeGameObject = GameObject.Find("Canvas_game_WordEurope");
            if (europeGameObject != null && europeGameObject.activeSelf)
                return "Word_europe";

            GameObject africaGameObject = GameObject.Find("Canvas_game_WordAfrica");
            if (africaGameObject != null && africaGameObject.activeSelf)
                return "Word_africa";

            GameObject southAmericaGameObject = GameObject.Find("Canvas_game_WordSouthAmerica");
            if (southAmericaGameObject != null && southAmericaGameObject.activeSelf)
                return "Word_southAmerica";

            GameObject northAmericaGameObject = GameObject.Find("Canvas_game_WordNorthAmerica");
            if (northAmericaGameObject != null && northAmericaGameObject.activeSelf)
                return "Word_northAmerica";

            GameObject australiaGameObject = GameObject.Find("Canvas_game_WordAustralia&Oceania");
            if (australiaGameObject != null && australiaGameObject.activeSelf)
                return "Word_australia";

            else return "questions";
        }


        [System.Serializable]
        public class ContinentData
        {
            public List<QuestionWordData> Word_europe;
            public List<QuestionWordData> Word_asia;
            public List<QuestionWordData> Word_africa;
            public List<QuestionWordData> Word_southAmerica;
            public List<QuestionWordData> Word_northAmerica;
            public List<QuestionWordData> Word_australia;
            public List<QuestionWordData> Word_questions;

            public List<WordQuestion> GetQuestionsForContinent(string continent)
            {
                switch (continent)
                {
                    case "Word_asia": return ConvertToQuestionList(Word_asia);
                    case "Word_europe": return ConvertToQuestionList(Word_europe);
                    case "Word_africa": return ConvertToQuestionList(Word_africa);
                    case "Word_southAmerica": return ConvertToQuestionList(Word_southAmerica);
                    case "Word_northAmerica": return ConvertToQuestionList(Word_northAmerica);
                    case "Word_australia": return ConvertToQuestionList(Word_australia);
                    default: return ConvertToQuestionList(Word_questions);
                }
            }

            List<WordQuestion> ConvertToQuestionList(List<QuestionWordData> questionWordDataList)
            {
                if (questionWordDataList == null)
                {
                    return new List<WordQuestion>();
                }

                List<WordQuestion> result = new List<WordQuestion>();
                foreach (var questionWordData in questionWordDataList)
                {
                    WordQuestion question = new WordQuestion
                    {
                        questionText = questionWordData.questionText,
                        correctAnswer = questionWordData.correctAnswer,
                        wrongAnswers = questionWordData.wrongAnswers
                    };
                    result.Add(question);
                }
                return result;
            }
        }

        [System.Serializable]
        public class QuestionWordData
        {
            public string questionText;
            public string correctAnswer;
            public List<string> wrongAnswers;
        }

        [System.Serializable]
        public class WordQuestion
        {
            public string questionText;
            public string correctAnswer;
            public List<string> wrongAnswers;
        }

        void SelectQuestion()
        {
            if (questions.Count > 0)
            {
                int random = Random.Range(0, questions.Count);
                selectedQuestion = questions[random];
                quizWordUI.SetQuestion(initialQuestions.ToArray(), selectedQuestion);
                questions.RemoveAt(random);
            }
            else
            {
                resultsPanel.SetActive(true);
                menuCanvas.SetActive(true);
                settingsPanel.SetActive(false);
                string scoreText = GetScoreTextBasedOnLanguage(gameScore);
                quizWordUI.GetFinalScoreText().text = scoreText;
            }
        }

        string GetScoreTextBasedOnLanguage(int score)
        {
            switch (LocalizationManager.CurrentLanguage)
            {
                case "Bulgarian": return "Точки: " + score;
                case "English": return "Points: " + score;
                case "Russian": return "Очков: " + score;
                case "Ukrainian": return "Очків: " + score;
                default: return "Score: " + score;
            }
        }

        public void ReloadQuizManager()
        {
            string questionsPath = Path.Combine(Application.dataPath, "Game Specific/TravelQuizAssets/Scripts/questionsWord.json");
            LoadQuestionsFromJSON(questionsPath);
            initialQuestions = new List<WordQuestion>(questions);
            SelectQuestion();
            gameScore = 0;
        }

        public bool Answer(string answered)
        {
            bool isCorrectAnswer = false;
            if (answered == selectedQuestion.correctAnswer)
            {
                isCorrectAnswer = true;
                gameScore += 20;
                string scoreText = GetScoreTextBasedOnLanguage(gameScore);
                quizWordUI.GetScoreText().text = scoreText;
            }

            Invoke("SelectQuestion", 0.6F);

            return isCorrectAnswer;
        }
    }
}
