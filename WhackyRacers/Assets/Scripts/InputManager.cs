using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Vector2 inputs;
    public Vector2 switched = Vector2.one;
    public float rotation = 0.0f;

    public bool[] buttons = new bool[3];
    private float lastResetTime = 0.0f;
    private float resetCooldownTime = 5.0f;

    private TableManager tableManager;

    private void Start()
    {
        tableManager = GameObject.Find("Table").GetComponent<TableManager>();
    }

    // Update is called once per frame
    void Update()
    {
        inputs.x = Input.GetAxis("Horizontal");
        inputs.y = Input.GetAxis("Vertical");

        buttons[0] = Input.GetAxis("Fire1") > 0.0f;
        buttons[1] = Input.GetAxis("Fire2") > 0.0f;

        if (Time.time > lastResetTime && Input.GetAxis("Fire3") > 0.0f)
        {
            buttons[2] = true;
            lastResetTime = Time.time + resetCooldownTime;
        }
        else
        {
            buttons[2] = false;
        }

        if (!switched.Equals(Vector2.zero))
        {
            if (tableManager.turned)
            {
                rotation += tableManager.randomRotation;
                tableManager.turned = false;
            }
            inputs = Quaternion.Euler(0.0f, 0.0f, rotation) * inputs * switched;
        } else
        {
            rotation = 0.0f;
        }

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }
}
