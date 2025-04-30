using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ParticleController : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask groundLayer;            // Layer to detect the ground
    public bool isBot = false;               // Simulate movement and auto-trigger if true
    public float moveDuration = 3f;          // Time in seconds to move to target position (2-3 seconds)

    [Header("Movement Boundaries")]
    public float minX = -10f;                // Minimum X position
    public float maxX = 10f;                 // Maximum X position
    public float minZ = -10f;                // Minimum Z position
    public float maxZ = 10f;                 // Maximum Z position

    public static event Action OnBowlingTriggered; // Event for triggering bowling
    public static bool isBowlLaunched = false;

    private Vector3 botTargetPosition;       // Bot's target position
    private float elapsedTime = 0f;          // Tracks time for smooth movement
    private Vector3 startPosition;           // Starting position for smooth movement
    private bool isMovingToTarget = false;   // Tracks if bot is moving to target

    public Transform ballMarker;
    public Vector3 startingSize;

    public Transform cameraTarget;

    private void Start()
    {
        if (isBot)
        {
            SetNewBotTargetPosition();
        }
        startingSize = ballMarker.localScale;
        GameManager.OnResetGame += ResetMarker;
        BowlerController.OnBallLaunched += OnBowlerLaunched;
    }


    public void InitMarker(Button bowlButton)
    {
        //bowlButton.onClick.RemoveAllListeners();
        MultiplayerManager.Instance.markerCameraCin.Follow = transform;
        MultiplayerManager.Instance.markerCameraCin.LookAt = transform;

        //bowlButton.onClick.AddListener(TriggerBowling);
    }


    private void OnBowlerLaunched()
    {
        ballMarker.DOScale(0f, 0.5f).SetEase(Ease.InBack);
    }

    public void ResetMarker()
    {
        elapsedTime = 0f;
        isBowlLaunched = false;
        isMovingToTarget = false;
        ballMarker.localScale = startingSize;
        if (isBot)
        {
            SetNewBotTargetPosition();
        }

        

    }
    private void Update()
    {
        if (isBowlLaunched) return;

        if (isBot)
        {
            SimulateSmoothBotMovement();
        }
        else
        {
            HandleManualMovement();
        }
    }

    private void HandleManualMovement()
    {
        float moveSpeed = 5f; // Default manual movement speed
        float moveHorizontal = SimpleInput.GetAxis("BowlVertical") * moveSpeed * Time.deltaTime;
        float moveVertical = SimpleInput.GetAxis("BowlHorizontal") * moveSpeed * Time.deltaTime;

        Vector3 movement = new Vector3(moveHorizontal, 0, -moveVertical);
        MoveToPosition(transform.position + movement);
    }

    private void SimulateSmoothBotMovement()
    {
        if (!isMovingToTarget)
        {
            SetNewBotTargetPosition();
            isMovingToTarget = true;
            startPosition = transform.position;
            elapsedTime = 0f; // Reset time
        }

        // Smoothly interpolate the position based on time
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / moveDuration); // Normalize time (0 to 1)
        Vector3 newPosition = Vector3.Lerp(startPosition, botTargetPosition, t);

        // Raycast to adjust Y position to ground
        if (Physics.Raycast(newPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            newPosition.y = hit.point.y;
        }
        transform.position = newPosition;

        // If movement is complete, trigger bowling
        if (t >= 1.0f)
        {
            isMovingToTarget = false;
            TriggerBowling();
        }
    }

    private void SetNewBotTargetPosition()
    {
        // Generate a random position within boundaries
        float randomX = UnityEngine.Random.Range(minX, maxX);
        float randomZ = UnityEngine.Random.Range(minZ, maxZ);
        botTargetPosition = new Vector3(randomX, 0, randomZ);
        
    }

    private void MoveToPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);

        if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            position.y = hit.point.y; // Adjust Y position to ground
            transform.position = position;
        }
    }

    public void TriggerBowling()
    {
        isBowlLaunched = true;
        OnBowlingTriggered?.Invoke();
        
    }

    private void OnDrawGizmos()
    {
        // Draw movement boundaries in Scene view
        Gizmos.color = Color.blue;
        Vector3 center = new Vector3((minX + maxX) / 2, 0, (minZ + maxZ) / 2);
        Vector3 size = new Vector3(maxX - minX, 0.1f, maxZ - minZ);
        Gizmos.DrawWireCube(center, size);

        // Draw target position for debugging
        if (isBot)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(botTargetPosition, 0.3f);
        }
    }
}
