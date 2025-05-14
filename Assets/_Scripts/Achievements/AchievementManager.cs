using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    public List<Achievement> Achievements;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddProgress(string title, int progress)
    {
        Achievement achievement = Achievements.Find(a => a.Title == title);

        if (achievement != null && !achievement.IsUnlocked)
        {
            achievement.CurrentProgress += progress;
            if (achievement.CurrentProgress >= achievement.TargetProgress)
            {
                UnlockAchievement(achievement);
            }
        }
    }

    private void UnlockAchievement(Achievement achievement)
    {
        achievement.IsUnlocked = true;
        achievement.CurrentProgress = achievement.TargetProgress;
        Debug.Log($"Achievement Unlocked: {achievement.Title}");
        // Call UI notification here if desired
    }
}
[System.Serializable]
public class Achievement
{
    public string Title;
    public string Description;
    public bool IsUnlocked;
    public int CurrentProgress;
    public int TargetProgress;
}
