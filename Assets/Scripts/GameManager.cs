
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for scene management

public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;

        // Load roles from PlayerPrefs or set default values
        isPlayerBatsman = PlayerPrefs.GetInt("IsPlayerBatsman", 1) == 1;
        isPlayerBowler = PlayerPrefs.GetInt("IsPlayerBowler", 0) == 1;
    }

    public GameObject startingBatsmanCamera;
    public GameObject startingBowlerCamera;
    public GameObject batsmanPanel;
    public GameObject bowlerPanel;
    public bool isPlayerBatsman = true;
    public bool isPlayerBowler;
    public GameObject bowlCamera;
    public GameObject ResetCamera;
    public Transform batsmanCameraTarget;
    public CinemachineBasicMultiChannelPerlin cinemachineNoise;
    


    public GameObject resetPanel;
    public Button resetButton;
    public bool isSinglePlayer;

    public static bool IsGameReset = false;

    public static event Action OnResetGame;


    private List<int> currentOverScores = new List<int>();
    private int ballCount = 0;
    private int totalScore = 0;

    public GameObject scoreUI;

    public static int score;


    public FielderController[] fielders;
    public void Start()
    {
       /* if (isPlayerBatsman)
        {
            startingBatsmanCamera.SetActive(true);
            startingBowlerCamera.SetActive(false);
            batsmanPanel.SetActive(true);
            bowlerPanel.SetActive(false);
        }
        else if (isPlayerBowler)
        {
            startingBatsmanCamera.SetActive(false);
            startingBowlerCamera.SetActive(true);
            batsmanPanel.SetActive(false);
            bowlerPanel.SetActive(true);
        }*/

        fielders = FindObjectsByType<FielderController>(FindObjectsSortMode.None);
       
    }
    public void RecordScore(int score)
    {
        if (ballCount < 6)
        {
            // Add the score for the current ball
            currentOverScores.Add(score);
            totalScore += score;
            ballCount++;

            Debug.Log($"Ball {ballCount}: Scored {score} runs. Total Score: {totalScore}");

            // Check if the over is complete
            if (ballCount == 6)
            {
                Debug.Log("Over complete!");
                EndOver();
            }
        }
        else
        {
            Debug.LogWarning("Over already completed. Start a new over.");
        }
    }

    private void EndOver()
    {
        // Perform any end-of-over logic here (e.g., display total score)
        Debug.Log($"Over completed! Total Score: {totalScore}");

        // Reset for the next over
        ResetScores();
    }

    private void ResetScores()
    {
        currentOverScores.Clear();
        ballCount = 0;
    }
    public void AssignReset()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            resetPanel.SetActive(true);
            resetButton.onClick.AddListener(GameReset);
        }
        else
        {
            resetPanel.SetActive(false);
        }
    }

    public void EnableBatsman()
    {
        startingBatsmanCamera.SetActive(true);
        startingBowlerCamera.SetActive(false);
        batsmanPanel.SetActive(true);
        bowlerPanel.SetActive(false);
        scoreUI.SetActive(true);

    }

    public void EnableBowler()
    {
        scoreUI.SetActive(true);
        startingBatsmanCamera.SetActive(false);
        startingBowlerCamera.SetActive(true);
        batsmanPanel.SetActive(false);
        bowlerPanel.SetActive(true);
    }

    // Function to restart the scene and switch roles
    public void RestartAndSwitchRoles()
    {
        // Toggle roles
        isPlayerBatsman = !isPlayerBatsman;
        isPlayerBowler = !isPlayerBowler;

        // Save the updated roles in PlayerPrefs
        PlayerPrefs.SetInt("IsPlayerBatsman", isPlayerBatsman ? 1 : 0);
        PlayerPrefs.SetInt("IsPlayerBowler", isPlayerBowler ? 1 : 0);

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void BallHit()
    {
        Ground.BowlHit = true;

        var batsmanCam = startingBatsmanCamera.GetComponent<CinemachineCamera>();
        batsmanCam.Follow = null;
        batsmanCam.LookAt = GameObject.FindGameObjectWithTag("Ball").transform;

        StartCoroutine(OnBallHitCouroutine());
    }

    private IEnumerator OnBallHitCouroutine()
    {
        // Brief pause (slow-motion effect)
        //Time.timeScale = 0.05f; // Slow down time
        yield return new WaitForSecondsRealtime(0.1f); // Pause for real-time duration

        //Time.timeScale = 1f; // Resume normal time

        // Perform camera shake
        cinemachineNoise.AmplitudeGain = 0.3f;
        cinemachineNoise.FrequencyGain = 0.5f;
        yield return new WaitForSeconds(0.2f); // Duration of the shake

        // Reset camera shake
        cinemachineNoise.AmplitudeGain = 0f;
        cinemachineNoise.FrequencyGain = 0.0f;

        // Switch to bowl camera
        yield return new WaitForSeconds(1f);
        bowlCamera.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            photonView.RPC(nameof(ResetGameRPC), RpcTarget.All);
        }
    }
    [PunRPC]
    public void ResetGameRPC()
    {
        ResetAll();
    }


    public void GameReset()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(ResetGameRPC), RpcTarget.All);
        }
       
    }

    public WicketsOutAnimation wicket;
    public void ResetAll()
    {
        //Reset();
        IsGameReset = true;

        FadeImageTransition.Instance.FadeInOut(null);
        ResetGame();
    }

    private void ResetGame()
    {
        
        if (MultiplayerManager.Instance.isBatsman)
        {
            startingBatsmanCamera.SetActive(true);
            startingBowlerCamera.SetActive(false);
            batsmanPanel.SetActive(true);
            bowlerPanel.SetActive(false);
            bowlCamera.SetActive(false);
            startingBatsmanCamera.GetComponent<CinemachineCamera>().Follow = MultiplayerManager.Instance.spawnedBatsman.GetComponent<BatsmanController>().cameraTarget;
            startingBatsmanCamera.GetComponent<CinemachineCamera>().LookAt = MultiplayerManager.Instance.spawnedBatsman.GetComponent<BatsmanController>().cameraTarget;
        }
        else
        {
            startingBatsmanCamera.SetActive(false);
            startingBowlerCamera.SetActive(true);
            batsmanPanel.SetActive(false);
            bowlerPanel.SetActive(true);

        }
        foreach (FielderController f in fielders)
        {
            f.FielderReset();
        }
        StartCoroutine(ResetCouroutine());
        MultiplayerManager.Instance.spawnedBowler.GetComponent<BowlerController>().ResetBowler();
        MultiplayerManager.Instance.spawnedMarker.GetComponent<ParticleController>().ResetMarker();
        MultiplayerManager.Instance.EnableRelevantController();
        OnResetGame?.Invoke();
        Ground.BowlHit = false;
        Ground.canSixer = true;
        SpinBowling.hasHitGround = false;
        MultiplayerManager.Instance.spawnedBall.GetComponent<TrailRenderer>().enabled = false;
        wicket.ResetWicketsAnimation();
    }


    private IEnumerator ResetCouroutine()
    {
        yield return new WaitForSeconds(1f);
        IsGameReset = false;
    }


    public void SpawnBatsman()
    {

    }
    public void GameEnd(bool isWin, bool isDraw = false, MatchEndPanel matchEndPanel=null)
    {
      

        if (isDraw)
        {
            Debug.Log("Game Ended: Draw!");
            // TODO: Activate your draw UI, e.g.:
            // endGamePanel.SetActive(true);
            // endGameText.text = "Match Drawn!";
           // matchEndPanel.InitMatchEnd(true);

        }
        else if (isWin)
        {
            Debug.Log("Game Ended: Victory!");
            // TODO: Activate your win UI, e.g.:
            // endGamePanel.SetActive(true);
            // endGameText.text = "You Win!";
           // matchEndPanel.InitMatchEnd(true);
        }
        else
        {
            Debug.Log("Game Ended: Defeat.");
            // TODO: Activate your loss UI, e.g.:
            // endGamePanel.SetActive(true);
            // endGameText.text = "You Lose!";
           // matchEndPanel.InitMatchEnd(true);
        }

        // Additional logic can be added here (e.g., disabling player controls,
        // pausing the game, or transitioning to another scene).
    }

}
