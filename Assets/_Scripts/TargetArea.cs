using UnityEngine;

[ExecuteAlways] // Allows the script to run in the editor
public class TargetArea : MonoBehaviour
{
    public Color areaColor = Color.green; // Color for visualizing the area
    public Vector3 areaSize = new Vector3(5, 0.1f, 5); // Default size of the area

    private void OnDrawGizmosSelected()
    {
        // Set Gizmo color
        Gizmos.color = areaColor;

        // Draw a transparent box to represent the area
        Gizmos.DrawCube(transform.position, areaSize);

        // Draw a border outline
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(transform.position, areaSize);
    }
}
