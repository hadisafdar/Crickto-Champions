using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class FielderController : MonoBehaviourPun
{
    #region Fielder Settings
    public float pickUpRadius = 2f;             // Radius to pick up the ball
    public Animator animator;                   // Animator for animations
    public Transform ballParent;                // Parent transform for holding the ball
    public BoxCollider triggerArea;             // Trigger area for the fielder to react
    public static bool isBallPicked = false;    // Flag for ball pick status
    public Vector3 startingPosition;            // Starting position
    public Quaternion startingRotation;         // Starting rotation
    public ParticleSystem leftFootDust, rightFootDust;  // Dust effects for the fielder
    #endregion

    #region Private Fields
    private NavMeshAgent agent;                 // NavMeshAgent for movement
    public Transform ballTransform;            // Ball's transform
    public Rigidbody ballRigidbody;            // Ball's Rigidbody
    private bool isPickingUp = false;           // Flag for picking up the ball
    public bool isBallInTriggerArea = false;   // Flag to track ball entering trigger area
    [HideInInspector] public bool canChase = false;  // only the chosen one is allowed to move
    #endregion

    #region Unity Callbacks
    void Start()
    {
        FielderManager.Instance.Register(this);
        // assume all your fielders have their ballTransform set in inspector,
        // so manager can just pick one reference:
        if (FielderManager.Instance.ballTransform == null)
            FielderManager.Instance.ballTransform = ballTransform;
        agent = GetComponent<NavMeshAgent>();
        startingPosition = transform.position;
        startingRotation = transform.rotation;

        if (triggerArea != null)
            triggerArea.isTrigger = true;

        SetAnimation(true, false, false);
        Boundary.OnSixerAndFour += OnSixerAndFour;
        GameManager.OnResetGame += FielderReset;
    }
    void OnDestroy()
    {
        if (FielderManager.Instance != null)
            FielderManager.Instance.Unregister(this);
    }
    void Update()
    {
        if (!Ground.BowlHit) return;
        if (!canChase) return;
        if (isBallPicked)
        {
            agent.isStopped = true;
            SetAnimation(true, false, false);
            return;
        }
        if (ballTransform == null || ballRigidbody == null || isPickingUp || !isBallInTriggerArea) return;

        MoveTowardsBall();
        CheckForBallPickup();
    }

    private void OnDisable()
    {
        GameManager.OnResetGame -= FielderReset;
        Boundary.OnSixerAndFour -= OnSixerAndFour;
    }
    #endregion

    #region Fielder Initialization


    public void FielderReset()
    {
        transform.position = startingPosition;
        transform.rotation = startingRotation;

        SetAnimation(true, false, false);
        isBallInTriggerArea = false;
        agent.isStopped = false;
        isPickingUp = true;
        isBallPicked = false;
        photonView.RPC(nameof(FielderSetPosition), RpcTarget.All, startingPosition);
        Invoke(nameof(EnableBoxCollider), 1f);
    }

    private void EnableBoxCollider()
    {
        isPickingUp = false;
    }
    #endregion

    #region Ball Pickup Handling
    private void OnSixerAndFour()
    {
        SetAnimation(true, false, false);
        isBallInTriggerArea = false;
        agent.isStopped = true;
    }

    private void MoveTowardsBall()
    {
        if (agent != null)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            photonView.RPC(nameof(FielderSetPosition), RpcTarget.All, ballTransform.position);
            SetAnimation(false, true, false);
        }
    }

    private void CheckForBallPickup()
    {
        float distanceToBall = Vector3.Distance(transform.position, ballTransform.position);

        if (distanceToBall <= pickUpRadius)
        {
            PickUpBall();
        }
    }

    private void PickUpBall()
    {
        isPickingUp = true;
        agent.isStopped = true;
        SetAnimation(false, false, true);
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.useGravity = false;
    }
    #endregion

    #region Ball Pickup Animation and Throwing
    public void PickBallEvent()
    {
        ballTransform.SetParent(ballParent);
        ballTransform.localPosition = new Vector3(0, 0f, 0f);
        isBallPicked = true;

        RotateTowardBowlerWithDOTween();
    }

    private void RotateTowardBowlerWithDOTween()
    {
        Vector3 directionToBowler = (CricketGameManager.Instance.bowlerController.ballParent.position - transform.position).normalized;
        directionToBowler.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(directionToBowler);
        Vector3 eulerAngles = targetRotation.eulerAngles;

        float rotationDuration = 0.5f;

        transform.DORotate(eulerAngles, rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                PlayAnimation("Throw");
                BowlerFaceBall();
            });
    }

    public void ThrowBall()
    {
        Transform bowler = CricketGameManager.Instance.bowlerController.ballParent;

        if (ballTransform == null || bowler == null) return;

        ballTransform.SetParent(null);

        Vector3 midPoint = (ballTransform.position + bowler.position) / 2f;
        midPoint.y += 2f;

        ballTransform.DOPath(new Vector3[] { ballTransform.position, midPoint, bowler.position }, 1f, PathType.CatmullRom)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                CricketGameManager.Instance.bowlerController.CatchBall(ballTransform.position, transform.position);
            });
    }

    private void BowlerFaceBall()
    {
        Vector3 directionToBall = (ballTransform.position - CricketGameManager.Instance.bowlerController.transform.position).normalized;
        directionToBall.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(directionToBall);
        Vector3 bowlerEulerAngles = targetRotation.eulerAngles;

        float bowlerRotationDuration = 0.4f;

        CricketGameManager.Instance.bowlerController.transform.DORotate(bowlerEulerAngles, bowlerRotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuad);
    }
    #endregion

    #region Photon Syncing
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
    #endregion

    #region Trigger Events
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            isBallInTriggerArea = true;
            FielderManager.Instance.EvaluateChaser();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            // Optionally handle the exit logic if needed
            FielderManager.Instance.EvaluateChaser();
            isBallInTriggerArea = false;
        }
    }
    // Called by the manager to tell this fielder if it should pursue or not
    public void SetChasePermission(bool allowed)
    {
        canChase = allowed;

        if (!allowed)
        {
            agent.isStopped = true;
            SetAnimation(idle: true, running: false, pick: false);
        }
    }
    public void LeftDustSpawn()
    {
        //leftFootDust.Play();
    }

    public void RightDustSpawn()
    {
        //rightFootDust.Play();
    }
    #endregion

    #region Debugging and Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pickUpRadius);
    }
    #endregion
}
