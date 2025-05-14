using Photon.Pun;
using System;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Central game manager for multiplayer cricket:
/// handles cameras, UI panels, ball‐hit effects,
/// and full game reset logic.
/// </summary>
public class GameManager : MonoBehaviourPun
{
    #region Singleton

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    #endregion

    #region Inspector Fields


    [Header("Cinemachine")]
    public CinemachineBasicMultiChannelPerlin cinemachineNoise;

    [Header("Reset Settings")]
    public FielderController[] fielders;
    public WicketsOutAnimation wicket;



    [Header("GameObjects Settings")]
    public GameObject batsman;
    public GameObject bowler;
    public GameObject marker;
    public GameObject batsmanCamera;
    public GameObject bowlerCamera;
    public GameObject batsmanCanvas;
    public GameObject bowlerCanvas;
    public GameObject inGameScoreUI;
    public GameObject endGamePanel;
    public TextMeshProUGUI endGameText;
    #endregion

    #region Public State & Events

    public static bool IsGameReset = false;
    public static event Action OnResetGame;

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        fielders = FindObjectsByType<FielderController>(FindObjectsSortMode.None);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            photonView.RPC(nameof(RPC_ResetGame), RpcTarget.All);
    }

    #endregion



    #region Ball Hit Sequence

    public void BallHit()
    {
        Ground.BowlHit = true;
        Debug.Log("Ball Hit");
        StartCoroutine(BallHitRoutine());
    }

    private IEnumerator BallHitRoutine()
    {
        yield return new WaitForSecondsRealtime(0.1f);

        cinemachineNoise.AmplitudeGain = 0.3f;
        cinemachineNoise.FrequencyGain = 0.5f;
        yield return new WaitForSeconds(0.2f);

        cinemachineNoise.AmplitudeGain = 0f;
        cinemachineNoise.FrequencyGain = 0f;

        yield return new WaitForSeconds(0.2f);
        CameraManager.Instance.bowlCamera.gameObject.SetActive(true);
        //CricketCinematicCam.Instance.TriggerCinematics();
        photonView.RPC(nameof(CameraCinematics), RpcTarget.All);
    }

    #endregion

    #region Reset Logic

    public void GameReset()
    {
        //PerformLocalReset();
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(RPC_ResetGame), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_ResetGame()
    {
        PerformLocalReset();
    }
    [PunRPC]
    private void CameraCinematics()
    {
        CricketCinematicCam.Instance.TriggerCinematics();
    }
    private void PerformLocalReset()
    {
        IsGameReset = true;
        FadeImageTransition.Instance.FadeInOut(() =>
        {
            ApplyReset();
            StartCoroutine(ResetFlagCoroutine());
        });

    }

    private void ApplyReset()
    {

        foreach (var f in fielders)
            f.FielderReset();


        Ground.BowlHit = false;
        Ground.canSixer = true;
        MissBall.IsBallMiss = false;
        SpinBowling.hasHitGround = false;

        if (CricketGameManager.Instance.isBatsman)
        {
            
            CameraManager.Instance.bowlCamera.gameObject.SetActive(false);
            CameraManager.Instance.bowlCamera.gameObject.SetActive(false);
            CameraManager.Instance.batsmanCamera.gameObject.SetActive(true);
        }
        else if(CricketGameManager.Instance.useBot)
        {

        }
        else
        {

        }
        CricketCinematicCam.Instance.DisableCinematics();
        wicket.ResetWicketsAnimation();
        batsman.GetComponent<BatsmanController>().ResetBatsman();
        bowler.GetComponent<BowlerController>().ResetBowler();
        inGameScoreUI.gameObject.SetActive(true);
        CircularBoundaryManager.Instance._outerLogged = false;
        CircularBoundaryManager.Instance._hasScored = false;
        CameraManager.Instance.ResetCamera();
        OnResetGame?.Invoke();
    }

    private IEnumerator ResetFlagCoroutine()
    {
        yield return new WaitForSeconds(1f);
        IsGameReset = false;
    }

    #endregion

    public void GameEnd(bool isWin, bool isDraw = false, MatchEndPanel matchEndPanel = null)
    {


        if (isDraw)
        {
            Debug.Log("Game Ended: Draw!");
            // TODO: Activate your draw UI, e.g.:
             endGamePanel.SetActive(true);
             endGameText.text = "Match Drawn!";
           matchEndPanel.InitMatchEnd(true);

        }
        else if (isWin)
        {
            Debug.Log("Game Ended: Victory!");
            // TODO: Activate your win UI, e.g.:
             endGamePanel.SetActive(true);
             endGameText.text = "You Win!";
             matchEndPanel.InitMatchEnd(true);
        }
        else
        {
            Debug.Log("Game Ended: Defeat.");
            // TODO: Activate your loss UI, e.g.:
             endGamePanel.SetActive(true);
             endGameText.text = "You Lose!";
             matchEndPanel.InitMatchEnd(true);
        }

        // Additional logic can be added here (e.g., disabling player controls,
        // pausing the game, or transitioning to another scene).
        //PhotonNetwork.Disconnect();
    }

}
