using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class CricketCinematicCam : MonoBehaviour
{
    public static CricketCinematicCam Instance { get; private set; }

    [Header("Follow Target (assign or defaults to this)")]
    public Transform followTarget;

    [Header("Cinematic Settings")]
    [Tooltip("Seconds each camera angle stays active")]
    public float timePerAngle = 1f;
    [Tooltip("Seconds for the camera to catch up position")]
    public float positionSmoothTime = 0.3f;
    [Tooltip("How fast the camera rotates toward the target")]
    public float rotationSmoothSpeed = 5f;
    [Tooltip("Seconds to blend between offsets")]
    public float offsetSmoothTime = 0.5f;

    [Header("Offsets (relative to the followTarget)")]
    public Vector3 followOffset = new Vector3(0, 2f, -5f);
    public Vector3 overheadOffset = new Vector3(0, 15f, 0f);
    public Vector3 sideOffset = new Vector3(15f, 5f, 0f);

    Camera mainCamera;
    Camera cinematicCam;
    bool sequencePlaying = false;
    Vector3 currentVelocity;    // for SmoothDamp(position)
    Vector3 offsetVelocity;     // for SmoothDamp(offset)
    Vector3 currentOffset;      // smoothed offset
    Vector3 targetOffset;       // where we’re heading
    Quaternion targetRotation;

    // --- NEW --- for manual prediction
    Rigidbody targetRb;
    Vector3 lastPhysicsPos;
    Vector3 currentPhysicsPos;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cache main camera
        mainCamera = Camera.main;

        // Default followTarget
        if (followTarget == null) followTarget = transform;

        // Cache Rigidbody if present
        targetRb = followTarget.GetComponent<Rigidbody>();
        currentPhysicsPos = lastPhysicsPos = followTarget.position;
    }

    void Start()
    {
        // Create the cinematic camera at runtime
        var go = new GameObject("CinematicCam");
        cinematicCam = go.AddComponent<Camera>();
        cinematicCam.clearFlags = CameraClearFlags.Skybox;
        cinematicCam.depth = 100;
        cinematicCam.enabled = false;

        // Ensure main is active initially
        if (mainCamera != null) mainCamera.enabled = true;

        // Initialize offsets
        currentOffset = targetOffset = followOffset;
    }

    void FixedUpdate()
    {
        // Record physics‐step positions
        lastPhysicsPos = currentPhysicsPos;
        currentPhysicsPos = followTarget.position;
    }

    /// <summary>Trigger the 3-shot cinematic.</summary>
    public void TriggerCinematics()
    {
        if (sequencePlaying) return;

        ResetCinematics();
        if (mainCamera != null) mainCamera.enabled = false;
        StartCoroutine(CinematicSequence());
    }

    /// <summary>Immediately stop and reset.</summary>
    public void DisableCinematics()
    {
        ResetCinematics();
    }

    void ResetCinematics()
    {
        StopAllCoroutines();
        sequencePlaying = false;
        cinematicCam.enabled = false;
        if (mainCamera != null) mainCamera.enabled = true;

        // reset smoothing
        offsetVelocity = Vector3.zero;
        currentOffset = followOffset;
        targetOffset = followOffset;
        currentVelocity = Vector3.zero;
    }

    IEnumerator CinematicSequence()
    {
        sequencePlaying = true;
        cinematicCam.enabled = true;

        // 1) Follow
        targetOffset = followOffset;
        yield return new WaitForSeconds(timePerAngle);

        // 2) Overhead
        targetOffset = overheadOffset;
        yield return new WaitForSeconds(timePerAngle);

        // 3) Side
        targetOffset = sideOffset;
        yield return new WaitForSeconds(timePerAngle);

        // Done
        ResetCinematics();
    }

    void LateUpdate()
    {
        if (!sequencePlaying) return;

        // 1) MANUAL PREDICTION OF BALL POSITION
        Vector3 targetPos;
        if (targetRb != null)
        {
            // Extrapolate from last physics step: pos + vel * dt
            Vector3 estimated = currentPhysicsPos + targetRb.linearVelocity * Time.deltaTime;
            targetPos = estimated;
        }
        else
        {
            // No Rigidbody: just use Transform
            targetPos = followTarget.position;
        }

        // 2) SMOOTH OFFSET TRANSITION
        currentOffset = Vector3.SmoothDamp(
            currentOffset,
            targetOffset,
            ref offsetVelocity,
            offsetSmoothTime
        );

        // 3) SMOOTH CAMERA POSITION
        Vector3 desiredPos = targetPos + currentOffset;
        cinematicCam.transform.position = Vector3.SmoothDamp(
            cinematicCam.transform.position,
            desiredPos,
            ref currentVelocity,
            positionSmoothTime
        );

        // 4) SMOOTH CAMERA ROTATION
        targetRotation = Quaternion.LookRotation(
            targetPos - cinematicCam.transform.position
        );
        cinematicCam.transform.rotation = Quaternion.Slerp(
            cinematicCam.transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSmoothSpeed
        );
    }
}
