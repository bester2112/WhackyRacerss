using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RaceTrack))]
[CanEditMultipleObjects]
public class RaceTrackEditor : BezierSplineEditor
{
    SerializedProperty trackHalfWidth;
    SerializedProperty height;
    SerializedProperty curbRatio;
    SerializedProperty curbSize;
    SerializedProperty curbHeight;
    SerializedProperty checkPointPrefab;
    SerializedProperty startPrefab;
    SerializedProperty startPositionPrefab;
    SerializedProperty subdivisionsCheckPoints;
    SerializedProperty startPositionsCount;

    public new void OnEnable()
    {
        base.OnEnable();

        trackHalfWidth = serializedObject.FindProperty("trackHalfWidth");
        height = serializedObject.FindProperty("height");
        curbRatio = serializedObject.FindProperty("curbRatio");
        curbSize = serializedObject.FindProperty("curbSize");
        curbHeight = serializedObject.FindProperty("curbHeight");
        checkPointPrefab = serializedObject.FindProperty("checkPointPrefab");
        startPrefab = serializedObject.FindProperty("startPrefab");
        startPositionPrefab = serializedObject.FindProperty("startPositionPrefab");
        subdivisionsCheckPoints = serializedObject.FindProperty("subdivisionsCheckPoints");
        startPositionsCount = serializedObject.FindProperty("startPositionsCount");
    }

    public new void OnSceneGUI()
    {
        base.OnSceneGUI();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.PropertyField(trackHalfWidth);
        EditorGUILayout.PropertyField(height);
        EditorGUILayout.PropertyField(curbRatio);
        EditorGUILayout.PropertyField(curbSize);
        EditorGUILayout.PropertyField(curbHeight);
        EditorGUILayout.PropertyField(checkPointPrefab);
        EditorGUILayout.PropertyField(startPrefab);
        EditorGUILayout.PropertyField(startPositionPrefab);
        EditorGUILayout.PropertyField(subdivisionsCheckPoints);
        EditorGUILayout.PropertyField(startPositionsCount);

        RaceTrack track = target as RaceTrack;
        if (GUILayout.Button("Generate Mesh"))
        {
            Undo.RecordObject(target, "Mesh generated");
            track.generateMesh();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
