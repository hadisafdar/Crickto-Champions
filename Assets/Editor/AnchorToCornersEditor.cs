using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnchorToCorners))]
public class AnchorToCornersEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AnchorToCorners script = (AnchorToCorners)target;
        if (GUILayout.Button("Set Anchors to Corners"))
        {
            script.SetAnchorsToCorners();
        }
    }
}