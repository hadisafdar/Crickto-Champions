using Photon.Pun;
using UnityEngine;

public class BallLaunchSettings : MonoBehaviourPun
{
    // Singleton instance
    public static BallLaunchSettings Instance { get; private set; }

    [Header("Adjusted Launch Parameters")]
    public float launchHeight = 5f;             // Final launch height (adjusted dynamically)
    public float forceMultiplier = 1f;          // Final force multiplier (adjusted dynamically)

    [Header("Slider Ranges")]
    public SliderRange[] sliderRanges;          // Array of ranges adjustable in Inspector

    private void Awake()
    {
        // Singleton implementation
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /*public void AdjustLaunchSettings(float sliderValue)
    {
        // Iterate through the ranges to find the correct one based on the slider value
        foreach (var range in sliderRanges)
        {
            if (sliderValue >= range.minValue && sliderValue <= range.maxValue)
            {
                // Lerp between min and max values based on slider's position within the range
                float normalizedValue = Mathf.InverseLerp(range.minValue, range.maxValue, sliderValue);

                launchHeight = Mathf.Lerp(range.minLaunchHeight, range.maxLaunchHeight, normalizedValue);
                forceMultiplier = Mathf.Lerp(range.minForceMultiplier, range.maxForceMultiplier, normalizedValue);

               
                return;
            }
        }

        // Default values if no range matches
        launchHeight = 5f;
        forceMultiplier = 1f;
    }*/
    /// <summary>
    /// Adjusts the launch settings based on the slider value and synchronizes across the network.
    /// </summary>
    public void AdjustLaunchSettings(float sliderValue)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            photonView.RPC("RPC_AdjustLaunchSettings", RpcTarget.All, sliderValue);
        }
        else
        {
            ApplyLaunchSettings(sliderValue);
        }
    }

    /// <summary>
    /// RPC to synchronize launch settings across all clients.
    /// </summary>
    [PunRPC]
    private void RPC_AdjustLaunchSettings(float sliderValue)
    {
        ApplyLaunchSettings(sliderValue);
    }

    /// <summary>
    /// Calculates and applies launch settings based on slider value.
    /// </summary>
    private void ApplyLaunchSettings(float sliderValue)
    {
        foreach (var range in sliderRanges)
        {
            if (sliderValue >= range.minValue && sliderValue <= range.maxValue)
            {
                float normalizedValue = Mathf.InverseLerp(range.minValue, range.maxValue, sliderValue);

                launchHeight = Mathf.Lerp(range.minLaunchHeight, range.maxLaunchHeight, normalizedValue);
                forceMultiplier = Mathf.Lerp(range.minForceMultiplier, range.maxForceMultiplier, normalizedValue);

                return;
            }
        }

        // Default values if no range matches
        launchHeight = 5f;
        forceMultiplier = 1f;
    }
}

[System.Serializable]
public class SliderRange
{
    [Header("Slider Range Settings")]
    public string zoneColor;                  // Zone name: Red, Green, Blue
    public float minValue;                    // Minimum slider value for this range
    public float maxValue;                    // Maximum slider value for this range

    [Header("Launch Settings")]
    public float minLaunchHeight;             // Minimum launch height for this range
    public float maxLaunchHeight;             // Maximum launch height for this range
    public float minForceMultiplier;          // Minimum force multiplier for this range
    public float maxForceMultiplier;          // Maximum force multiplier for this range
}
