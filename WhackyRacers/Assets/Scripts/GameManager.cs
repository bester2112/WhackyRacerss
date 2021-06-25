using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using System;

public class GameManager : NetworkBehaviour
{
    public GameObject carPrefab;

    private GameObject startPositions;
    private GameObject cars;
    private GameObject checkPoints;

    private TableManager tableManager;

    private bool raceActive = true;
    private bool spawnedCars = false;

    // Start is called before the first frame update
    void Start()
    {
        startPositions = GameObject.Find("StartPositions");
        cars = GameObject.Find("Cars");
        checkPoints = GameObject.Find("Checkpoints");
        tableManager = GameObject.Find("Table").GetComponent<TableManager>();
    }

    public override void NetworkStart()
    {
        if (IsClient)
        {
            requestCarServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc]
    private void requestCarServerRpc(ulong localClientId)
    {
        spawnCar(localClientId, false);
        setCarActive(localClientId, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            if (raceActive)
            {
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    if (client.PlayerObject.TryGetComponent<CarManager>(out var carManager))
                    {
                        if (carManager.requestReset.Value)
                        {
                            spawnCar(client.ClientId, true);
                            setCarActive(client.ClientId, true);
                        }
                    }
                }

                for (int i = 0; i < cars.transform.childCount; ++i)
                {
                    CarManager carManager = cars.transform.GetChild(i).GetComponent<CarManager>();

                    if (carManager.currentLap > tableManager.laps)
                    {
                        restartRace();
                    }
                }
            }
            else
            {
                if (tableManager.activeRace.Value)
                {
                    raceActive = true;

                    foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                    {
                        setCarActive(client.ClientId, true);
                    }
                }
                else
                {
                    if (Time.time >= tableManager.startTimer - tableManager.spawnNewCarsMinusDelta && !spawnedCars)
                    {
                        spawnedCars = true;
                        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                        {
                            spawnCar(client.ClientId, false);
                            setCarActive(client.ClientId, false);
                        }
                    }
                }
            }
        }
    }

    private void restartRace()
    {
        raceActive = false;
        spawnedCars = false;

        tableManager.resetTable();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            setCarActive(client.ClientId, false);
        }
    }

    private void spawnCar(ulong localClientId, bool isRespawn)
    {
        int lastCheck = 0;
        int lap = 0;

        Vector3 position = startPositions.transform.GetChild((int)localClientId - 2).position;
        Quaternion rotation = startPositions.transform.GetChild((int)localClientId - 2).rotation * Quaternion.AngleAxis(90.0f, Vector3.up);

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(localClientId, out var client))
        {
            if (isRespawn)
            {
                if (client.PlayerObject.TryGetComponent<CarManager>(out var carManager))
                {
                    position = checkPoints.transform.GetChild(carManager.lastCheckPoint.Value).position;
                    rotation = checkPoints.transform.GetChild(carManager.lastCheckPoint.Value).rotation;
                    lastCheck = carManager.lastCheckPoint.Value;
                    lap = carManager.currentLap;
                }
            }

            if (client.PlayerObject.IsSpawned)
            {
                client.PlayerObject.Despawn(true);
            }
        }

        GameObject clientCar = Instantiate(carPrefab, position + Vector3.up * 0.15f, rotation, cars.transform);
        clientCar.GetComponent<NetworkObject>().SpawnAsPlayerObject(localClientId);

        if (isRespawn)
        {
            clientCar.GetComponent<CarManager>().updateLastCheckPointClientRpc(lastCheck, lap);
            clientCar.GetComponent<CarManager>().currentLap = lap;
            clientCar.GetComponent<CarManager>().nextCheckPoint = lastCheck + 1;
            if (clientCar.GetComponent<CarManager>().nextCheckPoint >= checkPoints.GetComponent<CheckPointManager>().checkPointsCount)
            {
                clientCar.GetComponent<CarManager>().nextCheckPoint = 0;
            }
        }
    }

    private void setCarActive(ulong localClientId, bool value)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(localClientId, out var client))
        {
            client.PlayerObject.GetComponent<CarController>().activeRace.Value = value;
        }
    }
}
