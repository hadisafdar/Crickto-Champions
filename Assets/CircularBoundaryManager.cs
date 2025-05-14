using Photon.Pun;
using UnityEngine;

/// <summary>
/// Defines circular scoring boundaries around a pitch center.
/// MasterClient can call EvaluateRuns(position) manually to award 1,2,3,4 or 6 runs.
/// Additionally, Update() only watches for the ball crossing the outer boundary
/// to debug‐log “Four!” or “Six!” in real time.
/// </summary>
public class CircularBoundaryManager : MonoBehaviourPun
{

    public static CircularBoundaryManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    #region Inspector Fields

    [Header("Pitch Center")]
    public Transform pitchCenter;

    [Header("Ball Reference")]
    public Transform ballTransform;

    [Header("Radii (meters)")]
    public float oneRunRadius = 10f;
    public float twoRunRadius = 20f;
    public float threeRunRadius = 30f;
    public float boundaryRadius = 40f;  // both 4 and 6

    [Header("Six Height Threshold")]
    [Tooltip("Ball must be above this Y to count as six instead of four")]
    public float sixMinHeight = 2f;

    #endregion

    #region Internal State

    public bool _hasScored = false;  // prevents double‐awarding
    public bool _outerLogged = false;  // for Update() debug

    #endregion

    #region Unity Callbacks

    private void Update()
    {
        if (MissBall.IsBallMiss) return;
        // Only debug‐log 4/6 crossing, don't award here
        if (_outerLogged || !PhotonNetwork.IsMasterClient) return;
        if (pitchCenter == null || ballTransform == null) return;

        float dist = Vector2.Distance(
            new Vector2(pitchCenter.position.x, pitchCenter.position.z),
            new Vector2(ballTransform.position.x, ballTransform.position.z)
        );

        if (dist >= boundaryRadius)
        {
            bool isSix = ballTransform.position.y >= sixMinHeight;
            Debug.Log(isSix ? "Six!" : "Four!");
            if (Ground.canSixer)
            {
                Debug.Log("Six");
                Boundary.OnSixerOrFour();
                CricketScoreManager.Instance.AddScore(6);
            }
            else
            {
                Debug.Log("Four");
                Boundary.OnSixerOrFour();
                CricketScoreManager.Instance.AddScore(4);
            }

            _outerLogged = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (pitchCenter == null) return;
        DrawCircle(oneRunRadius, Color.white);
        DrawCircle(twoRunRadius, Color.yellow);
        DrawCircle(threeRunRadius, Color.cyan);
        DrawCircle(boundaryRadius, Color.green);
    }

    #endregion

    #region Public API

    /// <summary>
    /// Call this when you want to evaluate and award runs
    /// (e.g. when the ball stops or the over ends).
    /// Only the MasterClient will actually award.
    /// </summary>
    public void EvaluateRuns(Vector3 ballPosition)
    {

        if (!PhotonNetwork.IsMasterClient || _hasScored || pitchCenter == null) return;

        if (MissBall.IsBallMiss) return;
        // compute XZ distance
        float dist = Vector2.Distance(
            new Vector2(pitchCenter.position.x, pitchCenter.position.z),
            new Vector2(ballPosition.x, ballPosition.z)
        );

        int runs;
        if (dist <= oneRunRadius)
        {
            runs = 1;

        }
        else if (dist <= twoRunRadius)
        {
            runs = 2;

        }
        else
        {

            runs = 3;
        }

        CricketScoreManager.Instance.AddScore(runs);
        //AwardRuns(runs);
    }

    #endregion

    #region Scoring

    private void AwardRuns(int runs)
    {
        _hasScored = true;
        photonView.RPC(
            nameof(RPC_AwardRuns),
            RpcTarget.AllBuffered,
            runs
        );
    }

    [PunRPC]
    private void RPC_AwardRuns(int runs)
    {
        CricketScoreManager.Instance.AddScore(runs);
    }

    #endregion

    #region Helpers

    private void DrawCircle(float radius, Color col)
    {
        const int SEGMENTS = 64;
        Gizmos.color = col;
        Vector3 center = pitchCenter.position;
        Vector3 prev = center + Vector3.right * radius;
        for (int i = 1; i <= SEGMENTS; ++i)
        {
            float theta = (i / (float)SEGMENTS) * Mathf.PI * 2f;
            Vector3 next = center + new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    #endregion
}
