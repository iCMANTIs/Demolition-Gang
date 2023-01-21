using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    public Rigidbody rigidBody1;
    public Rigidbody rigidBody2;

    private string[] stickNames = 
    {
        "LeftJoyStickS1",
        "LeftJoyStickS2",
        "LeftJoyStickS3",
        "RightJoyStickS1",
        "RightJoyStickS2",
        "RightJoyStickS3",
    };

    enum StickState { ACCELERATE, FORWARD, IDLE, BACKWARD, DECELERATE }
    StickState leftStick1 = StickState.IDLE;
    StickState leftStick2 = StickState.IDLE;
    StickState leftStick3 = StickState.IDLE;
    StickState rightStick1 = StickState.IDLE;
    StickState rightStick2 = StickState.IDLE;
    StickState rightStick3 = StickState.IDLE;




    private void Awake()
    {
    }


    // Update is called once per frame 
    void Update()
    {
        UpdateJoyStickState(ref leftStick1, stickNames[0]);
        UpdateJoyStickState(ref leftStick2, stickNames[1]);
        UpdateJoyStickState(ref leftStick3, stickNames[2]);
        UpdateJoyStickState(ref rightStick1, stickNames[3]);
        UpdateJoyStickState(ref rightStick2, stickNames[4]);
        UpdateJoyStickState(ref rightStick3, stickNames[5]);
    }



    private void UpdateJoyStickState(ref StickState stick, string stickName)
    {
        float value = Input.GetAxis(stickName);

        //Debug.Log($"Stick {stickName} value {value}");

        if (value <= 1 && value > 0.6)
            stick = StickState.ACCELERATE;
        else if (value <= 0.6 && value > 0.2)
            stick = StickState.FORWARD;
        else if (value <= 0.2 && value > -0.2)
            stick = StickState.IDLE;
        else if (value <= -0.2 && value > -0.6)
            stick = StickState.BACKWARD;
        else if (value <= -0.6 && value >= -1)
            stick = StickState.DECELERATE;
    }






    // Show some data 
    void OnGUI()
    {
        GUI.TextArea(new Rect(0, 40, 250, 40), "Current Horizontal Axis : " + Input.GetAxis("Horizontal"));
        GUI.TextArea(new Rect(0, 80, 250, 40), "Current Vertical Axis : " + Input.GetAxis("Vertical"));
        GUI.TextArea(new Rect(0, 120, 250, 40), "Current Fire1 Axis : " + Input.GetAxis("LeftJoyStickS1"));
        GUI.TextArea(new Rect(0, 160, 250, 40), "Current Fire2 Axis : " + Input.GetAxis("LeftJoyStickS2"));
        GUI.TextArea(new Rect(0, 200, 250, 40), "Current Fire3 Axis : " + Input.GetAxis("LeftJoyStickS3"));
        GUI.TextArea(new Rect(0, 240, 250, 40), "Left stick : " + leftStick1.ToString() + " : " + leftStick2.ToString());
        GUI.TextArea(new Rect(0, 280, 250, 40), "Right stick : " + rightStick1.ToString() + " : " + rightStick2.ToString());
        GUI.TextArea(new Rect(0, 320, 250, 40), "Current Joy Button 0 : " + Input.GetAxis("LeftJoyStickB0"));
    }



}
