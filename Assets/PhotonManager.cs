using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // Import DoTween
using UnityEngine.UI;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    #region Singleton
    public static PhotonManager Instance;

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
    #endregion




    private void Start()
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "asia";

        ConnectToPhotonServer();
    }

    private void ConnectToPhotonServer()
    {
        PopupManager.Instance.ShowLoadingBar();
        PopupManager.Instance.UpdateLoadingBarText("Connecting to Server");
        PhotonNetwork.ConnectUsingSettings();

    }

    public override void OnConnectedToMaster()
    {
        PopupManager.Instance.UpdateLoadingBarText("Connected to Server");
        PopupManager.Instance.HideLoadingBar(OnLobbyJoinedCoroutine);

        Debug.Log($"Connected to Photon Master Server. Region: {PhotonNetwork.CloudRegion}");
    }



    private void OnLobbyJoinedCoroutine()
    {

        SceneManager.LoadScene("Login");

    }





}
