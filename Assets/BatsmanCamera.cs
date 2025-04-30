using Unity.Cinemachine;
using UnityEngine;

public class BatsmanCamera : MonoBehaviour
{
    public CinemachineRotationComposer m_Composer;
    public float maxOffsetX = 0.1f; // Maximum offset range for X-axis
    public float maxOffsetY = 0.1f; // Maximum offset range for Y-axis
    public float smoothSpeed = 5f;  // Speed for smooth interpolation
    private Vector3 initialOffset;
    private Vector3 targetOffset;

    void Start()
    {
        if (m_Composer != null)
        {
            initialOffset = m_Composer.TargetOffset;
        }
    }

    void Update()
    {
        if (m_Composer == null) return;

        float horizontalInput = SimpleInput.GetAxis("BatHorizontal");
        float verticalInput = SimpleInput.GetAxis("BatVertical");

        // Calculate target offsets based on input percentages of max values
        targetOffset = initialOffset;
        targetOffset.x += horizontalInput * maxOffsetX;
        targetOffset.y += verticalInput * maxOffsetY;

        // Smoothly interpolate towards the target offset
        m_Composer.TargetOffset = Vector3.Lerp(m_Composer.TargetOffset, targetOffset, Time.deltaTime * smoothSpeed);
    }
}
