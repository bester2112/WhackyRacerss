using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : NetworkBehaviour
{
    public int laps = 5;

    public GameObject itemPrefab;
    private GameObject items;

    private RaceTrack raceTrack;

    Rigidbody rigidbodyTable;

    public float turnTime = 1.0f;

    public float randomTime = 15.0f;
    public float randomRotation = 0.0f;
    public Quaternion randomQuaternion = Quaternion.identity;
    //private float[] randomRotations = new float[7] { 90.0f, 180.0f, 270.0f, 0.0f, -90.0f, -180.0f, -270.0f };
    //private float[] turnTimes = new float[7]       {  1.0f,   2.0f,   3.0f, 1.0f,   1.0f,    2.0f,    3.0f };
    private float[] randomRotations = new float[3] { 90.0f, 180.0f, -90.0f };
    private float[] turnTimes = new float[3] { 2.5f, 5.0f, 2.5f};

    public NetworkVariableQuaternion rotation = new NetworkVariableQuaternion(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    Quaternion oldRot = Quaternion.identity;
    float t = 0.0f;

    public float itemTimer = 10.0f;
    private float nextItemDelta = 10.0f;
    private int maxItems = 6;

    public NetworkVariableBool activeRace = new NetworkVariableBool(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public float startTimer = 0.0f;
    public float nextStartDelta = 15.0f;
    public float spawnNewCarsMinusDelta = 5.0f;
    public float resetRotationMinusDelta = 10.0f;

    private bool resetRotation = false;

    public bool turned = false;

    // Start is called before the first frame update
    void Start()
    {
        rigidbodyTable = GetComponent<Rigidbody>();
        items = GameObject.Find("Items");
        raceTrack = GameObject.Find("RaceTrack").GetComponent<RaceTrack>();

        if (IsOwner)
        {
            GameObject.Find("Gui").GetComponent<GuiManager>().tableManager = this;
        }
    }

    public override void NetworkStart()
    {
        if (IsClient)
        {
            if (rigidbodyTable == null)
            {
                rigidbodyTable = GetComponent<Rigidbody>();
            }

            if (!IsServer)
            {
                rigidbodyTable.rotation = rotation.Value;
                oldRot = rotation.Value;
            }
        }
    }

    [ClientRpc]
    public void turnTableClientRpc(int randomIndex)
    {
        randomRotation = randomRotations[randomIndex];
        randomQuaternion = Quaternion.AngleAxis(randomRotation, Vector3.up);

        turnTime = turnTimes[randomIndex];

        oldRot = rigidbodyTable.rotation;
        t = 0.0f;
        turned = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (activeRace.Value)
        {
            rotateTable();
            spawnItem();
        }
        else
        {
            if (IsServer)
            {
                if (resetRotation && Time.time > startTimer - resetRotationMinusDelta)
                {
                    resetRotation = false;
                    resetTableRotation();
                }

                if (Time.time > startTimer)
                {
                    activeRace.Value = true;
                }
            }
        }
    }

    private void rotateTable()
    {
        if (IsServer)
        {
            rotation.Value = rigidbodyTable.rotation;

            if (Time.time >= randomTime)
            {
                int randomIndex = Random.Range(0, 3);

                randomRotation = randomRotations[randomIndex];
                Debug.Log(randomRotation);
                randomQuaternion = Quaternion.AngleAxis(randomRotation, Vector3.up);

                turnTime = turnTimes[randomIndex];

                oldRot = rigidbodyTable.rotation;
                t = 0.0f;

                randomTime += Random.Range(15.0f, 25.0f) + turnTime;

                turnTableClientRpc(randomIndex);
            }
        }

        Quaternion deltaRotation = Quaternion.Slerp(Quaternion.identity, randomQuaternion, t / turnTime);

        rigidbodyTable.MoveRotation(oldRot * deltaRotation);

        t += Time.fixedDeltaTime;
    }

    public void spawnItem()
    {
        if (IsServer)
        {
            if (Time.time >= itemTimer)
            {
                if (items.transform.childCount < maxItems)
                {
                    GameObject itemObject = Instantiate(itemPrefab, raceTrack.getItemRandomItemPosition() + new Vector3(0.0f, 0.2f, 0.0f), Quaternion.identity, items.transform);
                    itemObject.GetComponent<NetworkObject>().Spawn();
                }

                itemTimer += nextItemDelta;
            }
        }
    }

    public void resetTable()
    {
        activeRace.Value = false;

        resetValues();

        resetTableClientRpc();
    }

    [ClientRpc]
    public void resetTableClientRpc()
    {
        resetValues();

        GameObject.Find("InputManager").GetComponent<InputManager>().switched = Vector2.one;
        GameObject.Find("InputManager").GetComponent<InputManager>().rotation = 0.0f;
    }

    private void resetValues()
    {
        turnTime = 1.0f;
        randomTime = Time.time + nextStartDelta + 15.0f;
        randomRotation = 0.0f;
        randomQuaternion = Quaternion.identity;
        oldRot = Quaternion.identity;
        t = 0.0f;
        itemTimer = Time.time + nextStartDelta + 10.0f;
        startTimer = Time.time + nextStartDelta;
        resetRotation = true;
        turned = false;
    }

    private void resetTableRotation()
    {
        rotation.Value = Quaternion.identity;
        rigidbodyTable.MoveRotation(oldRot);
        resetTableRotationClientRpc();
    }

    [ClientRpc]
    public void resetTableRotationClientRpc()
    {
        rigidbodyTable.MoveRotation(oldRot);
    }
}
