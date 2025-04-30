using DG.Tweening;
using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles multiplayer UI animations and status updates for the match.
/// </summary>
public class MultiplayerStatusUI : MonoBehaviourPun
{
    public static MultiplayerStatusUI Instance;

    [Header("UI Panels")]
    public GameObject statusPanel;
    public RectTransform player1Panel;
    public RectTransform player2Panel;
    public TextMeshProUGUI player1NameText;
    public TextMeshProUGUI player2NameText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI tossText;

    [Header("Animation Settings")]
    public float panelMoveDuration = 1f;
    public float statusTextDelay = 0.5f;
    public float tossDelay = 2f;
    public float matchStartCountdown = 3f;

    private Vector2 player1StartPos;
    private Vector2 player2StartPos;

    public bool batsman;

    public GameObject inGameUI;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeUI();
    }

    /// <summary>
    /// Initializes UI elements and hides unnecessary UI at start.
    /// </summary>
    private void InitializeUI()
    {
        
        player1StartPos = player1Panel.anchoredPosition;
        player2StartPos = player2Panel.anchoredPosition;

        // Move player panels off-screen
        player1Panel.anchoredPosition = new Vector2(-Screen.width, player1StartPos.y);
        player2Panel.anchoredPosition = new Vector2(Screen.width, player2StartPos.y);

        // Ensure status panel is active at the start
        statusPanel.SetActive(true);
        statusText.text = "Waiting for players...";
        tossText.text = "";
    }

    /// <summary>
    /// Updates the UI with player names and starts animations.
    /// </summary>
    public void SetPlayerNames(string player1Name, string player2Name)
    {

        photonView.RPC(nameof(RPC_SyncPlayerNames), RpcTarget.AllBuffered, player1Name, player2Name);

    }

    /// <summary>
    /// RPC function to sync player names across all clients.
    /// </summary>
    [PunRPC]
    private void RPC_SyncPlayerNames(string player1Name, string player2Name)
    {
        player1NameText.text = player1Name;
        player2NameText.text = player2Name;

        AnimatePlayerPanels();
    }


    /// <summary>
    /// Animates Player 1 & Player 2 panels into position.
    /// </summary>
    private void AnimatePlayerPanels()
    {
        player1Panel.DOAnchorPos(player1StartPos, panelMoveDuration).SetEase(Ease.InSine);
        player2Panel.DOAnchorPos(player2StartPos, panelMoveDuration).SetEase(Ease.InSine)
            .OnComplete(() => DOVirtual.DelayedCall(statusTextDelay, ShowTossStatus));
    }

    /// <summary>
    /// Displays tossing status.
    /// </summary>
    private void ShowTossStatus()
    {
        statusText.text = "Tossing...";
        DOVirtual.DelayedCall(tossDelay, () => MultiplayerManager.Instance.PerformToss());
    }

    /// <summary>
    /// Updates the UI with toss results and the chosen role.
    /// </summary>
    public void ShowTossResult(string winnerName, string role)
    {
        tossText.text = $"You will {role}.";
        
        if(role == "Bat")
        {
            batsman = true;
        }
        else
        {
            batsman=false;
        }

         //DOVirtual.DelayedCall(2f, StartMatchCountdown);
         Invoke(nameof(StartMatchCountdown), 2f);
        
    }

    /// <summary>
    /// Starts a countdown before the match begins.
    /// </summary>
    private bool countdownRunning = false; // Ensures countdown runs only once
    private bool matchStarted = false;
    public void StartMatchCountdown()
    {
        if (!countdownRunning) // Ensure only one instance runs
        {
            countdownRunning = true;
            StartCoroutine(MatchCountdownRoutine());
        }
    }

    /// <summary>
    /// Runs the countdown using a coroutine, ensuring only one instance.
    /// </summary>
    private IEnumerator MatchCountdownRoutine()
    {
        if(matchStarted) yield break;
        int count = (int)matchStartCountdown;
        statusText.text = $"Match will start in {count}...";

        while (count > 0)
        {
            yield return new WaitForSeconds(1f);
            count--;
            statusText.text = $"Match will start in {count}...";
        }
        photonView.RPC(nameof(RPC_MatchStarted), RpcTarget.AllBuffered);
        if (batsman) {

            MultiplayerManager.Instance.ChooseBatting();
        }
        else
        {
            MultiplayerManager.Instance.ChooseBowling();

        }
        statusPanel.SetActive(false);
        statusText.text = "Match Started!";
        countdownRunning = false; // Reset for next match
    }

    [PunRPC]
    private void RPC_MatchStarted()
    {
        FadeImageTransition.Instance.FadeInOut(MultiplayerManager.Instance.StartGame);
        inGameUI.SetActive(true);
        matchStarted = true;
        
    }

}
