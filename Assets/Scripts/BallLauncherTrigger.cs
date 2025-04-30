using Photon.Pun;
using UnityEngine;

public class BallTriggerLauncher : MonoBehaviourPun
{
    [Header("Target Area")]
    public TargetArea targetArea; // Reference to TargetArea script

    [Header("Launch Settings")]
    public float launchHeight = 3f; // Desired peak height for the trajectory
    public float forceMultiplier = 1f; // Multiplier to adjust the force applied
    public GameObject impactEffectPrefab; // Prefab for the impact effect

    [Header("Debug Settings")]
    public bool showDebug = true; // Toggle debug logs


    public Rigidbody ball;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball")) // Check if the collider has the tag "Ball"
        {
            Rigidbody ballRigidbody = other.GetComponent<Rigidbody>();
            ball = ballRigidbody;
            PhotonView ballPhotonView = other.GetComponent<PhotonView>();
            if (ballRigidbody != null && ballPhotonView != null && targetArea != null)
            {
                Debug.Log("Hit the bowl");
                AudioManager.instance.Play("BatHit");
                // Spawn the impact effect at the trigger's position
                SpawnImpactEffect(other.transform.position);

                // Calculate a random target position within the TargetArea
                Vector3 targetPosition = GetRandomPositionInTargetArea();

                // Get updated settings from BallLaunchSettings
                launchHeight = BallLaunchSettings.Instance.launchHeight;
                forceMultiplier = BallLaunchSettings.Instance.forceMultiplier;

                // Reset the ball physics for a clean launch
                //ResetBallPhysics(ballRigidbody);

                // Calculate and apply the launch velocity
                Vector3 launchVelocity = CalculateLaunchVelocity(other.transform.position, targetPosition, launchHeight);
               // ballRigidbody.AddForce(launchVelocity * forceMultiplier, ForceMode.VelocityChange);

                // Notify GameManager about the ball hit
                GameManager.Instance.BallHit();
                // Debug information
                if (showDebug)
                {
                    Debug.Log($"Impact spawned at {other.transform.position}");
                }
                photonView.RPC(nameof(RPC_HandleBallHit), RpcTarget.All, ballPhotonView.ViewID, other.transform.position);
                photonView.RPC(nameof(DoLaunchRPC), RpcTarget.All, launchVelocity);
            }
        }
    }

    /// <summary>
    /// Called on all clients to detach, enable physics/trail, and set the new flight velocity.
    /// </summary>
    [PunRPC]
    public void DoLaunchRPC(Vector3 launchVelocity)
    {
        ResetBallPhysics(ball);
        // 1) Detach & show trail
        transform.parent = null;
       
       
        // 2) Enable physics
        ball.isKinematic = false;
        ball.useGravity = true;

        // 3) Give it its new velocity
        ball.linearVelocity = launchVelocity;
    }


    [PunRPC]
    public void RPC_HandleBallHit(int ballViewID, Vector3 ballPosition)
    {
        AudioManager.instance.Play("BatHit");
        /*  Debug.Log("Hit the bowl");
          launchHeight = BallLaunchSettings.Instance.launchHeight;
          forceMultiplier = BallLaunchSettings.Instance.forceMultiplier;
          Vector3 targetPosition = GetRandomPositionInTargetArea();

          PhotonView ballPhotonView = PhotonView.Find(ballViewID);
          if (ballPhotonView != null)
          {
              Rigidbody ballRigidbody = ballPhotonView.GetComponent<Rigidbody>();
              if (ballRigidbody != null)
              {
                  // Reset the ball's physics and apply launch velocity
                  ResetBallPhysics(ballRigidbody);
                  Vector3 launchVelocity = CalculateLaunchVelocity(ballPosition, targetPosition, launchHeight);
                  ballRigidbody.AddForce(launchVelocity * forceMultiplier, ForceMode.VelocityChange);
              }
          }
          GameManager.Instance.BallHit();*/
    }

  
    private void SpawnImpactEffect(Vector3 position)
    {
        if (impactEffectPrefab != null)
        {
            var prefab =Instantiate(impactEffectPrefab, position, Quaternion.identity);
            Destroy(prefab,1.5f);
        }
    }

    private Vector3 GetRandomPositionInTargetArea()
    {
        // Generate a random position within the bounds of the target area
        Vector3 areaSize = targetArea.areaSize;
        float randomX = Random.Range(-areaSize.x / 2, areaSize.x / 2);
        float randomZ = Random.Range(-areaSize.z / 2, areaSize.z / 2);
        float yOffset = 0.5f; // Small offset above the ground

        return targetArea.transform.position + new Vector3(randomX, yOffset, randomZ);
    }

    private void ResetBallPhysics(Rigidbody ballRigidbody)
    {
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.linearDamping = 0f; // Reset drag
        ballRigidbody.useGravity = true; // Ensure gravity is enabled
    }

    private Vector3 CalculateLaunchVelocity(Vector3 startPosition, Vector3 targetPosition, float desiredHeight)
    {
        // Calculate the displacement between the start and target positions
        Vector3 displacement = targetPosition - startPosition;

        // Separate horizontal and vertical components
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0, displacement.z);
        float horizontalDistance = horizontalDisplacement.magnitude;

        float gravity = Mathf.Abs(Physics.gravity.y);

        // Calculate the time to reach the desired peak height
        float timeToApex = Mathf.Sqrt(2 * desiredHeight / gravity);
        float totalFlightTime = timeToApex + Mathf.Sqrt(2 * (desiredHeight - displacement.y) / gravity);

        // Calculate the horizontal velocity needed to cover the horizontal distance
        Vector3 horizontalVelocity = horizontalDisplacement / totalFlightTime;

        // Calculate the vertical velocity to reach the desired peak height
        float verticalVelocity = Mathf.Sqrt(2 * gravity * desiredHeight);

        // Combine the horizontal and vertical components into the launch velocity
        Vector3 launchVelocity = horizontalVelocity + Vector3.up * verticalVelocity;

        return launchVelocity;
    }
}
