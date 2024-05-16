using UnityEngine;

namespace FluurMat.GameHelpers
{
    [DefaultExecutionOrder(-49)]
    public class MenuWalker : MonoBehaviour
    {
        public static MenuWalker instance { get; private set; }

        [Tooltip("GameObject menu items.")] [SerializeField]
        private GameObject[] menus;

        [Tooltip("Auto SetActive the gameObject if it is not.")] [SerializeField]
        private bool autoPop = false;

        [Tooltip("In-game main menu.")] [SerializeField]
        private GameObject startMenu;

        [Tooltip("Quit menu.")] [SerializeField]
        private GameObject quitMenu;

        [Tooltip("Restart Geame menu.")] [SerializeField]
        private GameObject restartGameMenu;

        [Tooltip("Various GameOver menus.")] [SerializeField]
        private GameObject[] gameOverMenus;

        [Tooltip("Prevent going to MainMenu when in a GameOver menu.")] [SerializeField]
        private bool preventMainMenuWhenGameOver = true;

        private GameObject prevMenu;
        private GameObject currentMenu;
        private bool switchingMenusActive = true;

        public bool IsSwitchingMenusActive
        {
            get { return switchingMenusActive; }
        }


        void Awake()
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

        void Start()
        {
            ShowInitialMenu();
        }

        public void ShowInitialMenu()
        {
            if (!startMenu)
                return;

            GoToMenu(startMenu);
        }

        public void GoToPreviousMenu()
        {
            GoToMenu(prevMenu);
        }

        public void GoToMenu(GameObject which)
        {
            if (!which)
                return;

            if (preventMainMenuWhenGameOver && gameObject.activeSelf && which == startMenu && IsInAGameOverMenu())
                return;

            if (autoPop && !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i].activeSelf)
                {
                    menus[i].SetActive(false);
                    prevMenu = menus[i];
                }
            }

            if (!which.activeSelf)
            {
                which.SetActive(true);
                currentMenu = which;
            }
        }

        public void FreezeMenus()
        {
            switchingMenusActive = false;
        }

        public void UnfreezeMenus()
        {
            switchingMenusActive = true;
        }

        public void SetFreezeMenusTo(bool toWhat)
        {
            switchingMenusActive = toWhat;
        }

        public void BringQuitMenu()
        {
            if (!quitMenu)
                return;

            if (restartGameMenu && IsInRestartGameMenu())
                return;

            GoToMenu(quitMenu);
        }

        public void SetPreviousToInitial()
        {
            if (!startMenu)
                return;

            prevMenu = startMenu;
        }

        public bool IsInAnyMenu()
        {
            int activeMenus = 0;
            for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i].activeInHierarchy)
                {
                    activeMenus++;
                    break;
                }
            }

            return activeMenus > 0;
        }

        public bool IsInMainMenu()
        {
            int activeMenus = 0;
            for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i].activeSelf)
                {
                    activeMenus++;
                    currentMenu = menus[i];
                    break;
                }
            }

            if (activeMenus == 0)
                return false;
            
            return (currentMenu == startMenu);
        }

        public bool IsInQuitMenu()
        {
            int activeMenus = 0;
            for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i].activeSelf)
                {
                    activeMenus++;
                    currentMenu = menus[i];
                    break;
                }
            }

            if (activeMenus == 0)
                return false;
            
            return (currentMenu == quitMenu);
        }

        public bool IsInRestartGameMenu()
        {
            int activeMenus = 0;
            for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i].activeSelf)
                {
                    activeMenus++;
                    currentMenu = menus[i];
                    break;
                }
            }

            if (activeMenus == 0)
                return false;
            
            return (currentMenu == restartGameMenu);
        }

        public bool IsInAGameOverMenu()
        {
            if (gameOverMenus == null || gameOverMenus.Length == 0)
                return false;

            bool isInAGameOverMenu = false;
            int activeMenus = 0;
            for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i].activeSelf)
                {
                    activeMenus++;
                    for (int j = 0; j < gameOverMenus.Length; j++)
                    {
                        if (gameOverMenus[j] == menus[i])
                        {
                            isInAGameOverMenu = true;
                            break;
                        }
                    }
                }
            }

            if (activeMenus == 0)
                return false;
            
            return isInAGameOverMenu;
        }

        public void HideAllMenus()
        {
            for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i].activeSelf)
                {
                    menus[i].SetActive(false);
                }
            }
        }
    }
}