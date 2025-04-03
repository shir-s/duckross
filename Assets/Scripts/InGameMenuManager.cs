using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject inGameMenuCanvas; // The canvas containing the in-game menu UI.
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        // Set up button listeners.
        if (mainMenuButton) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (restartButton) restartButton.onClick.AddListener(RestartGame);
        if (backButton) backButton.onClick.AddListener(HideMenu);
        HideMenu();
    }

    private void Update()
    {
        // Toggle the in-game menu when the Escape key is pressed.
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
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

    // Returns to the main menu scene.
    private void ReturnToMainMenu()
    {
        WorldManager.Instance.EndGame();
        HideMenu();
    }

    // Restarts the game by calling a method on your WorldManager.
    private void RestartGame()
    {
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.RestartGame();
        }
        HideMenu();
    }
}