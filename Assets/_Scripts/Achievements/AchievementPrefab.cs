using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPrefab : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI ProgressText;
    public GameObject UnlockedIcon;

    private Achievement achievement;

    /// <summary>
    /// Initializes the prefab with achievement data.
    /// </summary>
    /// <param name="achievementData">The achievement to display.</param>
    public void Initialize(Achievement achievementData)
    {
        achievement = achievementData;

        UpdateUI();
    }

    /// <summary>
    /// Updates the UI elements based on the current achievement data.
    /// </summary>
    private void UpdateUI()
    {
        TitleText.text = achievement.Title;
        DescriptionText.text = achievement.Description;
        ProgressText.text = $"{achievement.CurrentProgress}/{achievement.TargetProgress}";
        UnlockedIcon.SetActive(achievement.IsUnlocked);
    }

    /// <summary>
    /// Refreshes the achievement UI when progress changes.
    /// </summary>
    public void Refresh()
    {
        if (achievement != null)
        {
            UpdateUI();
        }
    }
}
