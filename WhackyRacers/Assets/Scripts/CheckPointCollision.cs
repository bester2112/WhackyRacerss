using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointCollision : MonoBehaviour
{
    public int checkPointId;

    private void OnTriggerEnter(Collider other)
    {
        if(other.name.Equals("BodyBottom"))
        {
            if (other.transform.parent.parent.TryGetComponent<CarManager>(out CarManager carManager))
            {
                carManager.hitCheckPoint(checkPointId);
            }
        }
    }
}
