using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject hudPanel; // The new HUD panel
    //[SerializeField] private Text attemptsTextLegacy;
    [SerializeField] private TextMeshProUGUI attemptsText;

    public void UpdateUIState(GameState state, int attempts)
    {
        // Hide all panels by default
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);

        
        Debug.Log("Hid both panels");
        // Update attempts text
        if (attemptsText != null) 
            attemptsText.text = "Attempts: " + attempts;

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
        GameManager.Instance.StartGame();
    }

    public void OnPauseButton()
    {
        GameManager.Instance.PauseGame();
    }

    public void OnResumeButton()
    {
        GameManager.Instance.ResumeGame();
    }

    public void OnReturnToMenuButton()
    {
        GameManager.Instance.ReturnToMainMenu();
    }
}