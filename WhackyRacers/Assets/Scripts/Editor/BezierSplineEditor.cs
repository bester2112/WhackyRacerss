using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierSpline))]
[CanEditMultipleObjects]
public class BezierSplineEditor : Editor
{
    SerializedProperty subdivisions;
    SerializedProperty segments;
    SerializedProperty closed;

    public void OnEnable()
    {
        subdivisions = serializedObject.FindProperty("subdivisions");
        segments = serializedObject.FindProperty("segments");
        closed = serializedObject.FindProperty("closed");
    }

    public void OnSceneGUI()
    {
        input();
        render();
    }

    private void input()
    {
        BezierSpline spline = target as BezierSpline;

        Event currentEvent = Event.current;

        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && currentEvent.alt)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
            RaycastHit raycastHit;
            float t = 5.0f;

            if (Physics.Raycast(ray, out raycastHit))
            {
                t = raycastHit.distance;
            }

            Vector3 newPosition = ray.origin + ray.direction * t;

            Undo.RecordObject(target, "Added a segment");
            spline.addSegment(newPosition);
        }
    }

    private void render()
    {
        BezierSpline spline = target as BezierSpline;

        for (int i = 0; i < spline.segments.Count; ++i)
        {
            BezierCurve curve = spline.segments[i];

            Vector3[] globalControlPoints = { spline.transform.TransformPoint(curve.controlPoints[0]),
                                              spline.transform.TransformPoint(curve.controlPoints[1]),
                                              spline.transform.TransformPoint(curve.controlPoints[2]),
                                              spline.transform.TransformPoint(curve.controlPoints[3]) };

            Handles.color = Color.gray;
            Handles.DrawLine(globalControlPoints[0], globalControlPoints[1]);
            Handles.DrawLine(globalControlPoints[3], globalControlPoints[2]);

            Handles.DrawBezier(globalControlPoints[0], globalControlPoints[3], globalControlPoints[1], globalControlPoints[2], Color.green, null, 5);

            for (int j = 0; j < 4; ++j)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 movedPosition = Handles.PositionHandle(globalControlPoints[j], Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Moved controlpoint" + j);
                    spline.moveControlPoint(spline.transform.InverseTransformPoint(movedPosition), i, j);
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(subdivisions);
        EditorGUILayout.PropertyField(closed);
        EditorGUILayout.PropertyField(segments);

        BezierSpline spline = target as BezierSpline;
        spline.close();

        serializedObject.ApplyModifiedProperties();
    }
}
