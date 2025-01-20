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

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;  // Reference to an AudioSource (should be on this GameObject or assigned)
    [SerializeField] private AudioClip songClip;         // The main music track for the level

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
        // Set state to Playing and start the track from the beginning.
        SetGameState(GameState.Playing);
        Debug.Log("Game Started");

        if (audioSource != null && songClip != null)
        {
            audioSource.clip = songClip;
            audioSource.Play();
        }
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

        // Reset level (this will also restart the music track)
        ResetLevel();

        // Transition back to playing state (or after a delay/fade, if desired)
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
        GameObject[] deathParticles = GameObject.FindGameObjectsWithTag("DeathParticle");
        foreach (GameObject particle in deathParticles)
        {
            Destroy(particle);
        }

        // Restart the song track
        if (audioSource != null && songClip != null)
        {
            audioSource.Stop();
            audioSource.clip = songClip;
            audioSource.Play();
        }

        // Add additional reset logic for other environment elements, if necessary.
    }
}
