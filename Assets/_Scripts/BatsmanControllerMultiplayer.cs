using DG.Tweening;
using Photon.Pun;
using SimpleInputNamespace;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI; // For using the Slider component

public class BatsmanControllerMultiplayer : MonoBehaviourPun
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





    [Header("Target Areas")]

    public BallTriggerLauncher ballTriggerLauncher;

    public TargetArea straightArea;
    public TargetArea coverDriveArea;
    public TargetArea frontDefenceArea;
    public TargetArea backDefenceArea;




    private void Start()
    {
        animator = GetComponent<Animator>();

        // Optional: Add a listener to detect slider value changes
        moveSlider.onValueChanged.AddListener(OnSliderValueChanged);
        moveSlider.value = 0.5f;
    }

    private void Update()
    {
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

        if (horizontalValue > 0.5f && verticalValue > -0.1f && verticalValue < 0.5f)
        {
            if (chargeValue > 0.95f)
            {

                PlayShot("LegDrive");
                ballTriggerLauncher.targetArea = coverDriveArea;

            }
            else if (chargeValue > 0.7f)
            {
                PlayShot("LegDrive");
                ballTriggerLauncher.targetArea = coverDriveArea;
            }
            else
            {
                PlayShot("LegGlance");
                ballTriggerLauncher.targetArea = coverDriveArea;
            }
        }
        else if (horizontalValue >= 0.5f && verticalValue >= 0.5f)
        {
            ballTriggerLauncher.targetArea = backDefenceArea;
            PlayShot("Glance");
        }
        else if (horizontalValue > 0.5f && verticalValue >= -0.5f)
        {
            PlayShot("LoftedDrive");
            ballTriggerLauncher.targetArea = straightArea;
        }
        else if (verticalValue > 0.5f)
        {
            ballTriggerLauncher.targetArea = frontDefenceArea;
            BallLaunchSettings.Instance.forceMultiplier = 0.1f;
            BallLaunchSettings.Instance.launchHeight = 0.1f;
            PlayShot("Defence");
        }
        else
        {
            ballTriggerLauncher.targetArea = straightArea;
            PlayShot("StraightDrive");
        }
    }

    private void PlayShot(string shotName)
    {
        animator.speed = 1f;
        animator.Play(shotName);
    }


    public void BattingTriggerOn()
    {
        battingTrigger.enabled = true;
    }
    public void BattingTriggerOff()
    {

        battingTrigger.enabled = false;
    }


    // Function to call the RPC and send a message to all clients
    public void PlayShotNetwork(string message)
    {
        photonView.RPC(nameof(RPC_PlayAnimation), RpcTarget.All, message);
    }

    // The RPC function that will run on all clients
    [PunRPC]
    private void RPC_PlayAnimation(string message)
    {
        PlayShot(message);
    }
}
