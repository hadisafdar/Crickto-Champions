using UnityEngine;

public class CircularCricketScoring : MonoBehaviour
{
    public static CircularCricketScoring Instance { get; private set; } // Singleton instance

    public Transform fieldCenter; // The center of the cricket ground (pitch)
    public Transform ball; // Reference to the ball

    public float oneRunRadius = 10f;
    public float twoRunRadius = 20f;
    public float threeRunRadius = 30f;
    public float boundaryRadius = 40f; // 4 Runs
    public float sixRadius = 50f; // 6 Runs

    private int totalScore = 0;

    public delegate void BoundaryEvent();
    public static event BoundaryEvent OnFourHit;
    public static event BoundaryEvent OnSixHit;

    private void Awake()
    {
        // Singleton Pattern: Ensures only one instance exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Simulate ball stopping
        {
            AddRuns(ball.position);
        }
    }

    /// <summary>
    /// Calculates and returns runs based on distance from the field center.
    /// </summary>
    public int GetRunsFromPosition(Vector3 ballPosition)
    {
        float distance = Vector3.Distance(fieldCenter.position, ballPosition);

        if (distance < oneRunRadius)
            return 1;
        else if (distance < twoRunRadius)
            return 2;
        else if (distance < threeRunRadius)
            return 3;
        else if (distance < boundaryRadius)
        {
            TriggerFour(); // Call Four Hit Event
            return 4;
        }
        else if (distance < sixRadius)
        {
            TriggerSix(); // Call Six Hit Event
            return 6;
        }
        else
            return 0; // Ball out of bounds (Invalid area)
    }

    /// <summary>
    /// Adds runs to total score based on ball position.
    /// </summary>
    public int AddRuns(Vector3 ballPosition)
    {
        int runs = GetRunsFromPosition(ballPosition);

        totalScore += runs;
        Debug.Log("Runs Scored: " + runs);
        Debug.Log("Total Score: " + totalScore);
        return runs;
    }

    /// <summary>
    /// Called when the ball reaches the boundary (Four).
    /// </summary>
    private void TriggerFour()
    {
        Debug.Log("🏏 FOUR! The ball reached the boundary!");
        OnFourHit?.Invoke(); // Trigger event if there are listeners
    }

    /// <summary>
    /// Called when the ball crosses the boundary (Six).
    /// </summary>
    private void TriggerSix()
    {
        Debug.Log("🏏 SIX! The ball cleared the boundary!");
        OnSixHit?.Invoke(); // Trigger event if there are listeners
    }

    void OnDrawGizmos()
    {
        if (fieldCenter == null)
            return;

        // Draw scoring zones
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(fieldCenter.position, oneRunRadius); // 1 Run Zone

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(fieldCenter.position, twoRunRadius); // 2 Runs Zone

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(fieldCenter.position, threeRunRadius); // 3 Runs Zone

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(fieldCenter.position, boundaryRadius); // 4 Runs (Boundary)

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(fieldCenter.position, sixRadius); // 6 Runs (Six Hit)

        // Draw the ball position
        if (ball != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(ball.position, 0.5f);
        }
    }
}
