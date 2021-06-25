using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;

public class GuiManager : MonoBehaviour
{
    public Text text;
    public RawImage rawImage;

    public RawImage[] items;

    [HideInInspector]
    public CarManager carManager;
    [HideInInspector]
    public TableManager tableManager;
    public InputManager inputManager;

    GameObject gameGui;
    GameObject connectGui;
    GameObject restartGui;

    public Text restartText;

    private bool connected = false;

    // Start is called before the first frame update
    void Start()
    {
        gameGui = GameObject.Find("GameGui");
        connectGui = GameObject.Find("ConnectGui");
        restartGui = GameObject.Find("RestartGui");
    }

    // Update is called once per frame
    void Update()
    {
        if (!connected)
        {
            gameGui.SetActive(false);
            connectGui.SetActive(true);
            restartGui.SetActive(false);

            return;
        }

        if (!tableManager.activeRace.Value)
        {
            gameGui.SetActive(false);
            connectGui.SetActive(false);
            restartGui.SetActive(true);

            restartText.text = "Restart in: " + (int)(tableManager.startTimer - Time.time);

            return;
        }


        if (carManager != null)
        {
            gameGui.SetActive(true);
            connectGui.SetActive(false);
            restartGui.SetActive(false);

            text.text = "Lap: " + carManager.currentLap + "/" + tableManager.laps + "\n"
                     + "Checkpoint: " + (carManager.nextCheckPoint == 0 ? carManager.currentLap == 0 ? 0 : carManager.checkPointManager.checkPointsCount : carManager.nextCheckPoint) + "/" + (carManager.checkPointManager.checkPointsCount) + "\n"
                     + "Item: ";

            for (int i = 0; i < 5; ++i)
            {
                items[i].gameObject.SetActive(false);
            }
            items[carManager.item].gameObject.SetActive(true);

            if (!inputManager.switched.Equals(Vector2.zero))
            {
                rawImage.transform.localScale = inputManager.switched;
                rawImage.transform.rotation = Quaternion.AngleAxis(-inputManager.rotation, Vector3.forward);
            }
            else
            {
                rawImage.transform.localScale = Vector2.one;
                rawImage.transform.rotation = Quaternion.identity;
            }
        }
    }

    public void connectButton()
    {
        connected = true;
        NetworkManager.Singleton.StartClient();
    }
}
