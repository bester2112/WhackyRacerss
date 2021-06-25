using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    public GameObject[] checkPoints;
    public int checkPointsCount;
    public float maxCheckPointDifference;


    // Start is called before the first frame update
    void Start()
    {
        checkPointsCount = transform.childCount;
        checkPoints = new GameObject[checkPointsCount];

        for (int i = 0; i < checkPointsCount; ++i)
        {
            checkPoints[i] = transform.GetChild(i).gameObject;
        }

        for (int i = 1; i < checkPointsCount; ++i)
        {
            maxCheckPointDifference = Mathf.Max(maxCheckPointDifference, (checkPoints[i].transform.position - checkPoints[i - 1].transform.position).magnitude);
        }
    }

    public GameObject getNextCheckPoint(int index)
    {
        if (index >= checkPointsCount)
        {
            return checkPoints[index - checkPointsCount];
        } else
        {
            return checkPoints[index];
        }
    }
}
