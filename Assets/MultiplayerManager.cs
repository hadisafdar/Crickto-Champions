using Photon.Pun;
using Photon.Realtime;
using SimpleInputNamespace;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI; // For optional UI

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    public static MultiplayerManager Instance;
    public TextMeshProUGUI statusText; // Optional UI for displaying game state

    private bool tossCompleted = false;
    private Player tossWinner;
    public bool isBatsman;
    public GameObject batsman, marker, bowlerBall, Bowler;
    public Transform batsmanSpawnPosition, markerSpawnPosition, bowlerBallSpawnPosition, bowlerSpawnPosition;


    public Joystick batsmanJoystick;
    public Slider batsmanSlider;

    public TargetArea straightArea;
    public TargetArea legArea;
    public TargetArea frontDefenceArea;
    public TargetArea backDefenceArea;



    [Header("Bowler Elements")]

    public GameObject markerCamera;
    public GameObject bowlerCamera;
    public GameObject bowlCamera;
    public Transform markerTransform;
    public Slider bowlerSlider;
    public Image sliderFillImage;
    public Button startBowlButton;
    public Button markerLockButton;
    // public CinemachineCamera markerCamera; 


    public CinemachineCamera markerCameraCin;
    public CinemachineCamera bowlerCameraCin;
    public CinemachineCamera bowlCameraCin;
    public CinemachineCamera batsmanCameraCin;



    public FielderController fielderController;

    [Space(20)]
    [Header("Panel Elements")]
    public GameObject selectionPanel;
    public Button battingButton, bowlingButton;



    [Space(20)]
    [Header("Spawned Elements")]
    public GameObject spawnedBatsman;
    public GameObject spawnedBowler;
    public GameObject spawnedBall;
    public GameObject spawnedMarker;




    public GameObject animatedCamera;
    public GameObject mainCamera;

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
        AudioManager.instance.Play("Audience");
        AudioManager.instance.Stop("MainMenuMusic");

        // Ensure at least one player is in the room before calling SetPlayerNames
        if (PhotonNetwork.InRoom)
        {
            string player1Name = PhotonNetwork.PlayerList.Length > 0 ? PhotonNetwork.PlayerList[0].NickName : "Waiting...";
            string player2Name = PhotonNetwork.PlayerList.Length > 1 ? PhotonNetwork.PlayerList[1].NickName : "Waiting...";

            MultiplayerStatusUI.Instance.SetPlayerNames(player1Name, player2Name);
        }

        SpawnNetworkObjects();
        selectionPanel.SetActive(false);
        battingButton.onClick.AddListener(ChooseBatting);
        bowlingButton.onClick.AddListener(ChooseBowling);
    }




    private void SpawnNetworkObjects()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            spawnedBatsman = PhotonNetwork.Instantiate(batsman.name, batsmanSpawnPosition.position, batsmanSpawnPosition.rotation);
            spawnedBowler = PhotonNetwork.Instantiate(Bowler.name, bowlerSpawnPosition.position, bowlerSpawnPosition.rotation);
            spawnedBall = PhotonNetwork.Instantiate(bowlerBall.name, bowlerBallSpawnPosition.position, bowlerBallSpawnPosition.rotation);
            spawnedMarker = PhotonNetwork.Instantiate(marker.name, markerSpawnPosition.position, markerSpawnPosition.rotation);


            //spawnedBatsman.GetComponent<BatsmanController>().InitBatsman(batsmanJoystick, batsmanSlider, straightArea, backDefenceArea, frontDefenceArea, legArea);

            //spawnedBowler.GetComponent<BowlerController>().InitBowler(markerCamera, bowlerCamera, bowlCamera, spawnedBall, markerTransform, bowlerSlider, sliderFillImage, startBowlButton, markerLockButton);
            StartCoroutine(DelayedRPC());


        }
        else
        {
            Debug.Log("You are not the Master Client; waiting for synchronization.");
        }


    }


    private IEnumerator DelayedRPC()
    {
        yield return new WaitForSeconds(5f); // Wait for 1 second
        photonView.RPC(nameof(RPC_SyncNetworkObjects), RpcTarget.AllBuffered,
           spawnedBatsman.GetComponent<PhotonView>().ViewID,
           spawnedBowler.GetComponent<PhotonView>().ViewID,
           spawnedBall.GetComponent<PhotonView>().ViewID,
           spawnedMarker.GetComponent<PhotonView>().ViewID);

        // Invoke(nameof(PerformToss), 2f);
    }


    [PunRPC]
    public void RPC_SyncNetworkObjects(int batsmanViewID, int bowlerViewID, int ballViewID, int markerViewID)
    {
        // Assign the networked GameObjects for ALL clients using PhotonView IDs
        spawnedBatsman = PhotonView.Find(batsmanViewID)?.gameObject;
        spawnedBowler = PhotonView.Find(bowlerViewID)?.gameObject;
        spawnedBall = PhotonView.Find(ballViewID)?.gameObject;
        spawnedMarker = PhotonView.Find(markerViewID)?.gameObject;

        if (spawnedBatsman != null && spawnedBowler != null && spawnedBall != null && spawnedMarker != null)
        {
            Debug.Log("Network objects assigned on all clients.");

            // Initialize components on all clients after syncing
            if (spawnedBatsman.TryGetComponent<BatsmanController>(out BatsmanController batsmanController))
            {
                batsmanController.InitBatsman(batsmanJoystick, batsmanSlider, straightArea, backDefenceArea, frontDefenceArea, legArea);
                batsmanCameraCin.Follow = batsmanController.cameraTarget;
            }

            if (spawnedBowler.TryGetComponent<BowlerController>(out BowlerController bowlerController))
            {
                bowlerController.InitBowler(markerCamera, bowlerCamera, bowlCamera, spawnedBall, spawnedMarker.transform, bowlerSlider, sliderFillImage, startBowlButton, markerLockButton);
                bowlerCameraCin.Follow = bowlerController.cameraTarget;
            }
            if (spawnedMarker.TryGetComponent<ParticleController>(out ParticleController marker))
            {

                //markerCameraCin.Follow = marker.transform;
                marker.InitMarker(markerLockButton);
            }
            bowlCameraCin.Follow = spawnedBall.transform;

            fielderController.InitFielder(spawnedBall);

        }
        else
        {
            Debug.LogError("Failed to assign all network objects on this client.");
        }
    }



    public void PerformToss()
    {
        if (tossCompleted) return;

        int tossResult = Random.Range(0, 2); // Randomly choose 0 or 1
        //tossWinner = PhotonNetwork.PlayerList[tossResult]; // Get the toss-winning player
        tossWinner = PhotonNetwork.MasterClient;
        //string role = (tossWinner == PhotonNetwork.LocalPlayer) ? "Bat" : "Bowl";
        photonView.RPC(nameof(AnnounceTossWinner), RpcTarget.AllBuffered, tossWinner.NickName);
    }

    /* [PunRPC]
     private void AnnounceTossWinner(string winnerName)
     {
         statusText.text = $"{winnerName} won the toss!";
         if (PhotonNetwork.LocalPlayer.NickName == winnerName)
         {
             ShowTossOptions(); // Let the winner pick batting or bowling
         }
     }*/
    [PunRPC]
    private void AnnounceTossWinner(string winnerName)
    {
        bool isLocalPlayerBatsman = (PhotonNetwork.LocalPlayer.NickName == winnerName);
        string localRole = isLocalPlayerBatsman ? "Bat" : "Bowl";

        MultiplayerStatusUI.Instance.ShowTossResult(winnerName, localRole);
    }

    private void ShowTossOptions()
    {
        statusText.text += "\nSelect you role";
        selectionPanel.SetActive(true);
        winToss = true;
    }

    private bool winToss;
    private void Update()
    {
        if (!winToss) return;

        if (Input.GetKeyDown(KeyCode.B)) // Select to Bat
        {
            photonView.RPC(nameof(SetRoles), RpcTarget.AllBuffered, true); // Batsman = true
        }
        else if (Input.GetKeyDown(KeyCode.C)) // Select to Bowl
        {
            photonView.RPC(nameof(SetRoles), RpcTarget.AllBuffered, false); // Bowler = false
        }

    }

    [PunRPC]
    private void SetRoles(bool isBatsmanFirst)
    {
        // isBatsman = isBatsmanFirst == (PhotonNetwork.LocalPlayer == tossWinner);
        isBatsman = MultiplayerStatusUI.Instance.batsman;
        if (isBatsman)
        {
            statusText.text = "You are the batsman!";
        }
        else
        {
            statusText.text = "You are the bowler!";
        }

        selectionPanel.SetActive(false);
        statusText.gameObject.SetActive(false);
        // Enable relevant controllers based on role
        GameManager.Instance.AssignReset();
        EnableRelevantController();
    }

    public void EnableRelevantController()
    {


        if (isBatsman)
        {
            GameManager.Instance.EnableBatsman();
            TakeOwnership(spawnedBatsman);
            TakeOwnership(spawnedBall);
            CricketScoreManager.Instance.isBatting = true;

        }
        else
        {
            GameManager.Instance.EnableBowler();
            TakeOwnership(spawnedBowler);
            TakeOwnership(spawnedMarker);
            CricketScoreManager.Instance.isBatting = false;
        }

    }

    public void StartGame()
    {
        animatedCamera.SetActive(false);
        mainCamera.SetActive(true);
    }
    public void TransferBowlOwnerShip()
    {
        if (isBatsman)
        {
            TakeOwnership(spawnedBall);

        }
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

    [PunRPC]
    public void SwapRoles()
    {
        isBatsman = !isBatsman;
        CricketScoreManager.Instance.isBatting = !CricketScoreManager.Instance.isBatting;
        Debug.LogError("Swapped");
        // Swap roles visually
        if (isBatsman)
        {
            statusText.text = "You are now the batsman!";
           // GameManager.Instance.EnableBatsman();
            TakeOwnership(spawnedBatsman);
            TakeOwnership(spawnedBall);
        }
        else
        {
            statusText.text = "You are now the bowler!";
            //GameManager.Instance.EnableBowler();
            TakeOwnership(spawnedBowler);
            TakeOwnership(spawnedMarker);
        }

        statusText.gameObject.SetActive(false);


       // Invoke(nameof(ResetDelay), 3f);

    }

    public void ResetDelay()
    {
        GameManager.Instance.GameReset();
    }

    public void RequestSwapRoles()
    {
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(SwapRoles), RpcTarget.AllBuffered);
    }

    public void ChooseBatting()
    {
        photonView.RPC(nameof(SetRoles), RpcTarget.AllBuffered, true); // Batsman = true
    }

    public void ChooseBowling()
    {
        photonView.RPC(nameof(SetRoles), RpcTarget.AllBuffered, false); // Bowler = false
    }
    public GameObject frontButton,backButton;

    public void ChangeToBackStance()
    {
        frontButton.SetActive(true);
        backButton.SetActive(false);
        spawnedBatsman.GetComponent<BatsmanController>().ShiftStance(false);
    }


    public void ChangeToFrontStance()
    {
        frontButton.SetActive(false);
        backButton.SetActive(true);
        spawnedBatsman.GetComponent<BatsmanController>().ShiftStance(true);

    }



}
