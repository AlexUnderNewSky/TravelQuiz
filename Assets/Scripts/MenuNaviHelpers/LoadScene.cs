using UnityEngine;
using UnityEngine.SceneManagement;

namespace FluurMat.GameHelpers
{
    [DefaultExecutionOrder(-30)]
    public class LoadScene : MonoBehaviour
    {
        [Tooltip("Name of scene for LoadScene.")] [SerializeField]
        private string theScene;

        [Tooltip("Name of scene for LoadSecondScene.")] [SerializeField]
        private string theSecondScene;

        [Tooltip("Load those additively when current scene loads this GameObject.")] [SerializeField]
        private string[] loadScenes;

        [Tooltip("Reset time to 1 on scene load.")] [SerializeField]
        private bool resetTimeOnLoad = false;


        public void LoadFirstScene()
        {
            if (theScene != "")
            {
                if (resetTimeOnLoad)
                {
                    Time.timeScale = 1;
                }
                
                SceneManager.LoadScene(theScene);
            }
        }

        public void LoadSecondScene()
        {
            if (theSecondScene != "")
            {
                if (resetTimeOnLoad)
                {
                    Time.timeScale = 1;
                }
                
                SceneManager.LoadScene(theSecondScene);
            }
        }

        void Start()
        {
            if (loadScenes != null && loadScenes.Length > 0)
            {
                for (int i = 0; i < loadScenes.Length; i++)
                {
                    SceneManager.LoadScene(loadScenes[i], LoadSceneMode.Additive);
                }
            }
        }

        public void RestartLevel()
        {
            if (resetTimeOnLoad)
            {
                Time.timeScale = 1;
            }
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadMainMenu()
        {
            if (resetTimeOnLoad)
            {
                Time.timeScale = 1;
            }
            
            SceneManager.LoadScene(GlobalSceneSettings.mainMenuScene);
        }

        public void LoadAppSelection()
        {
            if (resetTimeOnLoad)
            {
                Time.timeScale = 1;
            }
            
            SceneManager.LoadScene(GlobalSceneSettings.appSelectionScene);
        }
    }
}