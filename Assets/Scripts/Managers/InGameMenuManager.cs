using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class InGameMenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject inGameMenuCanvas; // The canvas containing the in-game menu UI.
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button backButton;

        private bool inMainMenu = true;

        private void Awake()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnMainMenuEvent += TriggerMainMenu;
                EventManager.Instance.OnGameStartEvent += TriggerGameStart;
            }

            // Set up button listeners.
            if (mainMenuButton) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            if (restartButton) restartButton.onClick.AddListener(RestartGame);
            if (backButton) backButton.onClick.AddListener(HideMenu);
            HideMenu();
        }

        // Returns to the main menu scene.
        private void ReturnToMainMenu()
        {
            EventManager.Instance.EndGame();
        }
        
        private void TriggerGameStart()
        {
            inMainMenu = false;
        }

        private void TriggerMainMenu()
        {
            inMainMenu = true;
            HideMenu();
        }

        private void Update()
        {
            // Toggle the in-game menu when the Escape key is pressed.
            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && !inMainMenu)
            {
                if (inGameMenuCanvas.activeSelf)
                    HideMenu();
                else
                    ShowMenu();
            }
        }

        // Call this to show the in-game menu.
        public void ShowMenu()
        {
            if (inGameMenuCanvas != null)
            {
                inGameMenuCanvas.SetActive(true);
                // Optionally, apply additional blur effects to the background here.
            }
        }

        // Hide the in-game menu and resume the game.
        public void HideMenu()
        {
            if (inGameMenuCanvas != null)
            {
                inGameMenuCanvas.SetActive(false);
            }
        }

        // Restarts the game by calling a method on your WorldManager.
        private void RestartGame()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.RestartGame();
            }
            HideMenu();
        }
        private void OnDisable()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnGameStartEvent -= TriggerGameStart;
                EventManager.Instance.OnMainMenuEvent -= TriggerMainMenu;
            }
        }
    }
}