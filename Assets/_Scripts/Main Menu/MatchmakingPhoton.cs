using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;  // For Hashtable

/// <summary>
/// Handles matchmaking with synchronized animations, reward distribution,
/// countdown sequence, and automatic bot assignment if no second player joins.
/// Also sets a room property "isBot" when a bot is used.
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

    private Coroutine waitForOpponentCoroutine;

    /// <summary>
    /// Begin matchmaking: assign a random nickname, set up UI, connect.
    /// </summary>
    public void Init()
    {
        string randomUsername = "Player" + Random.Range(1000, 9999);
        PhotonNetwork.NickName = randomUsername;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "asia";
        InitializeUI();
        PhotonNetwork.ConnectUsingSettings();
    }

    private void InitializeUI()
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

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected to server. Joining room...";
        PhotonNetwork.JoinRandomRoom();
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
        waitForOpponentCoroutine = StartCoroutine(WaitForOpponentRoutine());
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AssignPlayerRole();
        if (waitForOpponentCoroutine != null)
            StopCoroutine(waitForOpponentCoroutine);
    }

    private void AssignPlayerRole()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1 && !isPlayer1Joined)
        {
            isPlayer1Joined = true;
            photonView.RPC(nameof(AnimatePlayerJoin), RpcTarget.AllBuffered, 1);
        }
        else if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && !isPlayer2Joined)
        {
            isPlayer2Joined = true;
            photonView.RPC(nameof(AnimatePlayerJoin), RpcTarget.AllBuffered, 2);
        }
    }

    private IEnumerator WaitForOpponentRoutine()
    {
        yield return new WaitForSeconds(10f);
        if (!isPlayer2Joined)
            SetupBot();
    }

    private void SetupBot()
    {
        isPlayer2Joined = true;

        // set room property isBot = true
        ExitGames.Client.Photon.Hashtable props = new Hashtable { { "isBot", true } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        // assign bot a random nickname
        string botName = "Bot" + Random.Range(1000, 9999);

        // sync names: player1 is local, player2 is bot
        photonView.RPC(
            nameof(RPC_SyncPlayerNames),
            RpcTarget.AllBuffered,
            PhotonNetwork.NickName,
            botName
        );

        // animate bot joining
        photonView.RPC(
            nameof(AnimatePlayerJoin),
            RpcTarget.AllBuffered,
            2
        );
    }

    [PunRPC]
    void AnimatePlayerJoin(int playerNumber)
    {
        if (playerNumber == 1)
        {
            statusText.text = "Player 1 joined!";
            player1Image.rectTransform
                .DOAnchorPos(player1StartPos, playerMoveDuration)
                .SetEase(Ease.OutBack);
        }
        else if (playerNumber == 2)
        {
            statusText.text = "Player 2 joined!";
            player2Image.rectTransform
                .DOAnchorPos(player2StartPos, playerMoveDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    if (isPlayer1Joined && isPlayer2Joined)
                        DOVirtual.DelayedCall(delayBetweenAnimations, () =>
                            photonView.RPC(nameof(ShowLightningEffect), RpcTarget.AllBuffered)
                        );
                });
        }
    }

    [PunRPC]
    void ShowLightningEffect()
    {
        statusText.text = "Match found!";
        Sequence seq = DOTween.Sequence();
        seq.Append(lightningImage.DOFade(1, lightningFadeDuration));
        seq.AppendInterval(lightningShowDuration);
        seq.Append(lightningImage.DOFade(0, lightningFadeDuration))
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

    void AnimateCoins()
    {
        statusText.text = "Distributing reward...";
        rewardText.gameObject.SetActive(true);

        SpawnAndAnimateCoin(player1Image.transform.position);
        SpawnAndAnimateCoin(player2Image.transform.position);

        DOVirtual.DelayedCall(coinFlyDuration + delayBetweenAnimations, StartRewardCount);
    }

    void SpawnAndAnimateCoin(Vector3 startPosition)
    {
        if (coinPrefab == null) return;
        GameObject coin = Instantiate(coinPrefab, rewardPanel.transform);
        coin.transform.position = startPosition;
        coin.transform.DOMove(rewardPanel.transform.position, coinFlyDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => Destroy(coin));
    }

    void StartRewardCount()
    {
        AudioManager.instance.Play("Coins");
        int currentReward = 0;
        DOTween.To(() => currentReward, x => currentReward = x, rewardAmount, 1f)
            .OnUpdate(() => rewardText.text = currentReward.ToString())
            .OnComplete(() => DOVirtual.DelayedCall(delayBetweenAnimations, StartCountdown));
    }

    private Tween countdownTween;

    void StartCountdown()
    {
        countdown = 3;
        statusText.text = $"Match will begin in {countdown}...";
        countdownTween?.Kill();
        countdownTween = DOTween.To(() => countdown, x => countdown = x, 0, countdown * countdownDuration)
            .OnUpdate(() => statusText.text = $"Match will begin in {countdown}...")
            .OnComplete(() =>
            {
                Debug.Log("Match started!");
                PhotonNetwork.AutomaticallySyncScene = true;
                if (PhotonNetwork.IsMasterClient)
                    PhotonNetwork.LoadLevel("SinglePlayer");
                statusText.text = "Match Started!";
            });
    }

    [PunRPC]
    void RPC_SyncPlayerNames(string p1Name, string p2Name)
    {
        // Place your name-sync logic here
    }
}
