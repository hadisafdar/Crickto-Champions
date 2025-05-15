using DG.Tweening;
using Photon.Pun;
using SimpleInputNamespace;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls batsman animations, input, movement, shot logic,
/// and optional bot behavior in a multiplayer cricket game.
/// </summary>
public class BatsmanController : MonoBehaviourPun
{
    #region Fields

    private Animator animator;

    [Header("Charge Settings")]
    public float chargeInput;
    public float chargeValue;
    private bool isCharging;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float leftLimit = -2f;
    public float rightLimit = 2f;
    public Slider moveSlider;
    public Joystick joystick;
    private Vector3 moveDirection;

    [Header("Input Values")]
    public float horizontalInput;
    public float verticalInput;
    private float horizontalValue;
    private float verticalValue;

    [Header("Batting")]
    public BoxCollider battingTrigger;
    public Rigidbody ball;

    [Header("Target Areas")]
    public BallTriggerLauncher ballTriggerLauncher;
    public TargetArea straightArea;
    public TargetArea coverDriveArea;
    public TargetArea frontDefenceArea;
    public TargetArea backDefenceArea;

    private bool isFrontStance = true;

    [Header("Bot Settings")]
    public bool isBot = false;
    public Transform markerTransform;
    [Range(-1f, 1f)]
    [Tooltip("Normalized slider offset applied to bot target (−1 to +1).")]
    public float botSliderOffset = 0f;
    public float botSliderSpeed = 0.5f;
    public Transform ballTransform;
    public float botReactionDistance = 1f;
    private bool botShotPlayed = false;

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        animator = GetComponent<Animator>();
        moveSlider.onValueChanged.AddListener(OnSliderValueChanged);
        moveSlider.value = 0.5f;
        GameManager.OnResetGame += ResetBatsman;
        ballTriggerLauncher.ball = this.ball;

    }

    private void OnDisable()
    {
        GameManager.OnResetGame -= ResetBatsman;
    }



    private void Update()
    {
        if (!CricketGameManager.GameStarted) return;
        if (isBot)
        {
            BotBehavior();
            return;
        }

        horizontalInput = SimpleInput.GetAxis("BatHorizontal");
        verticalInput = SimpleInput.GetAxis("BatVertical");
        HandleInput();
    }

    #endregion

    #region Bot Behavior

    private void BotBehavior()
    {
        AdjustSliderTowardsMarker();

        if (ballTransform != null)
        {
            float dist = Vector3.Distance(ballTransform.position, transform.position);

            if (dist <= botReactionDistance && !botShotPlayed)
            {
                PlayRandomShot();
                botShotPlayed = true;
            }
            else if (dist > botReactionDistance)
            {
                botShotPlayed = false;
            }
        }
    }

    #endregion

    #region Input Handling

    private void HandleInput()
    {
        bool significant = Mathf.Abs(verticalInput) > 0.1f ||
                           Mathf.Abs(horizontalInput) > 0.1f;

        if (significant)
        {
            isCharging = true;
            chargeInput = Mathf.Clamp01(Mathf.Abs(verticalInput - horizontalInput));
            animator.speed = chargeInput;
            animator.Play("Charging", 0, chargeInput);

            if (joystick.joystickHeld)
            {
                horizontalValue = horizontalInput;
                verticalValue = verticalInput;
                chargeValue = chargeInput;
            }
        }
        else if (isCharging && !joystick.joystickHeld)
        {
            isCharging = false;
            DetermineShot();
            chargeInput = 0f;
        }
    }

    #endregion

    #region Movement

    private void HandleSliderMovement()
    {
        float targetZ = Mathf.Lerp(leftLimit, rightLimit, moveSlider.value);
        Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, targetZ);
        transform.DOMove(targetPos, 0.5f).SetEase(Ease.OutQuad);

        if (targetZ > transform.position.z)
            animator.Play("MoveRight");
        else if (targetZ < transform.position.z)
            animator.Play("MoveLeft");
    }

    private void OnSliderValueChanged(float value) => HandleSliderMovement();

    #endregion

    #region Shot Determination

    private void DetermineShot()
    {
        if (isFrontStance)
        {
            if (horizontalValue > 0.5f && verticalValue is > -0.1f and < 0.5f)
            {
                PlayShot("LegDrive"); SetTargetArea("CoverDriveArea");
            }
            else if (horizontalValue >= 0.5f && verticalValue >= 0.5f)
            {
                PlayShot("Glance"); SetTargetArea("BackDefenceArea");
            }
            else if (horizontalValue > 0.5f && verticalValue >= -0.5f)
            {
                PlayShot("LoftedDrive"); SetTargetArea("StraightArea");
            }
            else if (verticalValue > 0.5f)
            {
                BallLaunchSettings.Instance.forceMultiplier = 0.1f;
                BallLaunchSettings.Instance.launchHeight = 0.1f;
                PlayShot("Defence"); SetTargetArea("FrontDefenceArea");
            }
            else
            {
                PlayShot("StraightDrive"); SetTargetArea("StraightArea");
            }
        }
        else
        {
            if (horizontalValue > 0.5f && verticalValue is > -0.1f and < 0.5f)
            {
                PlayShot("Pull"); SetTargetArea("CoverDriveArea");
            }
            else
            {
                PlayShot("Cut"); SetTargetArea("StraightArea");
            }
        }
    }

    #endregion

    #region RPC Methods

    [PunRPC]
    private void SetTargetAreaRPC(string name) => SetTargetAreaInternal(name);

    [PunRPC]
    private void PlayShotRPC(string shotName)
    {
        animator.speed = 1f;
        animator.Play(shotName);
    }

    #endregion

    #region Public API

    public void SetTargetArea(string name) =>
        photonView.RPC(nameof(SetTargetAreaRPC), RpcTarget.All, name);

    public void PlayShot(string shotName)
    {
        animator.speed = 1f;
        animator.Play(shotName);
        photonView.RPC(nameof(PlayShotRPC), RpcTarget.All, shotName);
    }

    public void BattingTriggerOn()
    {


        //battingTrigger.enabled = true;
        photonView.RPC(nameof(SetBattingTrigger), RpcTarget.All, true);

    }
    public void BattingTriggerOff()
    {

        //battingTrigger.enabled = false;
        photonView.RPC(nameof(SetBattingTrigger), RpcTarget.All, false);

    }
    public void ShiftStance(bool front) => isFrontStance = front;

    #endregion

    #region Helpers

    private void AdjustSliderTowardsMarker()
    {
        if (markerTransform == null) return;

        float markerNorm = Mathf.InverseLerp(
            leftLimit,
            rightLimit,
            markerTransform.position.z
        );

        float desired = Mathf.Clamp01(markerNorm + botSliderOffset);

        moveSlider.value = Mathf.MoveTowards(
            moveSlider.value,
            desired,
            botSliderSpeed * Time.deltaTime
        );
    }

    private void PlayRandomShot()
    {
        // Define shots with weights
        (string shot, string area, int weight)[] weightedOptions = {
            ("StraightDrive", "StraightArea", 5),
            ("LegDrive",      "CoverDriveArea", 4),
            ("Glance",        "BackDefenceArea", 1),
            ("LoftedDrive",   "StraightArea",    2),
            ("Defence",       "FrontDefenceArea",1),
            ("Pull",          "CoverDriveArea",  1),
            ("Cut",           "StraightArea",    1)
        };

        // Sum weights
        int totalWeight = 0;
        foreach (var opt in weightedOptions)
            totalWeight += opt.weight;

        // Pick random based on weight
        int rnd = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var opt in weightedOptions)
        {
            cumulative += opt.weight;
            if (rnd < cumulative)
            {
                PlayShot(opt.shot);
                SetTargetAreaInternal(opt.area);
                break;
            }
        }
    }

    private void SetTargetAreaInternal(string name)
    {
        TargetArea area = name switch
        {
            "StraightArea" => straightArea,
            "CoverDriveArea" => coverDriveArea,
            "FrontDefenceArea" => frontDefenceArea,
            "BackDefenceArea" => backDefenceArea,
            _ => null
        };
        if (area != null)
            ballTriggerLauncher.targetArea = area;
    }
    public void ResetBatsman()
    {
        
        photonView.RPC(nameof(ResetBatsmanRPC), RpcTarget.All);

    }

    [PunRPC]
    private void ResetBatsmanRPC()
    {
        moveSlider.value = 0.5f;
        botShotPlayed = false;
       
    }


    [PunRPC]
    private void SetBattingTrigger(bool trigger)
    {
        battingTrigger.enabled = trigger;
    }
    #endregion
}
