using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class HardwareInput : MonoBehaviour
{
    private bool isButtonPressed;
    private bool buttonPressedThisFrame;
    private bool buttonReleasedThisFrame;

    public Action<string> action;
    public Action<string> actionIgnite;

    // Start is called before the first frame update
    void Start()
    {
        //Initialize parameters
        isButtonPressed = false;
        buttonPressedThisFrame = false;
        buttonReleasedThisFrame = false;

        UduinoManager.Instance.OnDataReceived += DataReceived;
        Application.targetFrameRate = 60;
    }

    //There are two kinds of data that will be recieved from the hardware:
    //1. An interger passed by a string, which indicates how far the lever is pushed up (compared with its initial position, "0");
    //2. A single letter, "P", passed by a string, which indicates that the button is pressed.
    void DataReceived(string data, UduinoDevice board)
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
        else if (data.Length > 2 && data.Substring(0,2) == "IG" )
        {
            actionIgnite.Invoke(data.Substring(2) + "I");
        }
        else
        {
            action.Invoke(data);
        }
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

    // Update is called once per frame
    void Update()
    {

        if (buttonPressedThisFrame)
            Debug.Log("KeyDown");
        if (buttonReleasedThisFrame)
            Debug.LogWarning("KeyUp");
        if (isButtonPressed)
            Debug.LogError("Getkey");


        buttonPressedThisFrame = false;
        buttonReleasedThisFrame = false;

        if (Input.GetKeyDown("space"))
        {
            LightUp(100);
        }
    }

    void Awake()
    {
        action += Test;
        actionIgnite += Test;
    }


    void Test(string str)
    {
        Debug.Log(str);
    }
}
