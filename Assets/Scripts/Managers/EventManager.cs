using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    // Game state properties.
    public bool GameStarted { get; private set; }
    public bool GameOver { get; private set; }
    private bool isRestarting = false;

    // Game state events.
    public event Action OnGameStartEvent;
    public event Action OnGameOverEvent;
    public event Action OnGameRestartEvent;

    private void Awake()
    {
        // Standard singleton pattern.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optionally, uncomment the following line if you want this object to persist across scenes.
        // DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Call this method to start the game.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("EventManager: Game is starting...");
        GameStarted = true;
        GameOver = false;
        isRestarting = false;
        TriggerGameStart();
        // Additional game-start logic can be added here (e.g., scene transitions).
    }

    /// <summary>
    /// Call this method to signal that the game is over.
    /// </summary>
    public void EndGame()
    {
        Debug.Log("EventManager: Game is over!");
        GameOver = true;
        GameStarted = false;
        TriggerGameOver();
        // Additional game-over logic can be added here.
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
        Debug.Log("EventManager: Triggering Game Start event");
        OnGameStartEvent?.Invoke();
    }

    private void TriggerGameOver()
    {
        Debug.Log("EventManager: Triggering Game Over event");
        OnGameOverEvent?.Invoke();
    }

    private void TriggerGameRestart()
    {
        Debug.Log("EventManager: Triggering Game Restart event");
        OnGameRestartEvent?.Invoke();
    }
}
