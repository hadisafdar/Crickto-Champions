using DG.Tweening;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the animations when the match ends, reversing the lightning effect 
/// and making coins fly from the reward panel to the winning player.
/// </summary>
public class MatchEndPanel : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TextMeshProUGUI statusText;
    public Image player1Image;
    public Image player2Image;
    public Image lightningImage;
    public GameObject rewardPanel;
    public TextMeshProUGUI rewardText;

    [Header("Prefabs")]
    public GameObject coinPrefab;

    [Header("Animation Settings")]
    public float playerMoveDuration = 1f;
    public float lightningFadeDuration = 0.5f;
    public float lightningShowDuration = 0.5f;
    public float rewardPanelFlyDuration = 1f;
    public float coinFlyDuration = 1f;
    public float delayBetweenAnimations = 0.5f;

    [Header("Reward Settings")]
    public int rewardAmount = 100000;

    private Vector3 rewardPanelStartPos;

    public void InitMatchEnd(bool isPlayer1Winner)
    {
        InitializeUI();
        ShowLightningEffect(isPlayer1Winner);
    }

    /// <summary>
    /// Initializes UI positions and states before playing animations.
    /// </summary>
    void InitializeUI()
    {
        rewardPanelStartPos = rewardPanel.transform.position;
        rewardPanel.SetActive(true);
        rewardText.gameObject.SetActive(true);
        lightningImage.color = new Color(1, 1, 1, 0);
        statusText.text = "Match Ended!";
    }

    /// <summary>
    /// Shows the lightning effect in reverse.
    /// </summary>
    void ShowLightningEffect(bool isPlayer1Winner)
    {
        statusText.text = "Finalizing results...";
        Sequence lightningSequence = DOTween.Sequence();

        lightningSequence.Append(lightningImage.DOFade(1, lightningFadeDuration))
            .AppendInterval(lightningShowDuration)
            .Append(lightningImage.DOFade(0, lightningFadeDuration))
            .OnComplete(() => ShowRewardPanel(isPlayer1Winner));
    }

    /// <summary>
    /// Animates the reward panel before sending coins to the winning player.
    /// </summary>
    void ShowRewardPanel(bool isPlayer1Winner)
    {
        statusText.text = "Distributing rewards...";

        rewardPanel.transform.DOMove(rewardPanelStartPos, rewardPanelFlyDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => DOVirtual.DelayedCall(delayBetweenAnimations, () => AnimateCoins(isPlayer1Winner)));
    }

    /// <summary>
    /// Animates coins flying from the reward panel to the winning player.
    /// </summary>
    void AnimateCoins(bool isPlayer1Winner)
    {
        statusText.text = "Rewarding the winner...";

        Vector3 targetPosition = isPlayer1Winner ? player1Image.transform.position : player2Image.transform.position;

        for (int i = 0; i < 5; i++) // Create multiple coins for effect
        {
            SpawnAndAnimateCoin(targetPosition);
        }

        DOVirtual.DelayedCall(coinFlyDuration + delayBetweenAnimations, EndMatch);
    }

    /// <summary>
    /// Spawns a coin at the reward panel and sends it to the winning player.
    /// </summary>
    void SpawnAndAnimateCoin(Vector3 targetPosition)
    {
        if (coinPrefab == null) return;

        GameObject coin = Instantiate(coinPrefab, rewardPanel.transform);
        coin.transform.position = rewardPanel.transform.position;

        coin.transform.DOMove(targetPosition, coinFlyDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => Destroy(coin));
    }

    /// <summary>
    /// Final match end state.
    /// </summary>
    void EndMatch()
    {
        AudioManager.instance.Play("Coins");
        statusText.text = "Match Over!";
    }
}
