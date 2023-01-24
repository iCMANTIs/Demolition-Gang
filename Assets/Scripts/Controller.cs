using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    public float speedDamp = 0.10f;

    public Camera cameraTPS;
    public Camera cameraFPS;

    public GameObject cab;
    public GameObject boom;

    public AudioClip engine;
    public AudioClip collision;
    public AudioSource audioSource;

    private string[] stickNames = 
    {
        "LeftJoyStickS1",
        "LeftJoyStickS2",
        "LeftJoyStickS3",
        "RightJoyStickS1",
        "RightJoyStickS2",
        "RightJoyStickS3",
    };

    enum StickState { ACCELERATE = 30, FORWARD = 15, IDLE = 0, BACKWARD = -15, DECELERATE = -30 }
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

        UpdateCamera();
        UpdateExcavatorMovement();
        UpdateExcavatorRotation();
        UpdateCabRotation();
        UpdateBoomRotation();


        UpdateEngineSoundEffect();


        UpdateMovementTemp();
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


    private void UpdateExcavatorMovement()
    {
        Vector3 direction = Vector3.zero;
        float speed = Mathf.Abs((int)leftStick1 + (int)rightStick1) * speedDamp * Time.deltaTime;

        if ((int)leftStick1 > 0 && (int)rightStick1 > 0)
            direction = transform.right;
        else if ((int)leftStick1 < 0 && (int)rightStick1 < 0)
            direction = transform.right * -1;


        transform.Translate(direction * speed, Space.World);
    }


    private void UpdateExcavatorRotation()
    {
        float angularSpeed = Mathf.Abs((int)leftStick1 - (int)rightStick1) * Time.deltaTime;
        Vector3 axis = Vector3.up;

        if ((int)leftStick1 - (int)rightStick1 > 0)
            transform.Rotate(axis, angularSpeed);
        else if ((int)leftStick1 - (int)rightStick1 < 0)
            transform.Rotate(axis, angularSpeed * -1);
    }


    private void UpdateCabRotation()
    {
        float angularSpeed = (int)leftStick2 * Time.deltaTime;
        Vector3 axis = cab.transform.up; 
        cab.transform.Rotate(axis, angularSpeed, Space.World);
    }


    private void UpdateBoomRotation()
    {
        float angularSpeed = (int)leftStick3 * Time.deltaTime;
        Vector3 axis = boom.transform.forward;
        boom.transform.Rotate(axis, angularSpeed, Space.World);

        /* Limit boom rotation within specific angle */
        Vector3 eularAngle = boom.transform.localEulerAngles;
        if (eularAngle.z > 350)
            boom.transform.localRotation = Quaternion.Euler(eularAngle.x, eularAngle.y, 350);
        if (eularAngle.z < 260)
            boom.transform.localRotation = Quaternion.Euler(eularAngle.x, eularAngle.y, 260);
    }


    private void UpdateCamera()
    {
        if (Input.GetKeyUp(KeyCode.C))
        {
            cameraFPS.enabled = !cameraFPS.enabled;
            cameraFPS.GetComponent<AudioListener>().enabled = !cameraFPS.enabled;
            cameraTPS.enabled = !cameraFPS.enabled;
            cameraTPS.GetComponent<AudioListener>().enabled = !cameraFPS.enabled;
        }
    }


    private void UpdateEngineSoundEffect()
    {
        if (leftStick1 == StickState.IDLE && rightStick1 == StickState.IDLE)
            audioSource.Stop();
        else if (audioSource.isPlaying == false)
            audioSource.Play();
            
    }





    private void UpdateMovementTemp()
    {
        float speed = 15f * Input.GetAxis("Vertical") * Time.deltaTime;

        transform.Translate(transform.right * speed, Space.World);

        float angularSpeed = 30f * Input.GetAxis("Horizontal") * Time.deltaTime;

        transform.Rotate(transform.up, angularSpeed, Space.World);
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
