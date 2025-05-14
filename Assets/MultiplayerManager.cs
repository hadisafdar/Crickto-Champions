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

   
    [PunRPC]
    private void AnnounceTossWinner(string winnerName)
    {
        bool isLocalPlayerBatsman = (PhotonNetwork.LocalPlayer.NickName == winnerName);
        string localRole = isLocalPlayerBatsman ? "Bat" : "Bowl";

        MultiplayerStatusUI.Instance.ShowTossResult(winnerName, localRole);
    }

   


    [PunRPC]
    private void SetRoles(bool isBatsmanFirst)
    {
        // isBatsman = isBatsmanFirst == (PhotonNetwork.LocalPlayer == tossWinner);
      /*  isBatsman = MultiplayerStatusUI.Instance.batsman;
        if (isBatsman)
        {
            statusText.text = "You are the batsman!";
        }
        else
        {
            statusText.text = "You are the bowler!";
        }

       
        statusText.gameObject.SetActive(false);
        // Enable relevant controllers based on role
        GameManager.Instance.AssignReset();
        EnableRelevantController();*/
    }

    public void EnableRelevantController()
    {


       

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
           
        }
        else
        {
            statusText.text = "You are now the bowler!";
            
        }

        statusText.gameObject.SetActive(false);



    }

    public void ResetDelay()
    {
        //GameManager.Instance.GameReset();
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
        //spawnedBatsman.GetComponent<BatsmanController>().ShiftStance(false);
    }


    public void ChangeToFrontStance()
    {
        frontButton.SetActive(false);
        backButton.SetActive(true);
        //spawnedBatsman.GetComponent<BatsmanController>().ShiftStance(true);

    }



}
