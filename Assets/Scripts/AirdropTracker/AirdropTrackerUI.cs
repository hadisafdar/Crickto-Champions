using UnityEngine;
using TMPro;

public class AirdropTrackerUI : MonoBehaviour
{
    [Header("Drag in your TextMeshProUGUI fields")]
    public TextMeshProUGUI totalValue;
    public TextMeshProUGUI multiplierValue;

    void Update()
    {
        var T = AirdropTracker.Instance;
        // e.g. "12,680"
        totalValue.text = T.TotalPoints.ToString("N0");
        // e.g. "2.5×"
        multiplierValue.text = $"{T.LastSessionMultiplier:F1}×";
    }
}
