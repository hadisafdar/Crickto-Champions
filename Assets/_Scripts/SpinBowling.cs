using UnityEngine;

public class SpinBowling : MonoBehaviour
{
    [Header("Spin Settings")]
    public float spinForce = 2f; // Force applied for spin
    public float groundSpinMultiplier = 5f; // Multiplier for spin after hitting the ground
    public SpinType spinType = SpinType.OffSpin; // Type of spin

    private Rigidbody ballRigidbody;
    public static bool hasHitGround = false; // Check if ball has hit the ground

    private void Start()
    {
        ballRigidbody = GetComponent<Rigidbody>();

        if (ballRigidbody == null)
        {
            Debug.LogError("Rigidbody not found on the ball!");
        }
    }

    private void FixedUpdate()
    {
        if (!hasHitGround)
        {
            // Apply spin during flight
            ApplySpinDuringFlight();
        }
    }

    private void ApplySpinDuringFlight()
    {
        // Apply angular velocity to simulate spin
        switch (spinType)
        {
            case SpinType.OffSpin:
                ballRigidbody.AddTorque(Vector3.up * spinForce, ForceMode.Force); // Spin clockwise
                break;
            case SpinType.LegSpin:
                ballRigidbody.AddTorque(Vector3.down * spinForce, ForceMode.Force); // Spin counter-clockwise
                break; 
            case SpinType.NoSpin:
                //ballRigidbody.AddTorque(Vector3.down * spinForce, ForceMode.Force); // Spin counter-clockwise
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasHitGround && collision.gameObject.CompareTag("Pitch"))
        {
            hasHitGround = true;
            if (spinType == SpinType.NoSpin)
            {
                return;
            }
            // Apply additional force based on spin type
            Vector3 groundSpinForce = Vector3.zero;

            switch (spinType)
            {
                case SpinType.OffSpin:
                    groundSpinForce = new Vector3(0, 0, groundSpinMultiplier); // Deviate to the right
                    break;
                case SpinType.LegSpin:
                    groundSpinForce = new Vector3(0f, 0, -groundSpinMultiplier); // Deviate to the left
                    break;
            }
            

            // Add the spin deviation force after hitting the ground
            ballRigidbody.AddForce(groundSpinForce, ForceMode.VelocityChange);

            Debug.Log($"Ball hit the ground and deviated with {spinType}");
        }
    }
}

public enum SpinType
{
    OffSpin,
    LegSpin,
    NoSpin
}
