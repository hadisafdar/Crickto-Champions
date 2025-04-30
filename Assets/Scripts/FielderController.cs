using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class FielderController : MonoBehaviourPun
{
    [Header("Fielder Settings")]
    public float pickUpRadius = 2f;             // Radius to pick up the ball
    public Animator animator;                   // Animator for animations
    public Transform ballParent;                // Parent transform for holding the ball
    public BoxCollider triggerArea;             // Trigger area for the fielder to react

    private NavMeshAgent agent;                 // NavMeshAgent for movement
    private Transform ballTransform;            // Ball's transform
    private Rigidbody ballRigidbody;            // Ball's Rigidbody
    private bool isPickingUp = false;           // Flag for picking up the ball
    private bool isBallInTriggerArea = false;   // Flag to track ball entering trigger area

    public static bool isBallPicked = false;
    public Vector3 startingPosition;

    public Quaternion startingRotation;



    public ParticleSystem leftFootDust, rightFootDust;



    public void LeftDustSpawn()
    {
        leftFootDust.Play();
    }

    public void RightDustSpawn()
    {
        rightFootDust.Play();
    }

    void Start()
    {
        // Initialize NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        startingPosition = transform.position;
        startingRotation = transform.rotation;
        // Find the ball in the scene
        /* GameObject ballObject = GameObject.FindGameObjectWithTag("Ball");
         if (ballObject != null)
         {
             ballTransform = ballObject.transform;
             ballRigidbody = ballObject.GetComponent<Rigidbody>();
         }*/

        // Ensure the trigger area collider is set as a trigger
        if (triggerArea != null)
            triggerArea.isTrigger = true;

        // Start in idle state
        //animator.SetTrigger("Idle");
        SetAnimation(true, false, false);
        Boundary.OnSixerAndFour += OnSixerAndFour;
        GameManager.OnResetGame += FielderReset;
    }


    public void InitFielder(GameObject ball)
    {
        ballTransform = ball.transform;
        ballRigidbody = ball.GetComponent<Rigidbody>();
    }



    public void FielderReset()
    {
        transform.position = startingPosition;
        transform.rotation = startingRotation;
        /*GameObject ballObject = GameObject.FindGameObjectWithTag("Ball");
        if (ballObject != null)
        {
            ballTransform = ballObject.transform;
            ballRigidbody = ballObject.GetComponent<Rigidbody>();
        }*/

        /*// Ensure the trigger area collider is set as a trigger
        if (triggerArea != null)
            triggerArea.isTrigger = true;*/

        // Start in idle state
        //animator.SetTrigger("Idle");
        //animator.ResetTrigger("Run");
        //animator.ResetTrigger("Pick");

        SetAnimation(true, false, false);

        isBallInTriggerArea = false;
        agent.isStopped = false;
        isPickingUp = true;
        isBallPicked = false;
        //agent.SetDestination(startingPosition);
        photonView.RPC(nameof(FielderSetPosition), RpcTarget.All, startingPosition);
        Invoke(nameof(EnableBoxCollider), 1f);
    }

    private void EnableBoxCollider()
    {
        isPickingUp = false;
    }

    private void OnSixerAndFour()
    {
        //animator.SetTrigger("Idle");
        // animator.ResetTrigger("Run");
        // animator.ResetTrigger("Pick");
        SetAnimation(true, false, false);
        isBallInTriggerArea = false;
        agent.isStopped = true;
    }

    private void OnDisable()
    {
        GameManager.OnResetGame -= FielderReset;
        Boundary.OnSixerAndFour -= OnSixerAndFour;
    }
    void Update()
    {
        if (!Ground.BowlHit) return;

        /* if (isBallPicked)
         {
             animator.SetTrigger("Idle");
             animator.ResetTrigger("Run");
             animator.ResetTrigger("Pick");
             isBallInTriggerArea = false;
             agent.isStopped = true;
             return;
         }*/


        if (ballTransform == null || ballRigidbody == null || isPickingUp || !isBallInTriggerArea) return;

        MoveTowardsBall();       // Move the fielder towards the ball
        CheckForBallPickup();    // Check if the fielder is close enough to pick up the ball
    }

    /// <summary>
    /// Move the fielder towards the ball.
    /// </summary>
    private void MoveTowardsBall()
    {
        if (agent != null)
        {
            // Set destination to the ball's position
            //agent.SetDestination(ballTransform.position);
            photonView.RPC(nameof(FielderSetPosition), RpcTarget.All, ballTransform.position);
            //animator.SetTrigger("Run"); // Play running animation
            SetAnimation(false, true, false);

        }
    }

    /// <summary>
    /// Check if the ball is within the pickup radius and pick it up.
    /// </summary>
    private void CheckForBallPickup()
    {
        float distanceToBall = Vector3.Distance(transform.position, ballTransform.position);

        if (distanceToBall <= pickUpRadius)
        {
            PickUpBall();
        }
    }

    /// <summary>
    /// Pick up the ball.
    /// </summary>
    private void PickUpBall()
    {
        Debug.Log("Picking Up Ball");

        isPickingUp = true;
        agent.isStopped = true; // Stop NavMeshAgent movement
        //animator.SetTrigger("Pick"); // Play picking animation
        //animator.ResetTrigger("Run");
        SetAnimation(false, false, true);
        // Stop the ball's motion and "attach" it to the fielder
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        // ballRigidbody.isKinematic = true;
        ballRigidbody.useGravity = false;

    }

    /// <summary>
    /// Animation Event: Attach the ball to the fielder's hand position.
    /// </summary>
    public void PickBallEvent()
    {
        ballTransform.SetParent(ballParent);
        ballTransform.localPosition = new Vector3(0, 0f, 0f); // Adjust position to fielder's hand
        Debug.Log("Ball picked up by the fielder!");
        //animator.SetTrigger("Idle"); 
        RotateTowardBowlerWithDOTween();  // Rotate the fielder towards the bowler
        //SetAnimation(true, false, false);
        //PlayAnimation("Throw");

        isBallPicked = true;

    }
    private void RotateTowardBowlerWithDOTween()
    {
        

        Vector3 directionToBowler = (MultiplayerManager.Instance.spawnedBowler.GetComponent<BowlerController>().ballParent.position - transform.position).normalized;  // Direction towards bowler
        directionToBowler.y = 0;  // Lock Y-axis for horizontal rotation

        Quaternion targetRotation = Quaternion.LookRotation(directionToBowler);  // Target rotation
        Vector3 eulerAngles = targetRotation.eulerAngles;  // Get Euler angles for DORotate

        float rotationDuration = 0.5f;  // Time to complete the rotation

        // Use DORotate to rotate smoothly and play the throw animation after completion
        transform.DORotate(eulerAngles, rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuad)  // Smooth ease-out rotation
            .OnComplete(() =>
            {
                Debug.Log("Rotation complete, playing throw animation!");
                PlayAnimation("Throw");  // Play throw animation after rotation completes
                BowlerFaceBall();
            });
    }

    //In Animation Event
    public void ThrowBall()
    {
        /* float adjustedFlightTime = 1f; // Increase flight time for spin

         Vector3 launchForce = CalculateAdjustedLaunchForce(ballRigidbody.transform.position, MultiplayerManager.Instance.spawnedBowler.GetComponent<BowlerController>().ballParent.position, adjustedFlightTime);
         // Reduce the speed if spin is applied



         ballRigidbody.isKinematic = false;
         ballRigidbody.useGravity = true;
         ballRigidbody.AddForce(launchForce, ForceMode.Impulse);

         ballRigidbody.transform.parent = null;*/

        Transform bowler = MultiplayerManager.Instance.spawnedBowler.GetComponent<BowlerController>().ballParent;
        
        if (ballTransform == null || bowler == null) return;

        // Reset ball parent to avoid being a child of the fielder
        ballTransform.SetParent(null);

        // Calculate mid-point for the arc
        Vector3 midPoint = (ballTransform.position + bowler.position) / 2f;
        midPoint.y += 2f;  // Increase Y to create a throwing arc
        Vector3 ballPos = ballTransform.position;
        // Use DOTween to move along the curve
        ballTransform.DOPath(new Vector3[] { ballTransform.position, midPoint, bowler.position }, 1f, PathType.CatmullRom)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                Debug.Log("Ball reached bowler.");
                MultiplayerManager.Instance.spawnedBowler.GetComponent<BowlerController>().CatchBall(ballPos);
            });

    }
    private void BowlerFaceBall()
    {
        Vector3 directionToBall = (ballTransform.position - MultiplayerManager.Instance.spawnedBowler.transform.position).normalized;
        directionToBall.y = 0;  // Lock Y-axis for horizontal rotation

        Quaternion targetRotation = Quaternion.LookRotation(directionToBall);
        Vector3 bowlerEulerAngles = targetRotation.eulerAngles;

        float bowlerRotationDuration = 0.4f;

        // Rotate bowler to face the ball
        MultiplayerManager.Instance.spawnedBowler.transform.DORotate(bowlerEulerAngles, bowlerRotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                Debug.Log("Bowler is now facing the ball!");
            });
    }
    

    [PunRPC]
    public void FielderSetPosition(Vector3 ballPosition)
    {
        agent.SetDestination(ballPosition);
    }

    public void SetAnimation(bool idle, bool running, bool pick)
    {
        photonView.RPC(nameof(SetAnimationRpc), RpcTarget.All, idle, running, pick);
    }

    [PunRPC]
    public void SetAnimationRpc(bool idle, bool running, bool pick)
    {
        animator.SetBool("Idle", idle);
        animator.SetBool("Run", running);
        animator.SetBool("Pick", pick);
    }


    public void PlayAnimation(string animationName)
    {
        photonView.RPC(nameof(PlayAnimationRpc), RpcTarget.All, animationName);
    }



    [PunRPC]
    public void PlayAnimationRpc(string animationName)
    {
        animator.Play(animationName);
    }




    /// <summary>
    /// Trigger event: Detect when the ball enters the fielder's area.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            isBallInTriggerArea = true;
            Debug.Log("Ball entered the trigger area of the fielder!");
        }
    }










    /// <summary>
    /// Trigger event: Detect when the ball leaves the fielder's area.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            // isBallInTriggerArea = false;
            Debug.Log("Ball left the trigger area of the fielder.");
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize the pickup radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pickUpRadius);


    }
}
