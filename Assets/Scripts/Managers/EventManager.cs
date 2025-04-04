using System;
using Managers;
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
    public event Action OnMainMenuEvent;
    
    // Invoked when the number of chicks following the player is sufficient to pass the finish segment.
    public event Action OnChicksPassedFinishSegment;

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
        // Additional game-start logic can be added here.
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
        if (!isRestarting)
        {
            TriggerMainMenu();
        }
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
    
    private void TriggerMainMenu()
    {
        OnMainMenuEvent?.Invoke();
    }

    // Trigger method for when enough chicks have passed the finish segment.
    public void TriggerPlayerPassedFinishSegment()
    {
        Debug.Log("EventManager: Triggering Chicks Passed Finish Segment event");
        OnChicksPassedFinishSegment?.Invoke();
    }
}
