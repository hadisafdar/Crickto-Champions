using UnityEngine;
using UnityEditor;



public class MissingScriptRemover : MonoBehaviour
{
#if UNITY_EDITOR
    public void RemoveMissingScripts(GameObject obj)
    {
        // Iterate through all the components on the GameObject
        var components = obj.GetComponents<Component>();

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                // Log the object name and the index of the missing component
                Debug.Log($"Removing missing script from: {obj.name}");

                // Remove the missing script
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            }
        }

        // Recursively check all children
        foreach (Transform child in obj.transform)
        {
            RemoveMissingScripts(child.gameObject);
        }
    }
#endif
}