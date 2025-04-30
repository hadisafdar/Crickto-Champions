using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles matchmaking with synchronized animations, reward distribution, 
/// and countdown sequence before starting the game.
/// </summary>
public class MatchmakingPhoton : MonoBehaviourPunCallbacks
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
    public float countdownDuration = 1f;

    [Header("Reward Settings")]
    public int rewardAmount = 100000;

    private int countdown = 3;

    private Vector2 player1StartPos;
    private Vector2 player2StartPos;
    private Vector3 rewardPanelStartPos;

    private bool isPlayer1Joined = false;
    private bool isPlayer2Joined = false;

    public void Init()
    {// Generate a random username and set it
        string randomUsername = "Player" + Random.Range(1000, 9999);
        PhotonNetwork.NickName = randomUsername; // Set the player's nickname
        Debug.Log($"Assigned username: {PhotonNetwork.NickName}");
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "asia";
        InitializeUI();
        ConnectToPhoton();
    }

    /// <summary>
    /// Initializes UI positions and states before the game starts.
    /// </summary>
    void InitializeUI()
    {
        player1StartPos = player1Image.rectTransform.anchoredPosition;
        player2StartPos = player2Image.rectTransform.anchoredPosition;
        rewardPanelStartPos = rewardPanel.transform.position;

        player1Image.rectTransform.anchoredPosition = new Vector2(-Screen.width, player1StartPos.y);
        player2Image.rectTransform.anchoredPosition = new Vector2(Screen.width, player2StartPos.y);
        rewardPanel.transform.position = new Vector3(rewardPanelStartPos.x, -Screen.height, rewardPanelStartPos.z);

        rewardPanel.SetActive(false);
        lightningImage.color = new Color(1, 1, 1, 0);
        rewardText.gameObject.SetActive(false);
        statusText.text = "Connecting to matchmaking...";
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// Connects the player to the Photon server.
    /// </summary>
    void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected to server. Joining room...";
        
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "No rooms available. Creating a new room...";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Joined a room. Waiting for players...";
        AssignPlayerRole();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AssignPlayerRole();
    }

    /// <summary>
    /// Assigns player roles and triggers animations when players join the room.
    /// </summary>
    void AssignPlayerRole()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            isPlayer1Joined = true;
            photonView.RPC(nameof(AnimatePlayerJoin), RpcTarget.AllBuffered, 1);
        }
        else if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            isPlayer2Joined = true;
            photonView.RPC(nameof(AnimatePlayerJoin), RpcTarget.AllBuffered, 2);
        }
    }

    [PunRPC]
    void AnimatePlayerJoin(int playerNumber)
    {
        if (playerNumber == 1)
        {
            statusText.text = "Player 1 joined!";
            player1Image.rectTransform.DOAnchorPos(player1StartPos, playerMoveDuration).SetEase(Ease.OutBack);
        }
        else if (playerNumber == 2)
        {
            statusText.text = "Player 2 joined!";
            player2Image.rectTransform.DOAnchorPos(player2StartPos, playerMoveDuration).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    if (isPlayer1Joined && isPlayer2Joined)
                    {
                        DOVirtual.DelayedCall(delayBetweenAnimations, () =>
                        {
                            photonView.RPC(nameof(ShowLightningEffect), RpcTarget.AllBuffered);
                        });
                    }
                });
        }
    }

    [PunRPC]
    void ShowLightningEffect()
    {
        statusText.text = "Match found!";
        Sequence lightningSequence = DOTween.Sequence();
        lightningSequence.Append(lightningImage.DOFade(1, lightningFadeDuration));
        lightningSequence.AppendInterval(lightningShowDuration);
        lightningSequence.Append(lightningImage.DOFade(0, lightningFadeDuration))
            .OnComplete(() => photonView.RPC(nameof(ShowRewardPanel), RpcTarget.AllBuffered));
    }

    [PunRPC]
    void ShowRewardPanel()
    {
        rewardPanel.SetActive(true);

        rewardPanel.transform.DOMove(rewardPanelStartPos, rewardPanelFlyDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => DOVirtual.DelayedCall(delayBetweenAnimations, AnimateCoins));
    }

    /// <summary>
    /// Animates coins flying from player images to the reward panel.
    /// </summary>
    void AnimateCoins()
    {
        statusText.text = "Distributing reward...";
        rewardText.gameObject.SetActive(true);

        SpawnAndAnimateCoin(player1Image.transform.position);
        SpawnAndAnimateCoin(player2Image.transform.position);

        DOVirtual.DelayedCall(coinFlyDuration + delayBetweenAnimations, StartRewardCount);
    }

    /// <summary>
    /// Spawns a coin at the given position and animates it flying to the reward panel.
    /// </summary>
    void SpawnAndAnimateCoin(Vector3 startPosition)
    {
        if (coinPrefab == null) return;

        GameObject coin = Instantiate(coinPrefab, rewardPanel.transform);
        coin.transform.position = startPosition;

        coin.transform.DOMove(rewardPanel.transform.position, coinFlyDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => Destroy(coin));
    }

    /// <summary>
    /// Starts counting the reward amount after the coin animation completes.
    /// </summary>
    void StartRewardCount()
    {
        AudioManager.instance.Play("Coins");
        int currentReward = 0;
        DOTween.To(() => currentReward, x => currentReward = x, rewardAmount, 1f)
            .OnUpdate(() => rewardText.text = currentReward.ToString())
            .OnComplete(() => DOVirtual.DelayedCall(delayBetweenAnimations, StartCountdown));
    }

    /// <summary>
    /// Begins the countdown before starting the match.
    /// </summary>
    private Tween countdownTween; // Store reference to prevent duplicates

    void StartCountdown()
    {
        countdown = 3;
        statusText.text = $"Match will begin in {countdown}...";

        // Kill any existing countdown before starting a new one
        countdownTween?.Kill();

        // Start a new countdown animation
        countdownTween = DOTween.To(() => countdown, x => countdown = x, 0, countdown * countdownDuration)
            .OnUpdate(() => statusText.text = $"Match will begin in {countdown}...")
            .OnComplete(() =>
            {
                Debug.Log("Match started!");
                PhotonNetwork.AutomaticallySyncScene = true;
                // Ensure this only runs once per match
                if (PhotonNetwork.IsMasterClient && PhotonNetwork.AutomaticallySyncScene)
                {
                    PhotonNetwork.LoadLevel("SinglePlayer");
                }

                statusText.text = "Match Started!";
            });
    }




}
