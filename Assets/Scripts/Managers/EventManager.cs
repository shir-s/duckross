using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace Managers
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        // Game state properties.
        public bool GameStarted { get; private set; }
        public bool GameOver { get; private set; }
        private bool isRestarting = false;
        public int curentScore = 0;
        private int highScore = 0;

        // Game state events.
        public event Action OnGameStartEvent;
        public event Action OnGameOverEvent;
        public event Action OnGameRestartEvent;
        public event Action OnMainMenuEvent;
        public event Action OnGameOverByDeathEvent;
    
        // Invoked when enough chicks have passed the finish segment.
        public event Action<int> OnChicksPassedFinishSegment;

        // UI elements for displaying event messages.
        [SerializeField] private Canvas eventCanvas; // Canvas that holds event messages (assign in Inspector)
        [SerializeField] private TextMeshProUGUI eventText; // Text element on the canvas

        // NEW: Additional text that is always displayed on top.
        [SerializeField] private TextMeshProUGUI scoreText; // Assign in Inspector.
        [SerializeField] private TextMeshProUGUI highScoreText; // Assign in Inspector.

    
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Optionally: DontDestroyOnLoad(gameObject);

            // Ensure the event canvas is initially hidden.
            if (eventCanvas != null)
                eventCanvas.enabled = true;
            
            if (eventText != null)
            {
                eventText.text = "";
            }

            highScore = 0;
        }

        /// <summary>
        /// Call this method to start the game.
        /// </summary>
        public void StartGame()
        {
            curentScore = 0;
            GameStarted = true;
            GameOver = false;
            isRestarting = false;
            TriggerGameStart();
            // Additional game-start logic can be added here.
            if (scoreText != null)
            {
                scoreText.text = "Score: 0";
            }
            if (highScoreText != null)
            {
                highScoreText.text = "High Score: " + highScore.ToString();
            }
        }

        /// <summary>
        /// Call this method to signal that the game is over.
        /// </summary>
        public void EndGame()
        {
            Debug.Log("Game Over");
            GameOver = true;
            GameStarted = false;
            TriggerGameOver();

            // Start coroutine to delay further actions.
            if (!isRestarting)
            {
                StartCoroutine(DelayMainMenu(3f)); // Wait 3 seconds (adjust as needed)
            }
        }

        private IEnumerator DelayMainMenu(float delay)
        {
            yield return new WaitForSeconds(delay);
            TriggerMainMenu();
        }

        /// <summary>
        /// Call this method to restart the game.
        /// </summary>
        public void RestartGame()
        {
            isRestarting = true;
            EndGame();
            StartGame();
            TriggerGameRestart();
        }

        private void TriggerGameStart()
        {
            OnGameStartEvent?.Invoke();
        }

        private void TriggerGameOver()
        {
            OnGameOverEvent?.Invoke();
        }

        private void TriggerGameRestart()
        {
            OnGameRestartEvent?.Invoke();
        }
    
        private void TriggerMainMenu()
        {
            OnMainMenuEvent?.Invoke();
        }

        // Trigger method for when enough chicks have passed the finish segment.
        public void TriggerPlayerPassedFinishSegment(int requiredChicks)
        {
            SetScore(requiredChicks);
            OnChicksPassedFinishSegment?.Invoke(requiredChicks);
        }

        public void triggerGameOverByDeath()
        {
            OnGameOverByDeathEvent?.Invoke();
            ShowEventMessage("Game Over");
            StartCoroutine(DelayEndGame(3));
        }
        
        private IEnumerator DelayEndGame(float delay)
        {
            yield return new WaitForSeconds(delay);
            EndGame();
        }
        
        

        // Utility methods for showing/hiding event messages.
        public void ShowEventMessage(string message)
        {
            if (eventText != null)
            {
                eventText.text = message;
            }
        }

        public void HideEventMessage()
        {
            if (eventCanvas != null)
            {
                eventCanvas.enabled = false;
            }
        }

        // NEW: Utility method to set the persistent message.
        public void SetScore(int score)
        {
            curentScore += score;
            if (scoreText != null)
            {
                scoreText.text = "Score: " + curentScore.ToString();
            }
            if (highScoreText != null && (highScore < curentScore))
            {
                highScore = curentScore;
                highScoreText.text = "High Score: " + highScore.ToString();
            }
        }
    }
}

