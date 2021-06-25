using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class RaceTrack : BezierSpline
{
    public float trackHalfWidth;
    public float height;
    public float curbSize;
    public float curbRatio;
    public float curbHeight;

    public Transform checkPointPrefab;
    public Transform startPrefab;
    public Transform startPositionPrefab;

    public int subdivisionsCheckPoints;

    public int startPositionsCount;

    public void generateMesh()
    {
        if (!closed)
        {
            closed = true;
            close();
        }

        GameObject startRing = GameObject.Find("Start(Clone)");
        GameObject.DestroyImmediate(startRing);

        Transform checkpointStart = Instantiate(startPrefab, Vector3.zero, Quaternion.identity, transform);
        checkpointStart.localPosition = segments[0].controlPoints[0];
        checkpointStart.localRotation = Quaternion.LookRotation(segments[0].controlPoints[1] - segments[0].controlPoints[0]) * Quaternion.Euler(0.0f, 90.0f, 0.0f);

        GameObject checkpoints = GameObject.Find("Checkpoints");

        Debug.Log(checkpoints.transform.childCount);

        for (int i = checkpoints.transform.childCount - 1; i >= 0; --i)
        {
            GameObject.DestroyImmediate(checkpoints.transform.GetChild(i).gameObject);
        }

        List<BezierCurve> subdividedCheckPoints = subdivide(subdivisionsCheckPoints);

        int counter = 0;
        foreach (BezierCurve curve in subdividedCheckPoints)
        {
            Transform checkpoint = Instantiate(checkPointPrefab, Vector3.zero, Quaternion.identity, checkpoints.transform);
            checkpoint.localScale = new Vector3(trackHalfWidth + trackHalfWidth, trackHalfWidth, height);
            checkpoint.localPosition = curve.controlPoints[0] + new Vector3(0.0f, trackHalfWidth / 2.0f + height, 0.0f);
            checkpoint.localRotation = Quaternion.LookRotation(curve.controlPoints[1] - curve.controlPoints[0]);

            CheckPointCollision cpc = checkpoint.GetComponent<CheckPointCollision>();
            cpc.checkPointId = counter++;
        }

        GameObject startPositions = GameObject.Find("StartPositions");

        Debug.Log(startPositions.transform.childCount);

        for (int i = startPositions.transform.childCount - 1; i >= 0; --i)
        {
            GameObject.DestroyImmediate(startPositions.transform.GetChild(i).gameObject);
        }

        BezierCurve last = segments[segments.Count - 1];
        float tStart = 0.8f;
        float side = -1.0f;
        for (int i = 0; i < startPositionsCount; ++i)
        {
            Vector3 position = last.deCasteljau(tStart);
            Vector3 offset =  getLeft(last.tangent(tStart), Vector3.up) * trackHalfWidth * 0.5f * side;
            Instantiate(startPositionPrefab, transform.TransformPoint(position + offset) + Vector3.up * height, Quaternion.LookRotation(last.tangent(tStart)) * Quaternion.AngleAxis(-90.0f, Vector3.up), startPositions.transform);

            tStart -= 0.075f;
            side = -side;
        }

        List<BezierCurve> subdivided = subdivide(subdivisions);

        Vector3[] trackVertices = new Vector3[subdivided.Count * 6];
        Vector2[] trackUV = new Vector2[subdivided.Count * 6];
        int[] trackTriangles = new int[subdivided.Count * 30];

        int[] trackIndices = new int[] { 0, 6, 1, 1, 6, 7, 1, 7, 2, 2, 7, 8, 2, 8, 3, 3, 8, 9, 3, 9, 4, 4, 9, 10, 4, 10, 5, 5, 10, 11 };

        bool redCurb = true;

        float minC = 1.0f;
        float maxC = 0.0f;

        foreach (BezierCurve curve in subdivided)
        {
            float curvature = curve.curvature();

            minC = Mathf.Min(minC, curvature);
            maxC = Mathf.Max(maxC, curvature);
        }

        for (int i = 0; i < subdivided.Count; ++i)
        {
            Vector3 left = getLeft(subdivided[i].controlPoints[1] - subdivided[i].controlPoints[0], Vector3.up) * trackHalfWidth;
            float curbPercentage = (subdivided[i].curvature() - minC) / (maxC - minC);

            trackVertices[6 * i    ] = subdivided[i].controlPoints[0] + left + left * curbSize * curbPercentage * 0.25f + left * curbSize * curbPercentage * 0.25f - Vector3.up * height * curbHeight * curbPercentage;
            trackVertices[6 * i + 1] = subdivided[i].controlPoints[0] + left + left * curbSize * curbPercentage * 0.25f                                            + Vector3.up * height * curbHeight * curbPercentage;
            trackVertices[6 * i + 2] = subdivided[i].controlPoints[0] + left                                                                                       + Vector3.up * height;
            trackVertices[6 * i + 3] = subdivided[i].controlPoints[0] - left                                                                                       + Vector3.up * height;
            trackVertices[6 * i + 4] = subdivided[i].controlPoints[0] - left - left * curbSize * curbPercentage * 0.25f                                            + Vector3.up * height * curbHeight * curbPercentage;
            trackVertices[6 * i + 5] = subdivided[i].controlPoints[0] - left - left * curbSize * curbPercentage * 0.25f - left * curbSize * curbPercentage * 0.25f - Vector3.up * height * curbHeight * curbPercentage;

            trackUV[6 * i    ] = new Vector2(1.0f, 0.5f);
            trackUV[6 * i + 1] = new Vector2(1.0f, 0.5f);
            trackUV[6 * i + 2] = new Vector2(1.0f, 0.5f);
            trackUV[6 * i + 3] = new Vector2(1.0f, 0.5f);
            trackUV[6 * i + 4] = new Vector2(1.0f, 0.5f);
            trackUV[6 * i + 5] = new Vector2(1.0f, 0.5f);

            for (int j = 0; j < 30; ++j)
            {
                trackTriangles[30 * i + j] = 6 * i + trackIndices[j];
            }

            if (curbPercentage >= curbRatio)
            {
                float y = 1.0f;
                if (redCurb)
                {
                    y = 0.0f;
                }
                redCurb = !redCurb;

                trackUV[6 * i] = new Vector2(0.0f, y);
                trackUV[6 * i + 1] = new Vector2(0.0f, y);
                trackUV[6 * i + 2] = new Vector2(0.51f, y);
                trackUV[6 * i + 3] = new Vector2(0.51f, y);
                trackUV[6 * i + 4] = new Vector2(0.0f, y);
                trackUV[6 * i + 5] = new Vector2(0.0f, y);
            }
        }

        for (int j = 0; j < 30; ++j)
        {
            if (trackTriangles[30 * (subdivided.Count - 1) + j] > subdivided.Count * 6 - 1)
            {
                trackTriangles[30 * (subdivided.Count - 1) + j] = trackIndices[j] - 6;
            }
        }

        Mesh trackMesh = new Mesh();
        trackMesh.vertices = trackVertices;
        trackMesh.uv = trackUV;
        trackMesh.triangles = trackTriangles;

        GetComponent<MeshFilter>().mesh = trackMesh;
        GetComponent<MeshCollider>().sharedMesh = trackMesh;
    }

    public Vector3 getItemRandomItemPosition()
    {
        int randomCurve = UnityEngine.Random.Range(0, segments.Count);

        float randomT = UnityEngine.Random.value;
        float randomOffset = UnityEngine.Random.value * 2.0f - 1.0f;

        Vector3 position = segments[randomCurve].deCasteljau(randomT);// + transform.localPosition;
        Vector3 offset = getLeft(segments[randomCurve].tangent(randomT), Vector3.up) * randomOffset * trackHalfWidth;
        
        return transform.TransformPoint(position + offset);
    }

    private Vector3 getLeft(Vector3 forward, Vector3 up)
    {
        return Vector3.Cross(forward, up).normalized;
    }
}
