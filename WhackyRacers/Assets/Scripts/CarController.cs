using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class CarController : NetworkBehaviour
{
    public Transform centerofMass;

    public Transform[] wheelTransforms;
    public WheelCollider[] wheelColliders;

    public float motorTorque = 400.0f;
    public float steerAngle = 40.0f;

    public NetworkVariableBool activeRace = new NetworkVariableBool(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = centerofMass.localPosition;
    }

    //Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            if (!activeRace.Value)
            {
                return;
            }

            Vector2 inputs = GameObject.Find("InputManager").GetComponent<InputManager>().inputs;

            wheelColliders[0].steerAngle = inputs.x * steerAngle * Time.deltaTime * 1000.0f;
            wheelColliders[1].steerAngle = inputs.x * steerAngle * Time.deltaTime * 1000.0f;
            wheelColliders[2].motorTorque = inputs.y * motorTorque * Time.deltaTime * 1000.0f;
            wheelColliders[3].motorTorque = inputs.y * motorTorque * Time.deltaTime * 1000.0f;

            Vector3 pos;
            Quaternion quat;

            for (int i = 0; i < 4; ++i)
            {
                wheelColliders[i].GetWorldPose(out pos, out quat);
                wheelTransforms[i].position = pos;
                wheelTransforms[i].rotation = quat;
            }
        }
    }
}
