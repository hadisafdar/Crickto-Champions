using UnityEngine;

public class AchievementUIManager : MonoBehaviour
{
    [Header("Prefab and UI")]
    public GameObject AchievementPrefab; // Prefab for displaying an achievement
    public Transform AchievementList;   // Parent object to hold the achievement prefabs

    private void Start()
    {
        PopulateAchievements();
    }

    /// <summary>
    /// Populates the achievements list UI with current data.
    /// </summary>
    private void PopulateAchievements()
    {
        // Clear any existing children to prevent duplicates
        foreach (Transform child in AchievementList)
        {
            Destroy(child.gameObject);
        }

        // Create a prefab instance for each achievement
        foreach (var achievement in AchievementManager.Instance.Achievements)
        {
            GameObject prefabInstance = Instantiate(AchievementPrefab, AchievementList);
            prefabInstance.GetComponent<AchievementPrefab>().Initialize(achievement);
        }
    }

    /// <summary>
    /// Refreshes the UI for all achievements.
    /// </summary>
    public void RefreshAchievements()
    {
        foreach (Transform child in AchievementList)
        {
            child.GetComponent<AchievementPrefab>().Refresh();
        }
    }

    /// <summary>
    /// Triggered to update UI, e.g., when progress changes.
    /// </summary>
    public void OnAchievementProgressUpdated()
    {
        RefreshAchievements();
    }
}
