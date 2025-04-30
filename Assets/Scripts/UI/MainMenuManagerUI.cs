using UnityEngine;

/// <summary>
/// Manages the UI navigation in the main menu, including Mode Selection, Matchmaking, Settings,
/// Achievements, Leaderboard, and My Team panel.
/// </summary>
public class MainMenuManagerUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject modeSelectionPanel;
    public GameObject matchmakingPanel;
    public GameObject settingsPanel;
    public GameObject achievementPanel;
    public GameObject leaderboardPanel;
    public GameObject myTeamPanel;
    private void Start()
    {
        AudioManager.instance.Play("MainMenuMusic");
    }



    /// <summary>
    /// Opens the mode selection panel and hides the main menu.
    /// </summary>
    public void OpenModeSelection()
    {
        TogglePanel(modeSelectionPanel, true);
        TogglePanel(mainMenuPanel, false);
        AudioManager.instance.Play("Click");
    }

    /// <summary>
    /// Closes the mode selection panel and shows the main menu.
    /// </summary>
    public void CloseModeSelection()
    {
        TogglePanel(modeSelectionPanel, false);
        TogglePanel(mainMenuPanel, true);
        AudioManager.instance.Play("Click");
    }

    /// <summary>
    /// Opens the matchmaking panel.
    /// </summary>
    public void OpenMatchmaking()
    {
        AudioManager.instance.Play("Click");
        TogglePanel(matchmakingPanel, true);
    }

    /// <summary>
    /// Closes the matchmaking panel.
    /// </summary>
    public void CloseMatchmaking()
    {
        AudioManager.instance.Play("Click");
        TogglePanel(matchmakingPanel, false);
    }

    /// <summary>
    /// Opens the main menu panel.
    /// </summary>
    public void OpenMainMenu()
    {
        TogglePanel(mainMenuPanel, true);
    }

    /// <summary>
    /// Closes the main menu panel.
    /// </summary>
    public void CloseMainMenu()
    {
        TogglePanel(mainMenuPanel, false);
    }

    /// <summary>
    /// Opens the settings panel.
    /// </summary>
    public void OpenSettings()
    {
        TogglePanel(settingsPanel, true);
    }

    /// <summary>
    /// Closes the settings panel.
    /// </summary>
    public void CloseSettings()
    {
        TogglePanel(settingsPanel, false);
    }

    /// <summary>
    /// Opens the achievements panel.
    /// </summary>
    public void OpenAchievementPanel()
    {
        TogglePanel(achievementPanel, true);
    }

    /// <summary>
    /// Closes the achievements panel.
    /// </summary>
    public void CloseAchievementPanel()
    {
        TogglePanel(achievementPanel, false);
    }

    /// <summary>
    /// Opens the leaderboard panel.
    /// </summary>
    public void OpenLeaderboard()
    {
        TogglePanel(leaderboardPanel, true);
    }

    /// <summary>
    /// Closes the leaderboard panel.
    /// </summary>
    public void CloseLeaderboard()
    {
        TogglePanel(leaderboardPanel, false);
    }

    /// <summary>
    /// Opens the My Team panel.
    /// </summary>
    public void OpenMyTeam()
    {
        TogglePanel(myTeamPanel, true);
    }

    /// <summary>
    /// Closes the My Team panel.
    /// </summary>
    public void CloseMyTeam()
    {
        TogglePanel(myTeamPanel, false);
    }

    /// <summary>
    /// Toggles the visibility of a panel with error handling.
    /// </summary>
    /// <param name="panel">The UI panel to toggle.</param>
    /// <param name="isActive">True to activate, false to deactivate.</param>
    private void TogglePanel(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
        else
        {
            Debug.LogError($"[MainMenuManagerUI] Missing reference: {nameof(panel)} is not assigned!");
        }
    }
}
