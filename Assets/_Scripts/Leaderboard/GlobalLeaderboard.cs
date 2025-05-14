using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class GlobalLeaderboard : MonoBehaviour
{
    public GameObject leaderboardPrefab; // Assign your Leaderboard Prefab UI in the inspector
    public Transform leaderboardContainer; // Parent object to hold instantiated prefabs

    private string playFabTitleId = "YOUR_TITLE_ID"; // Replace with your PlayFab Title ID


    private void Start()
    {
        GetLeaderboard("Global");
    }

    [ContextMenu("Submit Test Score")]
    public void SubmitTestScore()
    {
        int testScore = 1000;           // Example score
        float testDuration = 375.5f;    // Example duration in seconds
        int testKnockdowns = 50;        // Example knockdowns
        int testWins = 10;              // Example wins
        string testCountry = "USA";     // Example country

        SubmitScore(testScore, testDuration, testKnockdowns, testWins, testCountry);
    }

    public void SubmitScore(int score, float duration, int knockdowns, int wins, string country)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "Global_Score", Value = score },
                new StatisticUpdate { StatisticName = "Global_Duration", Value = Mathf.RoundToInt(duration) },
                new StatisticUpdate { StatisticName = "Global_Knockdowns", Value = knockdowns },
                new StatisticUpdate { StatisticName = "Global_Wins", Value = wins }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnScoreSubmitSuccess, OnScoreSubmitFailure);
    }

    private void OnScoreSubmitSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Score submitted successfully!");
        GetLeaderboard("Global");
    }

    private void OnScoreSubmitFailure(PlayFabError error)
    {
        Debug.LogError("Score submission failed: " + error.GenerateErrorReport());
    }

    public void GetLeaderboard(string category)
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = $"{category}_Score",
            StartPosition = 0,
            MaxResultsCount = 5 // Top 5
        };

        PlayFabClientAPI.GetLeaderboard(request, result => OnLeaderboardSuccess(result, category), OnLeaderboardFailure);
    }

    private void OnLeaderboardSuccess(GetLeaderboardResult result, string category)
    {
        ClearLeaderboard();

        for (int i = 0; i < result.Leaderboard.Count; i++)
        {
            var entry = result.Leaderboard[i];
            GameObject row = Instantiate(leaderboardPrefab, leaderboardContainer);

            TextMeshProUGUI rankText = row.transform.Find("Rank").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = row.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI durationText = row.transform.Find("Duration").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI knockdownText = row.transform.Find("Knockdown").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI winsText = row.transform.Find("Wins").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI countryText = row.transform.Find("Country").GetComponent<TextMeshProUGUI>();

            rankText.text = $"{entry.Position + 1}";
            nameText.text = entry.DisplayName ?? "Unknown";
            durationText.text = FormatDuration(entry.StatValue);
            knockdownText.text = GetStatValue(category, "Knockdowns", entry.PlayFabId);
            winsText.text = GetStatValue(category, "Wins", entry.PlayFabId);
            countryText.text = GetPlayerCountry(entry.PlayFabId);
        }
    }

    private void OnLeaderboardFailure(PlayFabError error)
    {
        Debug.LogError("Failed to retrieve leaderboard: " + error.GenerateErrorReport());
    }

    private void ClearLeaderboard()
    {
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private string FormatDuration(int totalSeconds)
    {
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;
        return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
    }

    private string GetStatValue(string category, string statName, string playerId)
    {
        // Placeholder: Implement PlayFab calls to fetch individual stats for players
        return "0";
    }

    private string GetPlayerCountry(string playerId)
    {
        // Placeholder: Implement fetching player country from PlayFab player data
        return "N/A";
    }
}
