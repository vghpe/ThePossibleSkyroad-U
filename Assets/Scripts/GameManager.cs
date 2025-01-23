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

    [Header("References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayerController playerController;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;  // Reference to an AudioSource for the music
    [SerializeField] private AudioClip songClip;       // Main music track

    public AudioSource AudioSource => audioSource;     // Public getter for PlayerController to stop music

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        SetGameState(GameState.MainMenu);
        attempts = 0;
    }

    public void StartGame()
    {
        attempts = 0;
        SetGameState(GameState.Playing);

        Debug.Log("Game Started");

        // Start the music track
        if (audioSource != null && songClip != null)
        {
            audioSource.clip = songClip;
            audioSource.Play();
        }
    }

    public void OnPlayerDeath()
    {
        attempts++;
        Debug.Log($"Player Died - Attempts: {attempts}");

        // No need to stop music here; PlayerController handles it
        ResetLevel();
        SetGameState(GameState.Playing);
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
            Time.timeScale = 0f;

            Debug.Log("Game Paused");
            // Optionally pause music
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
            Time.timeScale = 1f;

            Debug.Log("Game Resumed");
            // Optionally resume music
            if (audioSource != null)
            {
                audioSource.UnPause();
            }
        }
    }

    public void OnLevelComplete()
    {
        Debug.Log("Level Complete!");
        SetGameState(GameState.Victory);

        // Stop the music if desired
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void ReturnToMainMenu()
    {
        ResetLevel();
        attempts = 0;
        SetGameState(GameState.MainMenu);
        Time.timeScale = 1f;
        Debug.Log("Returned to Main Menu");

        // Stop music if returning to main menu
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void SetGameState(GameState newState)
    {
        CurrentState = newState;

        // Notify UI of state change
        if (uiManager != null)
        {
            uiManager.UpdateUIState(newState, attempts);
        }
    }

    private void ResetLevel()
    {
        // Reset the player
        if (playerController != null)
        {
            playerController.ResetPlayer();
        }
        
        // Destroy all active death particles
        GameObject[] deathParticles = GameObject.FindGameObjectsWithTag("DeathParticle");
        foreach (GameObject particle in deathParticles)
        {
            Destroy(particle);
        }

        // Optionally restart the music if required
        if (audioSource != null && songClip != null)
        {
            audioSource.Stop();
            audioSource.clip = songClip;
            audioSource.Play();
        }
    }
}
