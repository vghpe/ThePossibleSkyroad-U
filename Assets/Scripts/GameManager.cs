using UnityEngine;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Death,
    Victory
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState CurrentState { get; private set; }

    private int attempts;

    // References to other managers / scripts
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayerController playerController;

    private void Awake()
    {
        // Singleton pattern (optional)
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        // Start in MainMenu for demonstration
        SetGameState(GameState.MainMenu);

        // For testing, we can show attempts in the console
        attempts = 0;
    }

    public void StartGame()
    {
        // Called from UI button or debug call
        attempts = 0; 
        // If we want to reset attempts whenever we start
        // or keep attempts across multiple runs, adjust as needed
        SetGameState(GameState.Playing);
        Debug.Log("Game Started");
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
            Time.timeScale = 0f;
            Debug.Log("Game Paused");
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
            Time.timeScale = 1f;
            Debug.Log("Game Resumed");
        }
    }

    public void OnPlayerDeath()
    {
        // Called by PlayerController or collision script
        attempts++;
        Debug.Log($"Player Died - Attempts: {attempts}");
        SetGameState(GameState.Death);

        // Reset logic, then return to Playing state
        ResetLevel();
        // Or do a short wait, or a fadeout if needed
        SetGameState(GameState.Playing);
    }

    public void OnLevelComplete()
    {
        Debug.Log("Level Complete!");
        SetGameState(GameState.Victory);
        // Could show a victory UI, etc.
    }

    public void ReturnToMainMenu()
    {
        // Called by UI
        SetGameState(GameState.MainMenu);
        Debug.Log("Returned to Main Menu");
    }

    private void SetGameState(GameState newState)
    {
        CurrentState = newState;

        // Notify UI or do other transitions
        if (uiManager != null)
        {
            uiManager.UpdateUIState(newState, attempts);
        }
    }

    private void ResetLevel()
    {
        // Reset the player position
        if (playerController != null)
        {
            playerController.ResetPlayer();
        }

        // Reset the camera bounds
        if (Camera.main != null)
        {
            SimpleThirdPersonCamera cameraController = Camera.main.GetComponent<SimpleThirdPersonCamera>();
            if (cameraController != null)
            {
                cameraController.ResetVerticalBounds();
            }
        }

        // Destroy all active death particles
        GameObject[] deathParticles = GameObject.FindGameObjectsWithTag("DeathParticle"); // Ensure the death particles are tagged properly
        foreach (GameObject particle in deathParticles)
        {
            Destroy(particle);
        }

        // Add additional reset logic for other environment elements, if necessary.
    }


}
