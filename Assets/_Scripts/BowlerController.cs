using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages bowler behavior, UI slider control, ball launch, bot automation,
/// and network synchronization in a multiplayer cricket game.
/// </summary>
public class BowlerController : MonoBehaviourPun
{
    #region Fields

    public GameObject particleCamera;
    public GameObject bowlerCamera;
    public GameObject bowlCamera;
    public Animator bowlerAnimator;
    public GameObject ball;
    public Transform particleEffect;
    public float scalingFactor = 0.9f;
    public static event Action OnBallLaunched;
    public Vector3 ballStartingPosition;
    public Transform ballParent;

    public Slider powerSlider;
    public float sliderSpeed = 1f;
    public Button startBowlButton;
    public Button markerLockButton;

    public bool isBot = false;
    public float botMinLockDelay = 1f;
    public float botMaxLockDelay = 3f;

    private bool isSliderLocked = false;
    private bool botHasLockedSlider = false;

    public Transform cameraTarget;
    public Vector3 ballOffset;
    public Quaternion startingRotation;

    public bool isSpin;
    public bool isSwing;
    public SpinType spinType;

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        ParticleController.OnBowlingTriggered += OnMarkerLocked;
        GameManager.OnResetGame += ResetBowler;

        
    }

    public void SetBowler()
    {
        photonView.RPC(nameof(ResetBowl), RpcTarget.All);
    }

    private void OnDisable()
    {
        ParticleController.OnBowlingTriggered -= OnMarkerLocked;
        GameManager.OnResetGame -= ResetBowler;
    }

    private void Update()
    {
        if (!isSliderLocked && powerSlider != null)
        {
            MoveSlider();
            UpdateSliderColor();
        }
    }

    #endregion

    #region Slider Control

    private void MoveSlider()
    {
        float newValue = powerSlider.value + sliderSpeed * Time.deltaTime;
        if (newValue >= powerSlider.maxValue || newValue <= powerSlider.minValue)
        {
            sliderSpeed = -sliderSpeed;
        }
        powerSlider.value = Mathf.Clamp(newValue, powerSlider.minValue, powerSlider.maxValue);
    }

    private void UpdateSliderColor()
    {
        float sliderValue = powerSlider.value;
        foreach (var range in BallLaunchSettings.Instance.sliderRanges)
        {
            if (sliderValue >= range.minValue && sliderValue <= range.maxValue)
            {
                return;
            }
        }
    }

    public void LockSliderAndStartBowling()
    {
        isSliderLocked = true;
        BallLaunchSettings.Instance.AdjustLaunchSettings(powerSlider.value);
        StartBowlingSequence();
    }

    #endregion

    #region Marker Lock

    private void OnMarkerLocked()
    {
        if (isBot)
        {
            float delay = UnityEngine.Random.Range(botMinLockDelay, botMaxLockDelay);
            Invoke(nameof(BotLockSliderAndStartBowling), delay);
            return;
        }
        particleCamera.SetActive(false);
        bowlerCamera.SetActive(true);
        powerSlider.gameObject.SetActive(true);
        startBowlButton.gameObject.SetActive(true);
        markerLockButton.gameObject.SetActive(false);
    }

    #endregion

    #region Bowling Sequence

    public void StartBowlingSequence()
    {
        PlayShot("Bowl");
    }

    private void PlayShot(string shotName)
    {
        bowlerAnimator.Play(shotName);
        photonView.RPC(nameof(PlayShotRPC), RpcTarget.All, shotName);
    }

    [PunRPC]
    private void PlayShotRPC(string shotName)
    {
        bowlerAnimator.Play(shotName);
    }

    #endregion

    #region Ball Launch

    public void LaunchBall()
    {
        if (ball == null || particleEffect == null) return;

        if (!isBot && !CricketGameManager.Instance.isBatsman)
        {
            particleCamera.SetActive(false);
            bowlerCamera.SetActive(false);
            bowlCamera.SetActive(true);
            CameraManager.Instance.LaunchBallCamera();
        }

        Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
        SpinBowling spinBowling = ball.GetComponent<SpinBowling>();
        if (ballRigidbody != null)
        {
            spinBowling.spinType = isSpin ? spinType : SpinType.NoSpin;
            float adjustedFlightTime = isSpin ? 1.5f : 1.2f;
            Vector3 launchForce = CalculateAdjustedLaunchForce(ball.transform.position, particleEffect.position, adjustedFlightTime);


            if (photonView.IsMine)
                photonView.RPC(nameof(DoLaunchRPC), RpcTarget.All, launchForce);
        }

        OnBallLaunched?.Invoke();
        bowlerAnimator.SetBool("Idle", true);
    }

    [PunRPC]
    private void DoLaunchRPC(Vector3 launchForce)
    {
        ball.transform.parent = null;
        ball.GetComponent<TrailRenderer>().enabled = true;
        var rb = ball.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = launchForce;
        var spin = ball.GetComponent<SpinBowling>();
        spin.spinType = isSpin ? spinType : SpinType.NoSpin;
    }

    #endregion

    #region Bot Behavior

    private void BotLockSliderAndStartBowling()
    {
        if (botHasLockedSlider) return;
        powerSlider.value = UnityEngine.Random.Range(powerSlider.minValue, powerSlider.maxValue);
        LockSliderAndStartBowling();
        botHasLockedSlider = true;
    }

    #endregion

    #region Catch & Reset

    public void CatchBall(Vector3 position, Vector3 scorePos)
    {
        //CricketScoreManager.Instance.AddScore(Boundary.CurrentScore);
        PlayAnimation("Catch");

        var ballRigidbody = ball.GetComponent<Rigidbody>();
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.linearDamping = 0f;

        CircularBoundaryManager.Instance.EvaluateRuns(scorePos);
        ball.transform.SetParent(ballParent);
        ball.transform.localPosition = ballStartingPosition;
        ball.transform.localRotation = Quaternion.identity;
    }

    public void PlayAnimation(string animationName)
    {
        photonView.RPC(nameof(PlayAnimationRpc), RpcTarget.All, animationName);
    }

    [PunRPC]
    private void PlayAnimationRpc(string animationName)
    {
        bowlerAnimator.Play(animationName);
    }

    public void ResetBowler()
    {
        Debug.Log("BowlRest");
        photonView.RPC(nameof(ResetBowl), RpcTarget.All);
        bowlerAnimator.SetBool("Idle", false);
        powerSlider.gameObject.SetActive(false);
        startBowlButton.gameObject.SetActive(false);
        bowlerCamera.gameObject.SetActive(false);
        PlayAnimation("Start");

        if (isBot || CricketGameManager.Instance.isBatsman) return;



        particleCamera.gameObject.SetActive(true);
        markerLockButton.gameObject.SetActive(true);
    }


    [PunRPC]
    private void ResetBowl()
    {
        transform.rotation = startingRotation;
        powerSlider.value = powerSlider.minValue;
        isSliderLocked = false;
        botHasLockedSlider = false;

        var rb = ball.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        ball.transform.SetParent(ballParent);
        ball.transform.localPosition = ballStartingPosition;
        ball.transform.localRotation = Quaternion.identity;
        botHasLockedSlider = false;
        photonView.RPC(nameof(SyncBallTransform), RpcTarget.All, ball.transform.position, ball.transform.rotation);
    }

    [PunRPC]
    private void SyncBallTransform(Vector3 position, Quaternion rotation)
    {
        ball.transform.localPosition = ballStartingPosition;
        ball.transform.localRotation = Quaternion.identity;
    }

    #endregion

    #region Helpers

    private Vector3 CalculateAdjustedLaunchForce(Vector3 startPosition, Vector3 targetPosition, float flightTime)
    {
        Vector3 displacement = targetPosition - startPosition;
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0, displacement.z);
        float gravity = Mathf.Abs(Physics.gravity.y);

        float verticalVelocity = (displacement.y / flightTime) + (0.5f * gravity * flightTime);
        Vector3 horizontalVelocity = horizontalDisplacement / flightTime;

        return horizontalVelocity + Vector3.up * verticalVelocity;
    }

    #endregion
}
