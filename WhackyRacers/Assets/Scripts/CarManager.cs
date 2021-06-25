using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;


public class CarManager : NetworkBehaviour
{
    public int currentLap = 0;
    public int nextCheckPoint = 0;
    public int item = 0;

    public CheckPointManager checkPointManager;
    private InputManager inputManager;

    private Color[] colors = new Color[] { new Color(193.0f / 255.0f, 28.0f / 255.0f, 28.0f / 255.0f, 1.0f),
                                            new Color(113.0f / 255.0f, 203.0f / 255.0f, 106.0f / 255.0f, 1.0f),
                                            new Color(203.0f / 255.0f, 106.0f / 255.0f, 184.0f / 255.0f, 1.0f),
                                            new Color(106.0f / 255.0f, 128.0f / 255.0f, 203.0f / 255.0f, 1.0f),
                                            new Color(251.0f / 255.0f, 71.0f / 255.0f, 10.0f / 255.0f, 1.0f),
                                            new Color(50.0f / 255.0f, 69.0f / 255.0f, 254.0f / 255.0f, 1.0f)};

    public NetworkVariableBool requestReset = new NetworkVariableBool(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariableInt lastCheckPoint = new NetworkVariableInt(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public void Start()
    {
        checkPointManager = GameObject.Find("Checkpoints").GetComponent<CheckPointManager>();
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();

        if (IsOwner)
        {
            GameObject.Find("Gui").GetComponent<GuiManager>().carManager = this;
        }

        transform.GetChild(2).GetChild(1).GetComponent<Renderer>().material.color = colors[GetComponent<NetworkObject>().OwnerClientId - 2];

        if (IsOwner)
        {
            requestReset.Value = false;
            lastCheckPoint.Value = checkPointManager.checkPointsCount - 1;
        }
    }

    [ClientRpc]
    public void updateLastCheckPointClientRpc(int lastCheck, int netLap)
    {
        if (IsOwner)
        {
            lastCheckPoint.Value = lastCheck;
            nextCheckPoint = lastCheckPoint.Value + 1;
            if (nextCheckPoint >= checkPointManager.checkPointsCount)
            {
                nextCheckPoint = 0;
            }
        }
        currentLap = netLap;
    }

    private void Update()
    {
        if (inputManager.buttons[0])
        {
            useItem();
        }
        if (inputManager.buttons[1])
        {
            sendItem();
        }
        if (inputManager.buttons[2])
        {
            if (IsOwner)
            {
                requestReset.Value = true;
            }
        }
    }

    public void hitCheckPoint(int checkPointid)
    {
        if (checkPointid == nextCheckPoint)
        {
            if (nextCheckPoint == 0)
            {
                currentLap++;
            }

            nextCheckPoint++;

            if (nextCheckPoint >= checkPointManager.checkPointsCount)
            {
                nextCheckPoint = 0;
            }

            if (IsOwner)
            {
                lastCheckPoint.Value++;

                if (lastCheckPoint.Value >= checkPointManager.checkPointsCount)
                {
                    lastCheckPoint.Value = 0;
                }
            }
        }
    }

    public void hitItem(int itemType)
    {
        if (IsOwner)
        {
            item = itemType;
        }
    }

    private void useItem()
    {
        if (item != 0)
        {
            activateItem(item);
            item = 0;
        }
    }

    private void sendItem()
    {
        if (item != 0)
        {
            requestItemActivatedServerRpc(item);
            item = 0;
        }
    }

    [ServerRpc]
    public void requestItemActivatedServerRpc(int itemSend)
    {
        sendItemClientRPC(itemSend);
    }

    [ClientRpc]
    public void sendItemClientRPC(int itemSend)
    {
        activateItem(itemSend);
    }

    private void activateItem(int itemSend)
    {
        if (inputManager.switched.Equals(Vector2.zero))
        {
            inputManager.switched = Vector2.one;
        }

        switch (itemSend)
        {
            case 1:
                inputManager.switched = Vector2.zero;
                break;
            case 2:
                inputManager.switched.x = -inputManager.switched.x;
                break;
            case 3:
                inputManager.switched.y = -inputManager.switched.y;
                break;
            case 4:
                inputManager.switched = -inputManager.switched;
                break;
            default:
                break;
        }
    }
}
