using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class ItemManager : NetworkBehaviour
{
    public int itemType;

    // Start is called before the first frame update
    void Start()
    {
        itemType = Random.Range(1, 5);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = transform.rotation * Quaternion.Euler(Vector3.up * Time.deltaTime * 20.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Equals("BodyBottom"))
        {
            if (other.transform.parent.parent.TryGetComponent<CarManager>(out CarManager carManager))
            {
                carManager.hitItem(itemType);

                if (IsServer)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
