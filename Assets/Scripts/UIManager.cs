using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject hudPanel; // The new HUD panel
    [SerializeField] private TextMeshProUGUI attemptsText;

    [Header("Button Press Sound")]
    [SerializeField] private AudioSource audioSource;     // Reference to the AudioSource for the sound effect
    [SerializeField] private AudioClip tickSound;         // Reference to the tick sound effect

    public void UpdateUIState(GameState state, int attempts)
    {
        // Hide all panels by default
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);

        // Update attempts text
        if (attemptsText != null) 
            attemptsText.text = "" + attempts;

        switch (state)
        {
            case GameState.MainMenu:
                if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                break;
            case GameState.Playing:
                if (hudPanel != null) hudPanel.SetActive(true);
                break;
            case GameState.Paused:
                if (pausePanel != null) pausePanel.SetActive(true);
                break;
            // For Death, Victory, etc., you might have separate UI or just do nothing
        }
    }

    // Button event hooks
    public void OnStartButton()
    {
        PlayButtonSound(); // Play tick sound
        GameManager.Instance.StartGame();
    }

    public void OnPauseButton()
    {
        PlayButtonSound(); // Play tick sound
        GameManager.Instance.PauseGame();
    }

    public void OnResumeButton()
    {
        PlayButtonSound(); // Play tick sound
        GameManager.Instance.ResumeGame();
    }

    public void OnReturnToMenuButton()
    {
        PlayButtonSound(); // Play tick sound
        GameManager.Instance.ReturnToMainMenu();
    }

    // Method to play the button press sound
    private void PlayButtonSound()
    {
        if (audioSource != null && tickSound != null)
        {
            audioSource.PlayOneShot(tickSound); // Play the tick sound once
        }
    }
}
