using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AStarTest))]
public class AStarTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AStarTest test = (AStarTest)target;

        EditorGUILayout.Space(15);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fixedHeight = 35;


        // -----------------------------
        // BUTTON 1 (existing)
        // -----------------------------
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Spawn AI and Move", buttonStyle))
            test.SpawnAIAndMove();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);


        // -----------------------------
        // BUTTON 2 : shoulder rotation test
        // -----------------------------
        GUI.backgroundColor = new Color(1f, 0.6f, 0f); // orange
        if (GUILayout.Button("Spawn Shoulder Rotation Test AI", buttonStyle))
            test.SpawnAISimpleShoulderRotation();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);


        // -----------------------------
        // BUTTON 3 : aim at player
        // -----------------------------
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Spawn AI Aim At Player", buttonStyle))
            test.SpawnAIAimAtPlayer();
        GUI.backgroundColor = Color.white;


        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Use the buttons to spawn AI for pathfinding, shoulder rotation, or aiming tests.",
            MessageType.Info
        );
    }
}
