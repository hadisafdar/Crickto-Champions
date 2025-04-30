using DG.Tweening;
using Photon.Pun;
using SimpleInputNamespace;
using UnityEngine;
using UnityEngine.UI; // For using the Slider component

public class BatsmanController : MonoBehaviourPun
{
    private Animator animator;

    public float chargeInput = 0f; // Current charge value
    public float chargeValue = 0f; // Current charge value
    private bool isCharging = false;

    public float horizontalInput = 0f; // Tracks Horizontal axis input
    public float verticalInput = 0f;   // Tracks Vertical axis input

    public float moveSpeed = 5f; // Speed of left and right movement
    private Vector3 moveDirection = Vector3.zero; // Movement direction
    public Joystick joystick;

    public Slider moveSlider; // Reference to the slider in the canvas
    public float leftLimit = -2f; // Left movement limit
    public float rightLimit = 2f; // Right movement limit

    public float horizontalValue;
    public float verticalValue;
    public BoxCollider battingTrigger;

    public Transform cameraTarget;




    [Header("Target Areas")]

    public BallTriggerLauncher ballTriggerLauncher;

    public TargetArea straightArea;
    public TargetArea coverDriveArea;
    public TargetArea frontDefenceArea;
    public TargetArea backDefenceArea;


    public void InitBatsman(Joystick batsmanJoystick, Slider batsmanSlider, TargetArea straightArea, TargetArea backArea, TargetArea defenceArea, TargetArea legDriveTargetArea)
    {
        joystick = batsmanJoystick;
        this.moveSlider = batsmanSlider;
        // Optional: Add a listener to detect slider value changes

        this.straightArea = straightArea;
        coverDriveArea = legDriveTargetArea;
        frontDefenceArea = defenceArea;
        backDefenceArea = backArea;
        moveSlider.onValueChanged.AddListener(OnSliderValueChanged);
        moveSlider.value = 0.5f;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();

    }

    private void Update()
    {

        if (!MultiplayerManager.Instance.isBatsman) return;

        // Handle input and animations for batting
        horizontalInput = SimpleInput.GetAxis("BatHorizontal");
        verticalInput = SimpleInput.GetAxis("BatVertical");

        HandleInput();


        // Handle movement based on slider value
        //HandleSliderMovement();
    }

    private void HandleInput()
    {
        if (verticalInput < -0.1f || verticalInput > 0.1f || horizontalInput < -0.1f || horizontalInput > 0.1f) // Downward input
        {
            isCharging = true;

            chargeInput = verticalInput - horizontalInput;
            chargeInput = Mathf.Clamp01(Mathf.Abs(chargeInput));

            animator.speed = chargeInput; // Animation speed proportional to charge
            animator.Play("Charging", 0, chargeInput); // Play Charging animation at charge value

            if (joystick.joystickHeld)
            {
                horizontalValue = horizontalInput;
                verticalValue = verticalInput;
                chargeValue = chargeInput;
            }
        }
        else if (isCharging && !joystick.joystickHeld) // When input is released
        {
            isCharging = false;

            DetermineShot();
            chargeInput = 0f;
        }
    }



    private void HandleSliderMovement()
    {
        // Map slider value (0 to 1) to the movement range (leftLimit to rightLimit)
        float targetXPosition = Mathf.Lerp(leftLimit, rightLimit, moveSlider.value);

        // Smoothly move the position using DOTween
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, targetXPosition);
        transform.DOMove(targetPosition, 0.5f).SetEase(Ease.OutQuad);

        // Update animations based on movement
        if (targetPosition.z > transform.position.z)
        {
            animator.Play("MoveRight");
        }
        else if (targetPosition.z < transform.position.z)
        {
            animator.Play("MoveLeft");
        }
    }



    private void OnSliderValueChanged(float value)
    {
        // Optional: React to slider value changes in real-time
        HandleSliderMovement();
    }

    private void MoveLeft()
    {
        moveDirection = Vector3.left;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        animator.Play("MoveLeft");
    }

    private void MoveRight()
    {
        moveDirection = Vector3.right;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        animator.Play("MoveRight");
    }

    private void DetermineShot()
    {



        if (isFrontStance)
        {
            if (horizontalValue > 0.5f && verticalValue > -0.1f && verticalValue < 0.5f)
            {
                if (chargeValue > 0.95f)
                {

                    PlayShot("LegDrive");
                    
                    //ballTriggerLauncher.targetArea = coverDriveArea;
                    SetTargetArea("CoverDriveArea");

                }
                else if (chargeValue > 0.7f)
                {
                    PlayShot("LegDrive");
                    //ballTriggerLauncher.targetArea = coverDriveArea;
                    SetTargetArea("CoverDriveArea");
                }
                else
                {
                    PlayShot("LegGlance");
                    //ballTriggerLauncher.targetArea = coverDriveArea;
                    SetTargetArea("CoverDriveArea");
                }
            }
            else if (horizontalValue >= 0.5f && verticalValue >= 0.5f)
            {
                //ballTriggerLauncher.targetArea = backDefenceArea;
                SetTargetArea("BackDefenceArea");
                PlayShot("Glance");
            }
            else if (horizontalValue > 0.5f && verticalValue >= -0.5f)
            {
                PlayShot("LoftedDrive");
                //ballTriggerLauncher.targetArea = straightArea;
                SetTargetArea("StraightArea");
            }
            else if (verticalValue > 0.5f)
            {
                //ballTriggerLauncher.targetArea = frontDefenceArea;
                BallLaunchSettings.Instance.forceMultiplier = 0.1f;
                BallLaunchSettings.Instance.launchHeight = 0.1f;
                SetTargetArea("FrontDefenceArea");
                PlayShot("Defence");


            }
            else
            {
                //ballTriggerLauncher.targetArea = straightArea;
                PlayShot("StraightDrive");
                SetTargetArea("StraightArea");


            }
        }
        else
        {
            if (horizontalValue > 0.5f && verticalValue > -0.1f && verticalValue < 0.5f)
            {
                if (chargeValue > 0.95f)
                {

                    //PlayShot("LegDrive");
                    PlayShot("Pull");
                    
                    SetTargetArea("CoverDriveArea");

                }
                else if (chargeValue > 0.7f)
                {
                   // PlayShot("LegDrive");
                    PlayShot("Pull");
                    SetTargetArea("CoverDriveArea");
                }
                else
                {
                   // PlayShot("LegGlance");
                    PlayShot("Pull");
                    SetTargetArea("CoverDriveArea");
                }
            }
            else if (horizontalValue >= 0.5f && verticalValue >= 0.5f)
            {
              
               // SetTargetArea("BackDefenceArea");
               // PlayShot("Glance");
            }
            else if (horizontalValue > 0.5f && verticalValue >= -0.5f)
            {
                //PlayShot("LoftedDrive");
                //ballTriggerLauncher.targetArea = straightArea;
               // SetTargetArea("StraightArea");
            }
            else if (verticalValue > 0.5f)
            {
                
               // BallLaunchSettings.Instance.forceMultiplier = 0.1f;
               // BallLaunchSettings.Instance.launchHeight = 0.1f;
                //SetTargetArea("FrontDefenceArea");
                //PlayShot("Defence");


            }
            else
            {
               
               // PlayShot("StraightDrive");
                PlayShot("Cut");
                SetTargetArea("StraightArea");


            }
        }
       
    }


    [PunRPC]
    public void SetTargetAreaRPC(string targetAreaName)
    {
        TargetArea selectedTargetArea = null;

        // Match the string with the corresponding TargetArea
        switch (targetAreaName)
        {
            case "StraightArea":
                selectedTargetArea = straightArea;
                break;
            case "CoverDriveArea":
                selectedTargetArea = coverDriveArea;
                break;
            case "FrontDefenceArea":
                selectedTargetArea = frontDefenceArea;
                break;
            case "BackDefenceArea":
                selectedTargetArea = backDefenceArea;
                break;
            default:
                Debug.LogWarning($"Target area '{targetAreaName}' not recognized.");
                break;
        }

        if (selectedTargetArea != null)
        {
            ballTriggerLauncher.targetArea = selectedTargetArea;
            Debug.Log($"Target area set to: {targetAreaName}");
        }
    }

    public void SetTargetArea(string targetAreaName)
    {
        // Call the RPC to synchronize across all clients
        photonView.RPC(nameof(SetTargetAreaRPC), RpcTarget.All, targetAreaName);
    }





    [PunRPC]
    public void PlayShotRPC(string shotName)
    {
        animator.speed = 1f;

        animator.Play(shotName);

    }

    private void PlayShot(string shotName)
    {



        photonView.RPC(nameof(PlayShotRPC), RpcTarget.All, shotName);
    }


    public void BattingTriggerOn()
    {
        //battingTrigger.enabled = true;
        photonView.RPC(nameof(BattingTriggerRpc), RpcTarget.All, true);
    }
    public void BattingTriggerOff()
    {

        //battingTrigger.enabled = false;
        photonView.RPC(nameof(BattingTriggerRpc), RpcTarget.All, false);
    }





    [PunRPC]
    public void BattingTriggerRpc(bool trigger)
    {
        battingTrigger.enabled = trigger;
    }


    private bool isFrontStance = true;


    public void ShiftStance(bool stance)
    {
        isFrontStance = stance;
    }


}
