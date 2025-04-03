using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }

    public bool GameStarted { get; private set; }
    public bool GameOver { get; private set; }

    // List of all registered listeners.
    private List<IGameStateListener> gameStateListeners = new List<IGameStateListener>();
    
    private bool isRestarting = false;

    private void Awake()
    {
        // Ensure only one instance exists.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optionally, keep this object between scenes.
        // DontDestroyOnLoad(gameObject);
    }

    // Register a new listener.
    public void RegisterListener(IGameStateListener listener)
    {
        Debug.Log("RegisterListener: " + listener.GetType().Name);
        if (!gameStateListeners.Contains(listener))
        {
            gameStateListeners.Add(listener);
        }
    }

    // Unregister a listener.
    public void UnregisterListener(IGameStateListener listener)
    {
        if (gameStateListeners.Contains(listener))
        {
            gameStateListeners.Remove(listener);
        }
    }

    // Called by the main menu button.
    public void StartGame()
    {
        Debug.Log("Game is starting...");
        GameStarted = true;
        GameOver = false;
        isRestarting = false;
        NotifyGameStart();
        // Additional logic (e.g., loading scenes, hiding menus) can be added here.
    }

    // Called when the game should end (for example, by the player).
    public void EndGame()
    {
        Debug.Log("Game is over!");
        GameOver = true;
        GameStarted = false;
        NotifyGameOver();
        // Additional logic for game over state can be added here.
        if (!isRestarting)
        {
            MenuManager.Instance.ReturnToMainMenu();
        }
    }

    // Example of a restart method.
    public void RestartGame()
    {
        isRestarting = true;
        // Restart logic here: e.g., reload the current scene.
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        EndGame();
        StartGame();
    }

    // Notifies all listeners that the game has started.
    private void NotifyGameStart()
    {
        Debug.Log(gameStateListeners.Count.ToString());
        foreach (IGameStateListener listener in gameStateListeners)
        {
            Debug.Log(listener.GetType().Name);
            listener.OnGameStart();
        }
    }

    // Notifies all listeners that the game is over.
    private void NotifyGameOver()
    {
        foreach (IGameStateListener listener in gameStateListeners)
        {
            listener.OnGameOver();
        }
    }
}
