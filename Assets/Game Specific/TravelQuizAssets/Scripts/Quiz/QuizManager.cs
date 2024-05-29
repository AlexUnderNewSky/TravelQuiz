using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using I2.Loc;

namespace FM_TravelQuiz
{
    public class QuizManager : MonoBehaviour
    {
        [SerializeField] private QuizUI quizUI;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject menuCanvas;
        [SerializeField] private GameObject settingsPanel;

        private List<Question> questions;
        private List<Question> initialQuestions;
        private int gameScore;
        private Question selectedQuestion;

        void Start()
        {
            string questionsPath = Path.Combine(Application.dataPath, "Game Specific/TravelQuizAssets/Scripts/questions.json");
            LoadQuestionsFromJSON(questionsPath);
            initialQuestions = new List<Question>(questions);
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

            // Определите, какой континент активен, используя название активного окна Canvas
            string activeContinent = ""; // Здесь будет храниться название активного континента
            GameObject asiaGameObject = GameObject.Find("Canvas_game_Asia");
            GameObject europeGameObject = GameObject.Find("Canvas_game_Europe");
            GameObject africaGameObject = GameObject.Find("Canvas_game_Africa");
            GameObject southAmericaGameObject = GameObject.Find("Canvas_game_SouthAmerica");
            GameObject northAmericaGameObject = GameObject.Find("Canvas_game_NorthAmerica");
            GameObject australiaGameObject = GameObject.Find("Canvas_game_Australia&Oceania");

            if (asiaGameObject != null && asiaGameObject.activeSelf)
            {
                activeContinent = "asia";
            }
            else if (europeGameObject != null && europeGameObject.activeSelf)
            {
                activeContinent = "europe";
            }
            else if (africaGameObject != null && africaGameObject.activeSelf)
            {
                activeContinent = "africa";
            }
            else if (southAmericaGameObject != null && southAmericaGameObject.activeSelf)
            {
                activeContinent = "southAmerica";
            }
            else if (northAmericaGameObject != null && northAmericaGameObject.activeSelf)
            {
                activeContinent = "northAmerica";
            }
            else if (australiaGameObject != null && australiaGameObject.activeSelf)
            {
                activeContinent = "australia";
            }
            else
            {
                activeContinent = "questions";
            }

            // Проверка, что activeContinent корректно установлен
            Debug.Log("Active Continent: " + activeContinent);

            // Другие проверки для других континентов, если необходимо

            questions = continentData.GetQuestionsForContinent(activeContinent);
            if (questions == null || questions.Count == 0)
            {
                Debug.LogWarning("No questions found for the continent: " + activeContinent);
            }
        }

        [System.Serializable]
        public class ContinentData
        {
            public List<QuestionData> europe;
            public List<QuestionData> asia;
            public List<QuestionData> africa;
            public List<QuestionData> southAmerica;
            public List<QuestionData> northAmerica;
            public List<QuestionData> australia;
            public List<QuestionData> questions;

            public List<Question> GetQuestionsForContinent(string continent)
            {
                switch (continent)
                {
                    case "asia":
                        return ConvertToQuestionList(asia);
                    case "europe":
                        return ConvertToQuestionList(europe);
                    case "africa":
                        return ConvertToQuestionList(africa);
                    case "southAmerica":
                        return ConvertToQuestionList(southAmerica);
                    case "northAmerica":
                        return ConvertToQuestionList(northAmerica);
                    case "australia":
                        return ConvertToQuestionList(australia);
                    default:
                        return ConvertToQuestionList(questions);
                }
            }

            List<Question> ConvertToQuestionList(List<QuestionData> questionDataList)
            {
                if (questionDataList == null)
                {
                    return new List<Question>();
                }

                List<Question> result = new List<Question>();
                foreach (var questionData in questionDataList)
                {
                    Question question = new Question
                    {
                        answerEn = questionData.answerEn,
                        answerBg = questionData.answerBg,
                        answerUk = questionData.answerUk,
                        answerRu = questionData.answerRu,
                        image = LoadSprite(questionData.imagePath)
                    };
                    result.Add(question);
                }
                return result;
            }

            Sprite LoadSprite(string imagePath)
            {
                // Получение абсолютного пути к изображению
                string absolutePath = Path.Combine(Application.dataPath, "Game Specific/TravelQuizAssets/Flags/" + imagePath);

                if (!File.Exists(absolutePath))
                {
                    Debug.LogError("Image file not found at path: " + absolutePath);
                    return null;
                }

                byte[] fileData = File.ReadAllBytes(absolutePath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
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

        public void ReloadQuizManager()
        {
            // Путь к вашему JSON-файлу с вопросами
            string questionsPath = Path.Combine(Application.dataPath, "Game Specific/TravelQuizAssets/Scripts/questions.json");

            // Загрузка вопросов из JSON в зависимости от активной панели
            LoadQuestionsFromJSON(questionsPath);

            // Остальная инициализация
            initialQuestions = new List<Question>(questions);
            SelectQuestion();
            gameScore = 0;
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

        [System.Serializable]
        public class QuestionDataArray
        {
            public QuestionData[] questions;
        }
    }
}
