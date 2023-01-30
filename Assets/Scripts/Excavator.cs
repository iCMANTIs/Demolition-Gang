using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Excavator : DestroyableSingleton<Excavator>
{
    [Header("Speed")]
    public float speedDamp = 0.10f;

    [Header("Mechanical Arm")]
    public float boomUpperBound = 0.0f;
    public float boomLowerBound = 0.0f;
    public float armUpperBound = 0.0f;
    public float armLowerBound = 0.0f;
    public float bucketAngularSpeed = 0.0f;
    public float bucketUpperBound = 0.0f;
    public float buckerLowerBound = 0.0f;

    [Header("Engine")]
    public float igniteInterval = 0.0f;
    public int igniteThreshold = 0;
    public int RPMSpeed = 0;
    public int RPMDecay = 0;
    public int RPMUpperBound = 0;
    public int RPMLowerBound = 0;


    [Header("GameObject")]
    public Camera cameraTPS;
    public Camera cameraFPS;
    public GameObject cab;
    public GameObject boom;
    public GameObject arm;
    public GameObject bucket;


    private bool isBucketRotating = false;
    private int engineRPM = 0;
    public int EngineRPM => engineRPM;
    public Action hornAction;


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

    enum EngineState { ON = 1, IGNITE = 0, OFF = -1}
    EngineState engineState = EngineState.OFF;

    enum GearState { THIRD = 3, SECOND = 2, FIRST = 1, NEUTRAL = 0, REVERSE = -1}
    GearState gearState = GearState.NEUTRAL;



    protected override void Awake()
    {
        base.Awake();
    }


    // Update is called once per frame 
    protected override void Update()
    {
        base.Update();

        UpdateJoyStickState(ref leftStick1, stickNames[0]);
        UpdateJoyStickState(ref leftStick2, stickNames[1]);
        UpdateJoyStickState(ref leftStick3, stickNames[2]);
        UpdateJoyStickState(ref rightStick1, stickNames[3]);
        UpdateJoyStickState(ref rightStick2, stickNames[4]);
        UpdateJoyStickState(ref rightStick3, stickNames[5]);
        
        
        if (GameplayController.Instance.gameState != GameplayController.GameState.PAUSED)
        {
            UpdateGearState();
            UpdateEngineRMP();

            UpdateCamera();
            UpdateExcavatorMovement();
            UpdateExcavatorRotation();
            UpdateCabRotation();
            UpdateBoomRotation();
            UpdateArmRotation();

            IgniteListener();
            BucketListener();
            HornListener();
        }


        //UpdateMovementTemp();
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
        if (engineState == EngineState.ON)
        {
            float speed;
            switch (leftStick1)
            {
                case StickState.DECELERATE:
                    speed = 0.0f;
                    break;
                case StickState.BACKWARD:
                case StickState.IDLE:
                    speed = 15.0f;
                    break;
                case StickState.FORWARD:
                case StickState.ACCELERATE:
                    speed = 30.0f;
                    break;
                default:
                    speed = 0.0f;
                    break;
            }

            Vector3 direction = transform.right;
            speed = speed * (int)gearState * speedDamp * Time.deltaTime;
            transform.Translate(direction * speed, Space.World);
        }
    }


    private void UpdateExcavatorRotation()
    {
        if (engineState == EngineState.ON)
        {
            Vector3 axis = transform.up;
            float angularSpeed = -1 * (int)rightStick1 * Time.deltaTime;
            transform.Rotate(axis, angularSpeed, Space.World);
        }
    }


    private void UpdateCabRotation()
    {
        if (engineState == EngineState.ON)
        {
            float angularSpeed = (int)leftStick2 * Time.deltaTime;
            Vector3 axis = cab.transform.up; 
            cab.transform.Rotate(axis, angularSpeed, Space.World);
        }
    }


    private void UpdateBoomRotation()
    {
        if (engineState == EngineState.ON)
        {
            float angularSpeed = (int)leftStick3 * Time.deltaTime;
            Vector3 axis = boom.transform.forward;
            boom.transform.Rotate(axis, angularSpeed, Space.World);
        }

        /* Limit boom rotation within specific angle */
        Vector3 eularAngle = boom.transform.localEulerAngles;
        if (eularAngle.z > boomUpperBound)
            boom.transform.localRotation = Quaternion.Euler(eularAngle.x, eularAngle.y, boomUpperBound);
        if (eularAngle.z < boomLowerBound)
            boom.transform.localRotation = Quaternion.Euler(eularAngle.x, eularAngle.y, boomLowerBound);
    }


    private void UpdateArmRotation()
    {
        if (engineState == EngineState.ON)
        {
            float angularSpeed = (int)rightStick2 * Time.deltaTime;
            Vector3 axis = arm.transform.forward;
            arm.transform.Rotate(axis, angularSpeed, Space.World);
        }

        //Debug.Log($"arm LocalEulaerAngle: {arm.transform.localEulerAngles}, EulerAngle: {arm.transform.eulerAngles}");

        /* Limit arm rotation within specific angle */
        Vector3 eulerAngle = arm.transform.localEulerAngles;
        if (eulerAngle.z > armUpperBound && eulerAngle.z < 180)
            arm.transform.localRotation = Quaternion.Euler(eulerAngle.x, eulerAngle.y, armUpperBound);
        if (eulerAngle.z < armLowerBound && eulerAngle.z > 180)
            arm.transform.localRotation = Quaternion.Euler(eulerAngle.x, eulerAngle.y, armLowerBound);
    }


    private void UpdateGearState()
    {
        GearState currentState;
        switch (rightStick3)
        {
            case StickState.ACCELERATE:
                currentState = GearState.SECOND;
                break;
            case StickState.FORWARD:
                currentState = GearState.FIRST;
                break;
            case StickState.IDLE:
                currentState = GearState.NEUTRAL;
                break;
            case StickState.BACKWARD:
            case StickState.DECELERATE:
                currentState = GearState.REVERSE;
                break;
            default:
                currentState = GearState.NEUTRAL;
                break;
        }

        if (currentState != gearState && !Input.GetKey(KeyCode.Joystick1Button2))
        {
            engineState = EngineState.OFF;
            gearState = GearState.NEUTRAL;
        }
        else
        {
            gearState = currentState;
        }
    }


    public void UpdateEngineRMP()
    {
        engineRPM -= Mathf.FloorToInt(RPMDecay * Time.deltaTime);

        if (engineState == EngineState.ON)
        {
            float stickValue = Input.GetAxis(stickNames[0]) + 1.0f;
            engineRPM += Mathf.FloorToInt(stickValue * RPMSpeed * Time.deltaTime);
        }

        engineRPM = Math.Clamp(engineRPM, RPMLowerBound, RPMUpperBound);
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


    private void IgniteListener()
    {
        if ((Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Joystick2Button0) || Input.GetKeyDown(KeyCode.Z)) 
            && engineState == EngineState.OFF)
        {
            StartCoroutine(IgniteCoroutine());
        }
    }


    private void BucketListener()
    {
        //Debug.Log($"{bucket.transform.localEulerAngles}");

        if ((Input.GetKeyDown(KeyCode.Joystick2Button2) || Input.GetKeyDown(KeyCode.X)) 
            && isBucketRotating == false)
        {
            StartCoroutine(BucketCoroutine());
        }
    }


    private void HornListener()
    {
        if (Input.GetKeyUp(KeyCode.Joystick1Button1) || Input.GetKeyUp(KeyCode.Joystick1Button3) ||
            Input.GetKeyUp(KeyCode.Joystick1Button5) || Input.GetKeyUp(KeyCode.Joystick2Button1) ||
            Input.GetKeyUp(KeyCode.Joystick2Button3) || Input.GetKeyUp(KeyCode.Joystick2Button5) ||
            Input.GetKeyUp(KeyCode.V))
        {
            SoundEffect.SoundEffectManager manager = SoundEffect.SoundEffectManager.Instance;
            manager.PlayOneShot(manager.singleAudioSourceList[0], "Horn");

            hornAction.Invoke();
        }
    }


    private IEnumerator IgniteCoroutine()
    {
        float count = 0f;
        float time = 0f;
        engineState = EngineState.IGNITE;

        while (time <= 10f)
        {
            if (count == igniteThreshold)
            {
                engineState = EngineState.ON;
                Debug.Log("Engine ON");
                yield break;
            }

            if (Input.GetKeyUp(KeyCode.Joystick1Button0))
                count++;
            else if (Input.GetKeyUp(KeyCode.Joystick2Button0))
                count++;
            else if (Input.GetKeyUp(KeyCode.Z))
                count++;

            time += Time.deltaTime;
            yield return null;
        }

        engineState = EngineState.OFF;
        yield break;
    }


    private IEnumerator BucketCoroutine()
    {
        isBucketRotating = true;
        Vector3 eulerAngle = bucket.transform.localEulerAngles;
        Vector3 axis = bucket.transform.forward;

        /* Rotate forward */
        while ((eulerAngle.z <= 360 && eulerAngle.z > 180) ||
               (eulerAngle.z <= bucketUpperBound && eulerAngle.z < 180))
        {
            eulerAngle = bucket.transform.localEulerAngles;
            bucket.transform.Rotate(axis, bucketAngularSpeed * Time.deltaTime, Space.World);
            yield return null;
        }

        /* Rotate backward */
        while ((eulerAngle.z >= 0 && eulerAngle.z < 180) ||
               (eulerAngle.z >= buckerLowerBound && eulerAngle.z > 180))
        {
            eulerAngle = bucket.transform.localEulerAngles;
            bucket.transform.Rotate(axis, -1 * bucketAngularSpeed * Time.deltaTime, Space.World);
            yield return null;
        }

        isBucketRotating = false;
        yield break;
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
        GUI.TextArea(new Rect(0, 40, 250, 40), $"Left stick : {leftStick1} : {leftStick2} : {leftStick3}");
        GUI.TextArea(new Rect(0, 80, 250, 40), $"Right stick : {rightStick1} : {rightStick2} : {rightStick3}");
        GUI.TextArea(new Rect(0, 120, 250, 40), $"Engine State : {engineState}");
        GUI.TextArea(new Rect(0, 160, 250, 40), $"Gear State : {gearState}");
        GUI.TextArea(new Rect(0, 200, 250, 40), $"Engine RPM : {engineRPM}");
        //GUI.TextArea(new Rect(0, 240, 250, 40), $"JoyStick Button : {Input.GetKey(KeyCode.Joystick1Button2)}");
    }



}
