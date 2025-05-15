// AirdropTracker.cs
using UnityEngine;
using System;
using System.Collections;

public class AirdropTracker : MonoBehaviour
{
    public static AirdropTracker Instance { get; private set; }

    [Header("Config")]
    [Tooltip("How many points per minute of active play")]
    public int pointsPerMinute = 5;

    // running totals
    public int TotalPoints { get; private set; }
    public int LastSessionPoints { get; private set; }
    public int ConsecutiveDays { get; private set; }

    // last session’s final multiplier (win * daily)
    public float LastSessionMultiplier { get; private set; }

    // per-second progress event (current session points without win bonus)
    public event Action<int> SessionPointsUpdated;

    // internal
    private float sessionTimeSec;
    private DateTime lastPlayDate;
    private bool isSessionActive;
    private float dailyMultiplier;
    private Coroutine progressRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else Destroy(gameObject);
    }

    void Update()
    {
        if (isSessionActive)
            sessionTimeSec += Time.deltaTime;
    }

    /// <summary>
    /// Call when gameplay actually begins.
    /// </summary>
    public void StartSession()
    {
        var today = DateTime.Today;
        if (lastPlayDate.Date == today) { /* same day: no streak change */ }
        else if (lastPlayDate.Date == today.AddDays(-1)) ConsecutiveDays++;
        else ConsecutiveDays = 1;

        lastPlayDate = today;

        // capture the daily streak multiplier for this session
        dailyMultiplier = GetDailyMultiplier(ConsecutiveDays);

        sessionTimeSec = 0f;
        isSessionActive = true;

        // start firing per-second updates
        if (progressRoutine != null) StopCoroutine(progressRoutine);
        progressRoutine = StartCoroutine(SessionProgressCoroutine());
    }

    /// <summary>
    /// Call when gameplay ends. 'isWin' applies the win bonus.
    /// </summary>
    public void EndSession(bool isWin)
    {
        isSessionActive = false;
        if (progressRoutine != null) StopCoroutine(progressRoutine);

        int minutesPlayed = Mathf.FloorToInt(sessionTimeSec / 60f);
        int basePoints = minutesPlayed * pointsPerMinute;

        float winMul = isWin ? 5f : 1f;
        LastSessionMultiplier = winMul * dailyMultiplier;
        LastSessionPoints = Mathf.RoundToInt(basePoints * LastSessionMultiplier);
        TotalPoints += LastSessionPoints;

        // final update so UI can show the very last value
        SessionPointsUpdated?.Invoke(LastSessionPoints);

        SaveData();
    }

    /// <summary>
    /// Every 1 second while playing, fire the current session points (no win bonus).
    /// </summary>
    private IEnumerator SessionProgressCoroutine()
    {
        while (isSessionActive)
        {
            yield return new WaitForSeconds(1f);

            int minutesPlayed = Mathf.FloorToInt(sessionTimeSec / 60f);
            int basePoints = minutesPlayed * pointsPerMinute;
            int sessionPts = Mathf.FloorToInt(basePoints * dailyMultiplier);

            SessionPointsUpdated?.Invoke(sessionPts);
        }
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

        var dateStr = PlayerPrefs.GetString("AT_LastPlayDate", "");
        if (!DateTime.TryParse(dateStr, out lastPlayDate))
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
