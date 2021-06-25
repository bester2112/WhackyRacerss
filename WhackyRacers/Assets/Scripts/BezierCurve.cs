using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BezierCurve {

    [SerializeField]
    public Vector3[] controlPoints;

    public BezierCurve(Vector3 origin)
    {
        controlPoints = new Vector3[4] {origin + new Vector3(1.0f, 0.0f, 0.0f), origin + new Vector3(0.0f, 0.0f, 1.0f), origin + new Vector3(0.0f, 0.0f, -1.0f), origin + new Vector3(-1.0f, 0.0f, 0.0f) };
    }

    public BezierCurve(Vector3 aaa, Vector3 aab, Vector3 abb, Vector3 bbb)
    {
        controlPoints = new Vector3[4] { aaa, aab, abb, bbb };
    }

    public Vector3 deCasteljau(float t)
    {
        Vector3 aaa = controlPoints[0];
        Vector3 aab = controlPoints[1];
        Vector3 abb = controlPoints[2];
        Vector3 bbb = controlPoints[3];

        Vector3 aac = Vector3.Lerp(aaa, aab, t);
        Vector3 abc = Vector3.Lerp(aab, abb, t);
        Vector3 bbc = Vector3.Lerp(abb, bbb, t);

        Vector3 acc = Vector3.Lerp(aac, abc, t);
        Vector3 bcc = Vector3.Lerp(abc, bbc, t);

        Vector3 ccc = Vector3.Lerp(acc, bcc, t);

        return ccc;
    }

    public Vector3 tangent(float t)
    {
        Vector3 aaa = controlPoints[0];
        Vector3 aab = controlPoints[1];
        Vector3 abb = controlPoints[2];
        Vector3 bbb = controlPoints[3];

        Vector3 aac = Vector3.Lerp(aaa, aab, t);
        Vector3 abc = Vector3.Lerp(aab, abb, t);
        Vector3 bbc = Vector3.Lerp(abb, bbb, t);

        Vector3 acc = Vector3.Lerp(aac, abc, t);
        Vector3 bcc = Vector3.Lerp(abc, bbc, t);

        return (bcc - acc).normalized;
    }

    public void subdivide(float t, out BezierCurve a, out BezierCurve b)
    {
        Vector3 aaa = controlPoints[0];
        Vector3 aab = controlPoints[1];
        Vector3 abb = controlPoints[2];
        Vector3 bbb = controlPoints[3];

        Vector3 aac = Vector3.Lerp(aaa, aab, t);
        Vector3 abc = Vector3.Lerp(aab, abb, t);
        Vector3 bbc = Vector3.Lerp(abb, bbb, t);

        Vector3 acc = Vector3.Lerp(aac, abc, t);
        Vector3 bcc = Vector3.Lerp(abc, bbc, t);

        Vector3 ccc = Vector3.Lerp(acc, bcc, t);

        a = new BezierCurve(aaa, aac, acc, ccc);
        b = new BezierCurve(ccc, bcc, bbc, bbb);
    }

    public float curvature()
    {
        Vector3 db0 = controlPoints[1] - controlPoints[0];
        Vector3 d2b0 = controlPoints[2] - 2.0f * controlPoints[1] + controlPoints[0];
        return 1.0f / (2.0f / 3.0f) * (Vector3.Cross(db0, d2b0).magnitude / (db0.magnitude * db0.magnitude * db0.magnitude));
    }
}
