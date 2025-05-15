using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton manager for a 2-player or human-vs-bot cricket match:
/// handles toss, role assignment, camera/UI setup, and role swapping.
/// </summary>
public class CricketGameManager : MonoBehaviourPunCallbacks
{
    #region Singleton & Events

    public static CricketGameManager Instance { get; private set; }

    /// <summary>Fired on all clients when a toss completes. Parameter is the batsman’s ActorNumber (–1 for bot).</summary>
    public static event Action<int> OnTossCompleted;

    /// <summary>Fired after roles are assigned on each client. Parameter indicates if this client is batsman.</summary>
    public static event Action<bool> OnRoleAssigned;

    /// <summary>Fired on all clients when roles are swapped mid-game.</summary>
    public static event Action OnRolesSwapped;




    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Inspector Fields

    [Header("Cameras & Canvases")]
    public GameObject batsmanCamera;
    public GameObject bowlerCamera;
    public Canvas batsmanCanvas;
    public Canvas bowlerCanvas;

    [Header("Controllers")]
    public BatsmanController batsmanController;
    public BowlerController bowlerController;
    public ParticleController marker;

    #endregion

    #region Internal State

    public bool isBatsman;
    public bool useBot;
    public static bool GameStarted;
    #endregion

    #region Photon Callbacks

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();



    }
    private void Start()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("isBot"))
        {
            bool isBot = (bool)PhotonNetwork.CurrentRoom.CustomProperties["isBot"];
            Debug.Log("isBot property: " + isBot);
        }
        else
        {
            Debug.Log("isBot property not found");
        }
        // Read "isBot" from room properties
        useBot = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("isBot")
              && (bool)PhotonNetwork.CurrentRoom.CustomProperties["isBot"];
        Debug.Log(useBot);
        // MasterClient kicks off the toss once ready
        if (PhotonNetwork.IsMasterClient &&
            (PhotonNetwork.PlayerList.Length == 2 || useBot))
        {
            //DoToss();
        }
    }
    #endregion

    #region Toss & Role Assignment

    /// <summary>
    /// MasterClient selects the batsman at random (or human vs bot) and broadcasts it.
    /// </summary>
    public void DoToss()
    {


        if (!PhotonNetwork.IsMasterClient) return;

        int batsmanActorNumber;
        var players = PhotonNetwork.PlayerList;

        if (useBot && players.Length == 1)
        {
            bool humanIsBatsman = UnityEngine.Random.value < 0.5f;
            batsmanActorNumber = humanIsBatsman
                ? players[0].ActorNumber
                : -1; // –1 indicates the bot
        }
        else
        {
            int idx = UnityEngine.Random.Range(0, players.Length);
            batsmanActorNumber = players[idx].ActorNumber;
        }

        photonView.RPC(
            nameof(RPC_AssignRoles),
            RpcTarget.AllBuffered,
            batsmanActorNumber
        );
    }

    [PunRPC]
    private void RPC_AssignRoles(int batsmanActorNumber)
    {

        // notify subscribers of toss result
        OnTossCompleted?.Invoke(batsmanActorNumber);

        // determine local role
        if (useBot && batsmanActorNumber == -1)
        {
            // bot is batsman → this client bowls
            isBatsman = false;
        }
        else
        {
            // human case
            isBatsman = PhotonNetwork.LocalPlayer.ActorNumber == batsmanActorNumber;
        }


        // notify subscribers of role assignment
        OnRoleAssigned?.Invoke(isBatsman);

        // if bot-match, immediately fire ball-launched to skip UI
        if (useBot)
        {

        }
    }

    #endregion

    #region Role Swap

    /// <summary>
    /// Swap batsman ↔ bowler mid-game and broadcast to all clients.
    /// </summary>
    public void SwapRoles()
    {
        photonView.RPC(nameof(RPC_SwapRoles), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_SwapRoles()
    {
        isBatsman = !isBatsman;
        OnRolesSwapped?.Invoke();
        CricketScoreManager.Instance.isBatting = isBatsman;
        if (useBot)
        {
            // If it's a bot match, handle the role swapping accordingly.
            if (isBatsman)
            {
                // Bot takes the batsman role → Enable the batsman controller and disable the bowler controller

                EnableBowlerForBot();
            }
            else
            {
                // Bot takes the bowler role → Enable the bowler controller and disable the batsman controller

                EnableBatsmanForBot();
            }
           
            return;
        }


        if (isBatsman)
        {

            EnableBatsman();
        }
        else
        {
            EnableBowler();
        }
    }

    #endregion
    #region Bot Role Handling

    /// <summary>
    /// Enable the batsman controller for bot role and disable the bowler controller.
    /// </summary>
    private void EnableBatsmanForBot()
    {

        batsmanCanvas.gameObject.SetActive(false);
        bowlerCamera.SetActive(true);
        batsmanCamera.SetActive(false);
        bowlerCanvas.gameObject.SetActive(true);
        batsmanController.isBot = true;
        bowlerController.isBot = false;
        marker.isBot = false;

    }

    /// <summary>
    /// Enable the bowler controller for bot role and disable the batsman controller.
    /// </summary>
    private void EnableBowlerForBot()
    {
        batsmanCanvas.gameObject.SetActive(true);
        bowlerCamera.SetActive(false);
        batsmanCamera.SetActive(true);
        bowlerCanvas.gameObject.SetActive(false);
        batsmanController.isBot = false;
        bowlerController.isBot = true;
        marker.isBot = true;

    }

    #endregion

    [Header("Bowler Models Container")]
    public Transform bowlerVisualsContainer;

    [Header("Batsman Models Container")]
    public Transform batsmanVisualsContainer;

    void ApplyVisuals()
    {
        StartCoroutine(UpdateVisuals());

    }

    IEnumerator UpdateVisuals()
    {
        yield return new WaitForSeconds(0.5f);
        var props = bowlerController.photonView.Owner.CustomProperties;
        var bats = batsmanController.photonView.Owner.CustomProperties;

        // read the string IDs (or default to "0")
        string bowlerId = props.ContainsKey("BowlerID") ? (string)props["BowlerID"] : "0";
        string batsmanId = bats.ContainsKey("BatsmanID") ? (string)props["BatsmanID"] : "0";

        foreach (Transform child in bowlerVisualsContainer)
            child.gameObject.SetActive(false);

        if (bowlerId != "0")
        {
            var match = bowlerVisualsContainer.Find(bowlerId);
            if (match != null)
            {

                match.gameObject.SetActive(true);
                match.GetComponent<BowlerVisual>().SetBowler();
            }
        }

        // --- Batsman visuals ---
        foreach (Transform child in batsmanVisualsContainer)
            child.gameObject.SetActive(false);

        if (batsmanId != "0")
        {
            var match = batsmanVisualsContainer.Find(batsmanId);
            if (match != null)
            {
                match.gameObject.SetActive(true);
                match.GetComponent<BatsmanVisual>().SetBatsman();
            }
        }


    }







    #region Helpers





    public void EnableBatsman()
    {

        if (useBot)
        {

            ApplyVisuals();
            EnableBowlerForBot();
            return;


        }

        batsmanCanvas.gameObject.SetActive(true);
        bowlerCamera.SetActive(false);
        batsmanCamera.SetActive(true);
        bowlerCanvas.gameObject.SetActive(false);
        batsmanController.gameObject.SetActive(true);
        bowlerController.gameObject.SetActive(true);
        marker.gameObject.SetActive(true);
        TakeOwnership(batsmanController.gameObject);
        ApplyVisuals();
    }
    public void EnableBowler()
    {
        if (useBot)
        {
            ApplyVisuals();
            EnableBatsmanForBot();
            return;


        }
        batsmanCanvas.gameObject.SetActive(false);
        bowlerCamera.SetActive(true);
        batsmanCamera.SetActive(false);
        bowlerCanvas.gameObject.SetActive(true);
        batsmanController.gameObject.SetActive(true);
        bowlerController.gameObject.SetActive(true);
        marker.gameObject.SetActive(true);
        TakeOwnership(marker.gameObject);
        ApplyVisuals();
    }
    public void TakeOwnership(GameObject obj)
    {
        PhotonView view = obj.GetComponent<PhotonView>();

        if (view != null)
        {
            if (view.Owner.NickName == PhotonNetwork.LocalPlayer.NickName)
            {
                //Debug.Log("You already own this object.");
            }
            else
            {
                // Transfer ownership to the local player
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
                //Debug.Log($"Ownership of {obj.name} transferred to {PhotonNetwork.LocalPlayer.NickName}.");
            }
        }
        else
        {
            Debug.LogError("No PhotonView found on the GameObject. Cannot take ownership.");
        }
    }
    #endregion
}
