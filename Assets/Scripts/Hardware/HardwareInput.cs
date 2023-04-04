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

    //New block of code added
    private bool isWireConnected;
    private bool wireConnectedThisFrame;
    private bool wireDisconnectedThisFrame;

    public Action<string> action;
    public Action<string> actionIgnite;


    // Start is called before the first frame update
    void Start()
    {
        //Initialize parameters
        isButtonPressed = false;
        buttonPressedThisFrame = false;
        buttonReleasedThisFrame = false;

        //New block of code added
        isWireConnected = false;
        wireConnectedThisFrame = false;
        wireDisconnectedThisFrame = false;

        UduinoManager.Instance.OnDataReceived += DataReceived;
        Application.targetFrameRate = 60;
    }

    //There are five kinds of data that will be received from the hardware:
    //1. A single letter, "P", passed by a string, which indicates that the button is pressed.
    //2. A single letter, "G", passed by a string, which indicates that the button is released.
    //3. A string, started with "C" and followed by "T" or "F", which indicates if the wire is connected (true or false);
    //4. A string, started with "IG" and followed immediately by an integer, which indicates how far the ignition lever is turned away (compared with its initial position, "0");
    //5. An integer passed by a string, which indicates how far the lever is pushed up (compared with its initial position, "0");
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
        //New block of code added
        else if (data.Length == 2 && data.Substring(0, 1) == "C")
        {
            if (data.Substring(1) == "T")
            {
                wireConnectedThisFrame = true;
                isWireConnected = true;
            }
            else if (data.Substring(1) == "F")
            {
                wireDisconnectedThisFrame = true;
                isWireConnected = false;
            }
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

    //New block of code added
    //Return true during the frame that the wire is connected.
    public bool GetWireConnected()
    {
        return wireConnectedThisFrame;
    }

    //New block of code added
    //Return true during the frame that the wire is disconnected.
    public bool GetWireDisconnected()
    {
        return wireDisconnectedThisFrame;
    }

    //New block of code added
    //Return true while wire is connected.
    public bool GetWireStatus()
    {
        return isWireConnected;
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

        //New block of code added
        if (wireConnectedThisFrame)
            Debug.Log("WireConnected");
        if (wireDisconnectedThisFrame)
            Debug.LogWarning("WireDisconnected");
        if (isWireConnected)
            Debug.LogError("CircuitClosed");


        buttonPressedThisFrame = false;
        buttonReleasedThisFrame = false;

        //New block of code added
        wireConnectedThisFrame = false;
        wireDisconnectedThisFrame = false;

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
