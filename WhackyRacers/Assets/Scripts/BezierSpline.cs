using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BezierSpline : MonoBehaviour
{
    public List<BezierCurve> segments = new List<BezierCurve>();
    public List<BezierCurve> equidistantSegments = new List<BezierCurve>();
    [SerializeField]
    private bool shouldClose = true;
    public bool closed = false;
    public int subdivisions;

    public void Awake()
    {
        if (segments.Count == 0)
        {
            segments.Add(new BezierCurve(Vector3.zero));
        }
    }

    public void addSegment(Vector3 position)
    {
        BezierCurve curve = new BezierCurve(position);

        BezierCurve before = segments[segments.Count - 1];

        curve.controlPoints[0] = before.controlPoints[3];
        curve.controlPoints[1] = before.controlPoints[3] + (before.controlPoints[3] - before.controlPoints[2]);
        curve.controlPoints[2] = position + (curve.controlPoints[1] - position) * 0.5f;
        curve.controlPoints[3] = position;

        segments.Add(curve);
    }

    public void moveControlPoint(Vector3 newPosition, int segment, int controlPoint)
    {
        switch (controlPoint)
        {
            case 0:
                Vector3 delta = newPosition - segments[segment].controlPoints[0];

                segments[segment].controlPoints[0] = newPosition;
                segments[segment].controlPoints[1] += delta;

                if (segment >= 1)
                {
                    segments[segment - 1].controlPoints[3] = newPosition;
                    segments[segment - 1].controlPoints[2] += delta;
                }
                else if (closed)
                {
                    segments[segments.Count - 1].controlPoints[3] = newPosition;
                    segments[segments.Count - 1].controlPoints[2] += delta;
                }
                break;
            case 1:
                segments[segment].controlPoints[1] = newPosition;

                if (segment >= 1)
                {
                    segments[segment - 1].controlPoints[2] = segments[segment].controlPoints[0] + (segments[segment].controlPoints[0] - segments[segment].controlPoints[1]);
                }
                else if (closed)
                {
                    segments[segments.Count - 1].controlPoints[2] = segments[segment].controlPoints[0] + (segments[segment].controlPoints[0] - segments[segment].controlPoints[1]);
                }
                break;
            case 2:
                segments[segment].controlPoints[2] = newPosition;

                if (segment < segments.Count - 1)
                {
                    segments[segment + 1].controlPoints[1] = segments[segment].controlPoints[3] + (segments[segment].controlPoints[3] - segments[segment].controlPoints[2]);
                }
                else if (closed)
                {
                    segments[0].controlPoints[1] = segments[segment].controlPoints[3] + (segments[segment].controlPoints[3] - segments[segment].controlPoints[2]);
                }
                break;
            case 3:
                delta = newPosition - segments[segment].controlPoints[3];

                segments[segment].controlPoints[3] = newPosition;
                segments[segment].controlPoints[2] += delta;

                if (segment < segments.Count - 1)
                {
                    segments[segment + 1].controlPoints[0] = newPosition;
                    segments[segment + 1].controlPoints[1] += delta;
                }
                else if (closed)
                {
                    segments[0].controlPoints[0] = newPosition;
                    segments[0].controlPoints[1] += delta;
                }
                break;
            default:
                break;
        }
    }

    public void close()
    {
        if (shouldClose && closed)
        {
            shouldClose = false;
            addSegment(segments[0].controlPoints[0]);
            moveControlPoint(segments[0].controlPoints[1], 0, 1);
        }
        if (!shouldClose && !closed)
        {
            shouldClose = true;
            if (segments.Count > 1)
            {
                segments.RemoveAt(segments.Count - 1);
            }
        }
    }

    public List<BezierCurve> subdivide(int subdivisions)
    {
        List<BezierCurve> subdividedSegments = new List<BezierCurve>();

        foreach (BezierCurve curve in segments)
        {
            subdividedSegments.Add(curve);
        }

        for (int i = 0; i < subdivisions; ++i)
        {
            List<BezierCurve> temp = new List<BezierCurve>();

            foreach (BezierCurve curve in subdividedSegments)
            {
                BezierCurve a;
                BezierCurve b;
                curve.subdivide(0.5f, out a, out b);

                temp.Add(a);
                temp.Add(b);
            }

            subdividedSegments = temp;
        }

        return subdividedSegments;
    }
}
