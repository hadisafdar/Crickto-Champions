// AirdropTracker.cs
using UnityEngine;
using System;

public class AirdropTracker : MonoBehaviour
{
    public static AirdropTracker Instance { get; private set; }

    [Header("Config")]
    [Tooltip("How many points per minute of active play")]
    public int pointsPerMinute = 5;

    // your running totals
    public int TotalPoints { get; private set; }
    public int LastSessionPoints { get; private set; }
    public int ConsecutiveDays { get; private set; }

    // internal
    private float sessionTimeSec;
    private DateTime lastPlayDate;
    private bool isSessionActive;
    public float LastSessionMultiplier { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (isSessionActive)
        {
            sessionTimeSec += Time.deltaTime;
        }
    }

    /// <summary>
    /// Call this when actual gameplay starts (e.g. at level load).
    /// </summary>
    public void StartSession()
    {
        // update streak
        var today = DateTime.Today;
        if (lastPlayDate.Date == today)
        {
            // already counted for today
        }
        else if (lastPlayDate.Date == today.AddDays(-1))
        {
            ConsecutiveDays++;
        }
        else
        {
            ConsecutiveDays = 1;
        }
        lastPlayDate = today;

        sessionTimeSec = 0f;
        isSessionActive = true;
    }

    /// <summary>
    /// Call this when gameplay ends (e.g. level complete). 
    /// Pass in true if the player won that session.
    /// </summary>
    public void EndSession(bool isWin)
    {
        isSessionActive = false;

        int minutesPlayed = Mathf.FloorToInt(sessionTimeSec / 60f);
        int basePoints = minutesPlayed * pointsPerMinute;

        float winMul = isWin ? 5f : 1f;
        float dailyMul = GetDailyMultiplier(ConsecutiveDays);

        LastSessionMultiplier = winMul * dailyMul;              // ← new
        LastSessionPoints = Mathf.RoundToInt(basePoints * LastSessionMultiplier);
        TotalPoints += LastSessionPoints;

        SaveData();
    }


    float GetDailyMultiplier(int days)
    {
        if (days >= 15) return 2f;
        if (days >= 7) return 1.5f;
        if (days >= 3) return 1.2f;
        return 1f;
    }

    void LoadData()
    {
        TotalPoints = PlayerPrefs.GetInt("AT_TotalPoints", 0);
        ConsecutiveDays = PlayerPrefs.GetInt("AT_ConsecutiveDays", 0);

        string dateStr = PlayerPrefs.GetString("AT_LastPlayDate", "");
        if (DateTime.TryParse(dateStr, out var dt))
            lastPlayDate = dt;
        else
            lastPlayDate = DateTime.MinValue;
    }

    void SaveData()
    {
        PlayerPrefs.SetInt("AT_TotalPoints", TotalPoints);
        PlayerPrefs.SetInt("AT_ConsecutiveDays", ConsecutiveDays);
        PlayerPrefs.SetString("AT_LastPlayDate", lastPlayDate.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();
    }
}
