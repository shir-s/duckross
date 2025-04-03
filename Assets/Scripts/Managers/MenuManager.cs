using UnityEngine;

namespace Managers
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance { get; private set; }

        // Reference to the entire Canvas that holds your menu.
        [SerializeField] private GameObject menuCanvas;

        private void Awake()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnMainMenuEvent += TriggerMainMenu;
            }
            // Singleton pattern: if an instance already exists, destroy this one.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Optionally, you can keep the MenuManager between scenes:
            // DontDestroyOnLoad(gameObject);
        }

        private void TriggerMainMenu()
        {
            ReturnToMainMenu();
        }

        // This method will be called when the Start button is clicked.
        public void StartGame()
        {
            if (menuCanvas != null)
            {
                menuCanvas.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Menu Canvas is not assigned!");
            }
            
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StartGame();
            }
            else
            {
                Debug.LogWarning("WorldManager instance not found!");
            }
        }

        // This method will be called when the Quit button is clicked.
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    
        // This method will be called when the player wants to return to the main menu.
        public void ReturnToMainMenu()
        {
            if (menuCanvas != null)
            {
                menuCanvas.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Menu Canvas is not assigned!");
            }
        }
    }
}