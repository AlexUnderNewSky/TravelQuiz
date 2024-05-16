using UnityEngine;
using UnityEngine.SceneManagement;

namespace FluurMat.GameHelpers
{
    [DefaultExecutionOrder(-31)]
    public class QuitToMain : MonoBehaviour
    {
        public enum QuitButtonPressBehaviour
        {
            DualPressQuit = 0,
            DualPressMenu,
            TriplePressQuit,
            TriplePressMenu,
            RaiseEvent
        }

        public static QuitToMain instance { get; private set; }

        public enum DefaultSceneToLoad
        {
            MainMenu = 0,
            AppSelection,
        }

        [Tooltip("Key press to listen to for Esc event.")] [SerializeField]
        private KeyCode theKey = KeyCode.Escape;

        [Tooltip("Key press for Menu back button.")] [SerializeField]
        private KeyCode keyMenuBackButton = KeyCode.Backspace;

        [Tooltip("Scene to go to. If empty, will try to go to Default selected below.")] [SerializeField]
        private string theScene = "";

        private bool quitMenuRaised = false;

        [Tooltip("Press once to bring menu, second to bring quit menu, third to quit.")] [SerializeField]
        private QuitButtonPressBehaviour pressQuitBehaviour = QuitButtonPressBehaviour.TriplePressMenu;

        [Tooltip("Default Scene to go to if name above is empty.")] [SerializeField]
        private DefaultSceneToLoad defaultSceneToLoad = DefaultSceneToLoad.AppSelection;

        [Tooltip("Reset time to 1 on scene load.")] [SerializeField]
        private bool resetTimeOnLoad = false;

        private bool confirmKeysDefined = false;
        private KeyCode keyConfirmYes = KeyCode.None;
        private KeyCode keyConfrimNo = KeyCode.None;

        private void Awake()
        {
            if (!instance)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            DefineConfirmKeys();
        }

        void Update()
        {
            if (Input.GetKeyDown(theKey))
            {
                if (MenuWalker.instance && MenuWalker.instance.IsInRestartGameMenu())
                {
                    MenuWalker.instance.GoToPreviousMenu();
                    return;
                }

                if (pressQuitBehaviour == QuitButtonPressBehaviour.TriplePressMenu ||
                    pressQuitBehaviour == QuitButtonPressBehaviour.TriplePressQuit)
                {
                    TriplePressQuit();
                }
                else if (pressQuitBehaviour == QuitButtonPressBehaviour.DualPressMenu ||
                         pressQuitBehaviour == QuitButtonPressBehaviour.DualPressQuit)
                {
                    BringQuitMenu();
                }
                else
                {
                    if (OnPauseRaised != null)
                    {
                        OnPauseRaised();
                    }
                }
            }
            else if (Input.GetKeyDown(keyMenuBackButton))
            {
                // handle this here to avoid yet another Update in the menu behaviour
                if (MenuWalker.instance && MenuWalker.instance.IsInAnyMenu() && !(MenuWalker.instance.IsInMainMenu() ||
                        MenuWalker.instance.IsInAGameOverMenu()))
                {
                    MenuWalker.instance.GoToPreviousMenu();
                    return;
                }
            }
            else if (confirmKeysDefined)
            {
                if (Input.GetKeyDown(keyConfirmYes))
                {
                    if (MenuWalker.instance)
                    {
                        if (MenuWalker.instance.IsInQuitMenu())
                        {
                            ConfirmQuit();
                        }
                        else if (MenuWalker.instance.IsInRestartGameMenu())
                        {
                            RestartLevel();
                        }
                    }
                }
                else if (Input.GetKeyDown(keyConfrimNo) && MenuWalker.instance && (MenuWalker.instance.IsInQuitMenu() ||
                             MenuWalker.instance.IsInRestartGameMenu()))
                {
                    MenuWalker.instance.GoToPreviousMenu();
                }
            }
        }

        private void DefineConfirmKeys()
        {
            var language = I2.Loc.LocalizationManager.CurrentLanguage;

            keyConfirmYes = KeyCode.Y;
            keyConfrimNo = KeyCode.N;

            if (language.ToLower() == "german")
            {
                keyConfirmYes = KeyCode.J;
                keyConfrimNo = KeyCode.N;
            }

            confirmKeysDefined = true;
        }

        private void TriplePressQuit()
        {
            if (MenuWalker.instance)
            {
                if (!MenuWalker.instance.IsInAnyMenu())
                {
                    MenuWalker.instance.ShowInitialMenu();
                    if (OnPauseRaised != null)
                    {
                        OnPauseRaised();
                    }
                }
                else if (!MenuWalker.instance.IsInQuitMenu() &&
                         (MenuWalker.instance.IsInMainMenu() || MenuWalker.instance.IsInAGameOverMenu()))
                {
                    MenuWalker.instance.BringQuitMenu();
                }
                else if (MenuWalker.instance.IsInQuitMenu())
                {
                    if (pressQuitBehaviour == QuitButtonPressBehaviour.TriplePressQuit)
                    {
                        ConfirmQuit();
                    }
                    else if (pressQuitBehaviour == QuitButtonPressBehaviour.TriplePressMenu)
                    {
                        MenuWalker.instance.GoToPreviousMenu();
                    }
                }
            }
            else
            {
                ConfirmQuit();
            }
        }

        public void BringQuitMenu(bool confirm = false)
        {
            if (MenuWalker.instance)
            {
                if (!quitMenuRaised && !confirm)
                {
                    MenuWalker.instance.BringQuitMenu();
                    if (MenuWalker.instance.IsSwitchingMenusActive)
                    {
                        MenuWalker.instance.SetPreviousToInitial();
                    }
                    
                    if (OnPauseRaised != null)
                    {
                        OnPauseRaised();
                    }

                    quitMenuRaised = true;
                }
                else
                {
                    if (pressQuitBehaviour == QuitButtonPressBehaviour.DualPressQuit || confirm)
                    {
                        ConfirmQuit();
                    }
                    else if (pressQuitBehaviour == QuitButtonPressBehaviour.DualPressMenu)
                    {
                        MenuWalker.instance.GoToPreviousMenu();
                    }
                }
            }
            else
            {
                ConfirmQuit();
            }
        }

        public void DenyQuit()
        {
            quitMenuRaised = false;
        }

        public void RestartLevel()
        {
            if (resetTimeOnLoad)
            {
                Time.timeScale = 1;
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ConfirmQuit()
        {
            if (resetTimeOnLoad)
            {
                Time.timeScale = 1;
            }

            if (!string.IsNullOrEmpty(theScene))
            {
                SceneManager.LoadScene(theScene);
            }
            else
            {
                if (defaultSceneToLoad == DefaultSceneToLoad.AppSelection)
                {
                    SceneManager.LoadScene(GlobalSceneSettings.appSelectionScene);
                }
                else
                {
                    SceneManager.LoadScene(GlobalSceneSettings.mainMenuScene);
                }
            }
        }

        public delegate void PauseRaised();

        public event PauseRaised OnPauseRaised;
    }
}