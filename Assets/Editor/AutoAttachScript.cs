using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

[InitializeOnLoad]
public static class AutoAttachScript
{
    static AutoAttachScript()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private static void OnHierarchyChanged()
    {
        // Find all GameObjects in the scene
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Check if the GameObject has a RectTransform and does not already have the AnchorToCornersOnStopMove component
            if (obj.GetComponent<RectTransform>() != null && obj.GetComponent<AnchorToCorners>() == null)
            {
                // Attach the script
                obj.AddComponent<AnchorToCorners>();
            }
        }
    }
}