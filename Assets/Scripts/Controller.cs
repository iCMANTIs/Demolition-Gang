using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{

    private string currentButton;
    private string currentAxis;
 
    // Use this for initialization 
    void Start()
    {
 
    }
    // Update is called once per frame 
    void Update()
    {
        var values = Enum.GetValues(typeof(KeyCode));
        for (int x = 0; x < values.Length; x++)
        {
            if (Input.GetKeyDown((KeyCode)values.GetValue(x)))
            {
                currentButton = values.GetValue(x).ToString();
            }
        }

        

    }



    // Show some data 
    void OnGUI()
    {
        GUI.TextArea(new Rect(0, 0, 250, 40), "Current Button : " + currentButton);
        GUI.TextArea(new Rect(0, 40, 250, 40), "Current Left X Axis : " + Input.GetAxis("Horizontal"));
        GUI.TextArea(new Rect(0, 80, 250, 40), "Current Left Y Axis : " + Input.GetAxis("Vertical"));
        GUI.TextArea(new Rect(0, 120, 250, 40), "Current Right X Axis : " + Input.GetAxis("MouseX"));
        GUI.TextArea(new Rect(0, 160, 250, 40), "Current Right Y Axis : " + Input.GetAxis("MouseY"));
    }



}
