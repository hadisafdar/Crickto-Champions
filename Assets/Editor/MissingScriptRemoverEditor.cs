using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MissingScriptRemover))]
public class MissingScriptRemoverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MissingScriptRemover script = (MissingScriptRemover)target;

        if (GUILayout.Button("Remove Missing Scripts"))
        {
            Undo.RecordObject(script.gameObject, "Remove Missing Scripts");
            script.RemoveMissingScripts(script.gameObject);
            Debug.Log("Missing scripts removed.");
        }
    }
}