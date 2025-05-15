using UnityEngine;
using TMPro;
using System.Collections;

public class AirdropTrackerUI : MonoBehaviour
{
    [Header("Drag in your TextMeshProUGUI fields")]
    public TextMeshProUGUI totalValue;
    public TextMeshProUGUI multiplierValue;

    void OnEnable()
    {
        // kick off the 1-second update loop
        StartCoroutine(UpdateLoop());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator UpdateLoop()
    {
        while (true)
        {
            RefreshUI();
            yield return new WaitForSeconds(1f);
        }
    }

    void RefreshUI()
    {
        var T = AirdropTracker.Instance;
        // e.g. "12,680"
        totalValue.text = T.TotalPoints.ToString("N0");
        // e.g. "2.5×"
        multiplierValue.text = $"{T.LastSessionMultiplier:F1}×";
    }
}
