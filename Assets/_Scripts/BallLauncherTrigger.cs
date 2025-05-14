using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class BallTriggerLauncher : MonoBehaviourPun
{
    #region Inspector Fields

    [Header("Target Area")]
    public TargetArea targetArea;

    [Header("Launch Settings")]
    public float launchHeight = 3f;
    public float forceMultiplier = 1f;
    public GameObject impactEffectPrefab;



    #endregion

    #region Internal State

    public Rigidbody ball;

    #endregion

    #region Unity Callbacks

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball") || targetArea == null)
            return;

        Debug.Log("kfladjsh");
       
        if (ball == null )
            return;
        if(!CricketGameManager.Instance.batsmanController.photonView.IsMine) return;
        SpawnImpactEffect(other.transform.position);

        Vector3 targetPos = GetRandomPositionInTargetArea();
        launchHeight = BallLaunchSettings.Instance.launchHeight;
        forceMultiplier = BallLaunchSettings.Instance.forceMultiplier;



        Vector3 launchVel = CalculateLaunchVelocity(
            other.transform.position,
            targetPos,
            launchHeight
        ) * forceMultiplier;

        photonView.RPC(nameof(LaunchBallRPC), RpcTarget.All, launchVel);
      //ball.GetComponent<BallLauncher>().Launch(launchVel);
        
    }

    #endregion

    #region RPC Methods

    [PunRPC]
    private void LaunchBallRPC(Vector3 velocity)
    {
        Debug.Log(velocity);
        if (ball == null) return;
        ResetBallPhysics(ball);
        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;
        ball.AddForce(velocity, ForceMode.VelocityChange);
        GameManager.Instance.BallHit();
        AudioManager.instance.Play("BatHit");
    }

    #endregion

    #region Helper Methods

    private void SpawnImpactEffect(Vector3 position)
    {
        if (impactEffectPrefab == null) return;

        var effect = Instantiate(impactEffectPrefab, position, Quaternion.identity);
        Destroy(effect, 1.5f);
    }

    private Vector3 GetRandomPositionInTargetArea()
    {
        Vector3 size = targetArea.areaSize;
        float x = Random.Range(-size.x * 0.5f, size.x * 0.5f);
        float z = Random.Range(-size.z * 0.5f, size.z * 0.5f);
        const float yOffset = 0.5f;
        return targetArea.transform.position + new Vector3(x, yOffset, z);
    }

    private void ResetBallPhysics(Rigidbody rb)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.linearDamping = 0f;
        rb.useGravity = true;
    }

    private Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 target, float peakHeight)
    {
        Vector3 displacement = target - start;
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f, displacement.z);
        float g = Mathf.Abs(Physics.gravity.y);
        float timeToApex = Mathf.Sqrt(2f * peakHeight / g);
        float totalTime = timeToApex +
                                      Mathf.Sqrt(2f * (peakHeight - displacement.y) / g);
        Vector3 horizontalVelocity = horizontalDisplacement / totalTime;
        float verticalVelocity = Mathf.Sqrt(2f * g * peakHeight);
        return horizontalVelocity + Vector3.up * verticalVelocity;
    }

    #endregion
}
