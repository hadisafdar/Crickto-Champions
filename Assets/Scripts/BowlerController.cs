using JetBrains.Annotations;
using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;

public class BowlerController : MonoBehaviourPun
{
    [Header("References")]
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
    public Image sliderFillImage;
    public Button startBowlButton, markerLockButton;

    [Header("Bot Settings")]
    public bool isBot = false;               // Toggle to enable bot behavior
    public float botMinLockDelay = 1f;       // Minimum delay before the bot locks the slider
    public float botMaxLockDelay = 3f;       // Maximum delay before the bot locks the slider

    private bool isSliderLocked = false;
    private bool botHasLockedSlider = false;
    public Transform cameraTarget;
    public Vector3 ballOffset;

    public Quaternion startingRotation;
    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        ParticleController.OnBowlingTriggered -= OnMarkerLocked;
    }


    public void InitBowler(GameObject markercamera, GameObject bowlerCamera, GameObject bowlCamera, GameObject ball, Transform markerTransform, Slider powerSlider, Image sliderFillImage, Button startBowlButton, Button markerLockButton)
    {



        particleCamera = markercamera;
        this.bowlerCamera = bowlerCamera;
        this.bowlCamera = bowlCamera;
        this.ball = ball;
        this.particleEffect = markerTransform;
        this.powerSlider = powerSlider;
        this.sliderFillImage = sliderFillImage;
        this.startBowlButton = startBowlButton;
        this.markerLockButton = markerLockButton;
        this.ball.transform.parent = ballParent;
        this.markerLockButton.onClick.AddListener(MultiplayerManager.Instance.spawnedMarker.GetComponent<ParticleController>().TriggerBowling);
        ParticleController.OnBowlingTriggered += OnMarkerLocked;
        this.startBowlButton.onClick.AddListener(LockSliderAndStartBowling);
        startingRotation = transform.rotation;
        GameManager.OnResetGame += ResetBowler;
    }



    public Transform GetBallParent()
    {
        return ballParent;
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (!isSliderLocked && powerSlider != null)
        {
            MoveSlider();
            UpdateSliderColor();
        }
    }

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
                switch (range.zoneColor.ToLower())
                {
                    case "red":
                        //sliderFillImage.color = Color.red;
                        break;
                    case "green":
                       // sliderFillImage.color = Color.green;
                        break;
                    case "blue":
                        //sliderFillImage.color = Color.blue;
                        break;
                    default:
                        //sliderFillImage.color = Color.white;
                        break;
                }
                return;
            }
        }
    }

    private void LockSliderAndStartBowling()
    {
        if (powerSlider != null)
        {
            isSliderLocked = true;


            BallLaunchSettings.Instance.AdjustLaunchSettings(powerSlider.value);
            StartBowlingSequence();
        }
    }

    private void OnMarkerLocked()
    {
        if (isBot)
        {
            // Start a coroutine to simulate bot behavior
            Invoke("BotLockSliderAndStartBowling", UnityEngine.Random.Range(botMinLockDelay, botMaxLockDelay));
            return;
        }
        particleCamera.gameObject.SetActive(false);
        bowlerCamera.gameObject.SetActive(true);
        powerSlider.gameObject.SetActive(true);
        startBowlButton.gameObject.SetActive(true);
        markerLockButton.gameObject.SetActive(false);
    }

    private void StartBowlingSequence()
    {
        if (bowlerAnimator != null)
        {
            //bowlerAnimator.Play("Bowl");

            PlayShot("Bowl");
        }
    }
    [PunRPC]
    public void PlayShotRPC(string shotName)
    {
       

        bowlerAnimator.Play(shotName);
    }

    private void PlayShot(string shotName)
    {



        photonView.RPC(nameof(PlayShotRPC), RpcTarget.All, shotName);
    }
    public bool isSpin;
    public bool isSwing;
    public SpinType spinType;





    public void LaunchBall()
    {
        
        if (ball != null && particleEffect != null)
        {
            if (!isBot & !MultiplayerManager.Instance.isBatsman)
            {

                particleCamera.gameObject.SetActive(false);
                bowlerCamera.gameObject.SetActive(false);
                bowlCamera.gameObject.SetActive(true);
            }

           

            Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
            SpinBowling spinBowling = ball.GetComponent<SpinBowling>();
            if (ballRigidbody != null)
            {
                if (isSpin && spinBowling != null)
                {
                    // Set spin type and enable spin behavior
                    spinBowling.spinType = spinType;
                }
                else
                {
                    spinBowling.spinType = SpinType.NoSpin;
                }


                float adjustedFlightTime = isSpin ? 1.5f : 1.2f; // Increase flight time for spin

                Vector3 launchForce = CalculateAdjustedLaunchForce(ball.transform.position, particleEffect.position, adjustedFlightTime);
                // Reduce the speed if spin is applied



               // ballRigidbody.isKinematic = false;
               // ballRigidbody.useGravity = true;
               // ballRigidbody.AddForce(launchForce, ForceMode.Impulse);

                photonView.RPC(nameof(DoLaunchRPC), RpcTarget.All, launchForce);
            }
            //ball.transform.parent = null;
           // photonView.RPC(nameof(RemoveBallParent), RpcTarget.All);
            OnBallLaunched?.Invoke();
            GetComponent<Animator>().SetBool("Idle", true);
            //MultiplayerManager.Instance.TransferBowlOwnerShip();
            
        }
    }

    [PunRPC]
    void DoLaunchRPC(Vector3 launchForce)
    {
        // 1) un‑parent & enable trail
        ball.transform.parent = null;
        ball.GetComponent<TrailRenderer>().enabled = true;

        // 2) turn physics back on
        var rb = ball.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = launchForce;     // <-- set initial velocity directly
       // rb.AddForce(launchForce, ForceMode.Impulse);
        // 3) (optional) spin logic
        var spin = ball.GetComponent<SpinBowling>();
        if (spin != null)
            spin.spinType = (isSpin ? spinType : SpinType.NoSpin);
    }





    private Vector3 CalculateAdjustedLaunchForce(Vector3 startPosition, Vector3 targetPosition, float flightTime)
    {
        Vector3 displacement = targetPosition - startPosition;

        // Separate horizontal and vertical components
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0, displacement.z);
        float horizontalDistance = horizontalDisplacement.magnitude;

        float gravity = Mathf.Abs(Physics.gravity.y);

        // Calculate the vertical velocity to reach the correct height
        float verticalVelocity = (displacement.y / flightTime) + (0.5f * gravity * flightTime);

        // Calculate the horizontal velocity needed to cover the distance in the given time
        Vector3 horizontalVelocity = horizontalDisplacement / flightTime;

        // Combine horizontal and vertical components
        Vector3 launchVelocity = horizontalVelocity + Vector3.up * verticalVelocity;

        return launchVelocity;
    }



    private void BotLockSliderAndStartBowling()
    {
        if (!botHasLockedSlider)
        {
            // Simulate bot selecting a random slider value
            float randomValue = UnityEngine.Random.Range(powerSlider.minValue, powerSlider.maxValue);
            powerSlider.value = randomValue;



            LockSliderAndStartBowling();
            botHasLockedSlider = true;
        }
    }

    public void CatchBall(Vector3 position)
    {   
        CricketScoreManager.Instance.AddScore(Boundary.CurrentScore);
       
        PlayAnimation("Catch");
        if (ball != null)
        {
            Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
            if (ballRigidbody != null)
            {
                ballRigidbody.linearVelocity = Vector3.zero; // Stop ball movement
                ballRigidbody.angularVelocity = Vector3.zero;
               // ballRigidbody.isKinematic = true;      // Disable physics for resetting position
                ballRigidbody.linearDamping = 0.0f;

            }

            // Reset ball position and parent
            ball.transform.SetParent(ballParent); // Reattach ball to Bowler
            ball.transform.localPosition = ballStartingPosition; // Adjust to starting position
            ball.transform.localRotation = Quaternion.identity;

        }

    }
    public void PlayAnimation(string animationName)
    {
        photonView.RPC(nameof(PlayAnimationRpc), RpcTarget.All, animationName);
    }



    [PunRPC]
    public void PlayAnimationRpc(string animationName)
    {
        bowlerAnimator.Play(animationName);
    }
    public void ResetBowler()
    {

        /*transform.rotation = startingRotation;
        // Reset slider and unlock
        if (powerSlider != null)
        {
            powerSlider.value = powerSlider.minValue; // Reset slider to minimum value
            isSliderLocked = false;
            botHasLockedSlider = false;
        }

        // Reset ball
        if (ball != null)
        {
            Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
            if (ballRigidbody != null)
            {
                ballRigidbody.linearVelocity = Vector3.zero; // Stop ball movement
                ballRigidbody.angularVelocity = Vector3.zero;
                ballRigidbody.isKinematic = true;      // Disable physics for resetting position
                ballRigidbody.linearDamping = 0.0f;

            }

            // Reset ball position and parent
            ball.transform.SetParent(ballParent); // Reattach ball to Bowler
            ball.transform.localPosition = ballStartingPosition; // Adjust to starting position
            ball.transform.localRotation = Quaternion.identity;
        }*/


        // Reset cameras
        // particleCamera.SetActive(true);
        // bowlerCamera.SetActive(false);
        // bowlCamera.SetActive(false);
        photonView.RPC(nameof(ResetBowl), RpcTarget.All);
        // Reset animator
        if (bowlerAnimator != null)
        {
            //bowlerAnimator.Play("Start");
            PlayAnimation("Start");
            bowlerAnimator.SetBool("Idle", false);
        }

        // Reset UI
        if (powerSlider != null)
            powerSlider.gameObject.SetActive(false);
        if (startBowlButton != null)
            startBowlButton.gameObject.SetActive(false);
        if (markerLockButton != null)
            markerLockButton.gameObject.SetActive(true);


    }

    [PunRPC]
    public void ResetBowl()
    {
        transform.rotation = startingRotation;

        // Reset slider and unlock
        if (powerSlider != null)
        {
            powerSlider.value = powerSlider.minValue; // Reset slider to minimum value
            isSliderLocked = false;
            botHasLockedSlider = false;
        }

        if (ball != null)
        {
            Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
            if (ballRigidbody != null)
            {
                ballRigidbody.linearVelocity = Vector3.zero; // Stop ball movement
                ballRigidbody.angularVelocity = Vector3.zero;
                ballRigidbody.isKinematic = true; // Disable physics for resetting position
            }

            // Manually sync transform changes for all clients
            ball.transform.SetParent(ballParent);
            ball.transform.localPosition = ballStartingPosition;
            ball.transform.localRotation = Quaternion.identity;

            // Force a network update to ensure all clients receive the position reset
            photonView.RPC(nameof(SyncBallTransform), RpcTarget.All, ball.transform.position, ball.transform.rotation);
        }
    }

    [PunRPC]
    public void SyncBallTransform(Vector3 position, Quaternion rotation)
    {
        if (ball != null)
        {
            ball.transform.localPosition = ballStartingPosition;
            ball.transform.localRotation = Quaternion.identity;
        }
    }
    [PunRPC]
    public void RemoveBallParent()
    {
        MultiplayerManager.Instance.spawnedBall.GetComponent<TrailRenderer>().enabled = true;
        ball.transform.parent = null;
    }


}
