using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles multiplayer UI animations and status updates for the match,
/// and drives the toss via CricketGameManager.
/// </summary>
public class MultiplayerStatusUI : MonoBehaviourPun
{
    #region Singleton & Events

    public static MultiplayerStatusUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        CricketGameManager.OnTossCompleted += HandleTossCompleted;
    }

    private void OnDisable()
    {
        CricketGameManager.OnTossCompleted -= HandleTossCompleted;
    }

    #endregion

    #region Inspector Fields

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

    [Header("In-Game UI")]
    public GameObject inGameUI;

    [Header("Game Cameras")]
    public GameObject mainCamera;
    public GameObject animatedCamera;

    [Header("Bot Settings")]
    public bool isBot;
    #endregion

    #region Internal State

    private Vector2 player1StartPos;
    private Vector2 player2StartPos;
    public bool batsman;
    private bool countdownRunning = false;
    private bool matchStarted = false;

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        InitializeUI();

        if (!PhotonNetwork.InRoom) return;

        // determine if this is a bot match
        isBot = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("isBot")
                  && (bool)PhotonNetwork.CurrentRoom.CustomProperties["isBot"];

        // player 1 name
        string p1 = PhotonNetwork.PlayerList.Length > 0
            ? PhotonNetwork.PlayerList[0].NickName
            : "Waiting...";

        // player 2 name: real player, or bot, or waiting
        string p2;
        if (PhotonNetwork.PlayerList.Length > 1)
        {
            p2 = PhotonNetwork.PlayerList[1].NickName;
        }
        else if (isBot)
        {
            p2 = "Bot" + Random.Range(1000, 9999);
        }
        else
        {
            p2 = "Waiting...";
        }

        SetPlayerNames(p1, p2);
    }

    #endregion

    #region Initialization

    private void InitializeUI()
    {
        player1StartPos = player1Panel.anchoredPosition;
        player2StartPos = player2Panel.anchoredPosition;

        player1Panel.anchoredPosition = new Vector2(-Screen.width, player1StartPos.y);
        player2Panel.anchoredPosition = new Vector2(Screen.width, player2StartPos.y);

        statusPanel.SetActive(true);
        statusText.text = "Waiting for players...";
        tossText.text = "";
    }

    #endregion

    #region Player Names

    public void SetPlayerNames(string player1Name, string player2Name)
    {
        Debug.Log(player1Name + " " + player2Name);
        photonView.RPC(
            nameof(RPC_SyncPlayerNames),
            RpcTarget.AllBuffered,
            player1Name,
            player2Name
        );
    }

    [PunRPC]
    private void RPC_SyncPlayerNames(string player1Name, string player2Name)
    {
        player1NameText.text = player1Name;
        player2NameText.text = player2Name;
        AnimatePlayerPanels();
    }

    #endregion

    #region Animations

    private void AnimatePlayerPanels()
    {
        player1Panel.DOAnchorPos(player1StartPos, panelMoveDuration).SetEase(Ease.InSine);
        player2Panel.DOAnchorPos(player2StartPos, panelMoveDuration).SetEase(Ease.InSine)
            .OnComplete(() => DOVirtual.DelayedCall(statusTextDelay, ShowTossStatus));
    }

    private void ShowTossStatus()
    {
        statusText.text = "Tossing...";
        DOVirtual.DelayedCall(tossDelay, () => CricketGameManager.Instance.DoToss());
    }

    #endregion

    #region Toss Handling

    private void HandleTossCompleted(int batsmanActorNumber)
    {
        string winnerName = batsmanActorNumber < 0
            ? "Bot"
            : PhotonNetwork.CurrentRoom.GetPlayer(batsmanActorNumber)?.NickName ?? "Unknown";

        bool localIsBatsman = batsmanActorNumber >= 0
            && PhotonNetwork.LocalPlayer.ActorNumber == batsmanActorNumber;

        string role = localIsBatsman ? "Bat" : "Bowl";
        ShowTossResult(winnerName, role);
    }

    public void ShowTossResult(string winnerName, string role)
    {
        tossText.text = $"{winnerName} will {role}.";
        batsman = (role == "Bat");
        Invoke(nameof(StartMatchCountdown), 2f);
    }

    #endregion

    #region Match Countdown

    public void StartMatchCountdown()
    {
        if (countdownRunning) return;
        countdownRunning = true;
        StartCoroutine(MatchCountdownRoutine());
    }

    private IEnumerator MatchCountdownRoutine()
    {
        if (matchStarted) yield break;

        int count = Mathf.CeilToInt(matchStartCountdown);
        while (count > 0)
        {
            statusText.text = $"Match will start in {count}...";
            yield return new WaitForSeconds(1f);
            count--;
        }

        photonView.RPC(nameof(RPC_MatchStarted), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_MatchStarted()
    {
        statusPanel.SetActive(false);
        statusText.text = "Match Started!";
        inGameUI.SetActive(true);
        matchStarted = true;

        animatedCamera.SetActive(false);
        mainCamera.SetActive(true);

        countdownRunning = false;
        CricketGameManager.GameStarted = true;
        FadeImageTransition.Instance.FadeInOut(() =>
        {
            CricketScoreManager.Instance.isBatting = batsman;
            if (batsman)
            {
                CricketGameManager.Instance.EnableBatsman();
                
            }
           
            else
            {
                CricketGameManager.Instance.EnableBowler();
                Debug.Log("this");

            }
        });
       
       
    }

    #endregion
}
