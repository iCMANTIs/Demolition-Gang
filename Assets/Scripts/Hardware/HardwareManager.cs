using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class HardwareManager : DontDestroySingleton<HardwareManager>
{
    private bool isButtonPressed;
    private bool buttonPressedThisFrame;
    private bool buttonReleasedThisFrame;

    public Action<string> action;
    public Action<string> actionIgnite;
    public Action OnStick2ChangeAction;


    private float joystick1;
    private float joystick2;

    public float Joystick1 { get { return joystick1; } }
    public float Joystick2 { get { return joystick2; } }




    protected override void Awake()
    {
        base.Awake();

        action += Test;
        actionIgnite += Test;

    }

    protected override void Start()
    {
        base.Start();

        //Initialize parameters
        isButtonPressed = false;
        buttonPressedThisFrame = false;
        buttonReleasedThisFrame = false;

        UduinoManager.Instance.OnDataReceived += OnDataReceived;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        //if (buttonPressedThisFrame)
        //    Debug.Log("KeyDown");
        //if (buttonReleasedThisFrame)
        //    Debug.LogWarning("KeyUp");
        //if (isButtonPressed)
        //    Debug.LogError("Getkey");


        buttonPressedThisFrame = false;
        buttonReleasedThisFrame = false;

        if (Input.GetKeyDown("space"))
        {
            LightUp(100);
        }
    }

    /* All possbile data messages from hardware:
     * "P": send at the moment when user press the button
     * "G": send at the moment when user release the button
     * "(integer)": send when user rotate the joystick rotator
     * "IG + (integer)": send when user rotate the side rotator
     */
    void OnDataReceived(string data, UduinoDevice board)
    {
        if (data == "P")
        {
            buttonPressedThisFrame = true;
            isButtonPressed = true;
        }
        else if (data == "G")
        {
            buttonReleasedThisFrame = true;
            isButtonPressed = false;
        }
        else if (data.Length > 2 && data.Substring(0, 2) == "IG")
        {
            OnStick2ChangeAction.Invoke();

            actionIgnite.Invoke(data);
            UpdateJoystick2(data);
        }
        else
        {
            action.Invoke(data);
            UpdateJoystick1(data);
        }
    }


    private void UpdateJoystick1(string data)
    {
        float value = float.Parse(data);
        float factor = 0.5f;        // Factor = [(max - min) / original_scale] * new_scale = (0 + 4) / 5 * 2
        float offset = 1.0f;        // Offset = max * factor - 1

        value = value * factor + offset;
        joystick1 = value;

        //Debug.Log($"Old stick value {data}, New stick value {value}");
    }

    private void UpdateJoystick2(string data)
    {
        float value = float.Parse(data.Substring(2)); ;
        joystick2 = value;


        Debug.Log($"Hardware stick 2 value {value}");
    }

    //Return true during the frame that the key specified by the parameter is pressed.
    public bool GetKeyDown(string key)
    {
        return buttonPressedThisFrame;
    }

    //Return true during the frame that the key specified by the parameter is released.
    public bool GetKeyUp(string key)
    {
        return buttonReleasedThisFrame;
    }

    //Return true while the key specified by the parameter is holding down.
    public bool GetKey(string key)
    {
        return isButtonPressed;
    }

    public void LightUp(int lightUpTime)
    {
        UduinoManager.Instance.sendCommand("lightUp", lightUpTime);
    }

    void Test(string str)
    {
        //Debug.Log(str);
    }
}
