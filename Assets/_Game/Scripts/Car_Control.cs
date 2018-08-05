using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_Control : MonoBehaviour
{

    public float MotorForce, SteerForce, BrakeForce;
    public WheelCollider FR_L_Wheel, FR_R_Wheel, RE_L_Wheel, RE_R_Wheel;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float v = Input.GetAxis("Acelerate") * MotorForce;

        if(Input.GetKey(KeyCode.JoystickButton1))
        {
        	v  = Input.GetAxis("Brake") * MotorForce;
        	Debug.Log(v);
        }

        float h = Input.GetAxis("Horizontal") * SteerForce;

        RE_R_Wheel.motorTorque = v;
        RE_L_Wheel.motorTorque = v;

        FR_L_Wheel.steerAngle = h;
        FR_R_Wheel.steerAngle = h;

        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton1) )
        {
            RE_R_Wheel.brakeTorque = BrakeForce;
            RE_L_Wheel.brakeTorque = BrakeForce;
        }
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton1) )
        {
            RE_R_Wheel.brakeTorque = 0;
            RE_L_Wheel.brakeTorque = 0;
        }
        if (Input.GetAxis("Acelerate") == 0)
        {
            RE_R_Wheel.brakeTorque = BrakeForce;
            RE_L_Wheel.brakeTorque = BrakeForce;

        }
        else
        {
            RE_R_Wheel.brakeTorque = 0;
            RE_L_Wheel.brakeTorque = 0;
        }

    }


}
