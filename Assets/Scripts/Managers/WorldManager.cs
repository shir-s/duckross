using UnityEngine;

namespace Managers
{
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager Instance { get; private set; }

        public bool GameStarted { get; private set; }
        public bool GameOver { get; private set; }

        private bool isRestarting = false;

        private void Awake()
        {
            // Standard singleton pattern.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Optionally: DontDestroyOnLoad(gameObject);
        }

        // Called by the main menu button or elsewhere to start the game.
        public void StartGame()
        {
            Debug.Log("WorldManager: Game is starting...");
            GameStarted = true;
            GameOver = false;
            isRestarting = false;

            // Notify all interested parties via the EventManager.
            if (EventManager.Instance != null)
            {
                /*EventManager.Instance.TriggerGameStart();*/
            }
            else
            {
                Debug.LogWarning("EventManager instance not found!");
            }

            // Additional game-start logic can go here.
        }

        // Called when the game should end (for example, by the player).
        public void EndGame()
        {
            Debug.Log("WorldManager: Game is over!");
            GameOver = true;
            GameStarted = false;

            if (EventManager.Instance != null)
            {
                /*EventManager.Instance.TriggerGameOver();*/
            }
            else
            {
                Debug.LogWarning("EventManager instance not found!");
            }

            // If not restarting, return to main menu (using your MenuManager, for example).
            if (!isRestarting && MenuManager.Instance != null)
            {
                MenuManager.Instance.ReturnToMainMenu();
            }
        }

        // Example of a restart method.
        public void RestartGame()
        {
            isRestarting = true;
            // For a restart, we first end the game, then start it again.
            EndGame();
            StartGame();

            if (EventManager.Instance != null)
            {
                /*EventManager.Instance.TriggerGameRestart();*/
            }
        }

        public void RegisterListener(InfiniteWorldGenerator infiniteWorldGenerator)
        {
            throw new System.NotImplementedException();
        }

        public void UnregisterListener(InfiniteWorldGenerator infiniteWorldGenerator)
        {
            throw new System.NotImplementedException();
        }
    }
}
