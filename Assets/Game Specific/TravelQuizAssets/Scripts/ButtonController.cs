using UnityEngine;

namespace FM_TravelQuiz
{
    public class ButtonController : MonoBehaviour
    {
        [SerializeField] public QuizManager quizManager;

        // Метод, вызываемый при нажатии кнопки
        public void OnButtonClick()
        {
            // Перезагрузка QuizManager при нажатии кнопки
            quizManager.ReloadQuizManager();
        }
    }
}