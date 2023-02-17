using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundEffect;

public class Excavator : DestroyableSingleton<Excavator>
{
    [Header("Speed")]
    public int speedRate = 15;
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
    public Animator rightTrackAnimator;
    public Animator leftTrackAnimator;

    [Header("Debug")]
    public bool isDebugMode = false;
    public bool useKeyBoard = false;

    private bool isBucketRotating = false;
    private int engineRPM = 0;
    private AudioSource engineIdleSource;
    private AudioSource engineAccelSource;
    public int EngineRPM => engineRPM;
    public Action<string> hornAction;




    private string[] stickNames = 
    {
        "LeftJoyStickS1",           // left first stick, control the left track
        "LeftJoyStickS2",           // left second  stick, control the cab rotation
        "LeftJoyStickS3",           // left third stick, control the boom rotation
        "RightJoyStickS1",          // right first stick, control the right track
        "RightJoyStickS2",          // right second stick, control the arm rotation
        "RightJoyStickS3",          // right third stick, control the gear shifting
    };

    enum StickState { ACCELERATE = 30, FORWARD = 15, IDLE = 0, BACKWARD = -15, DECELERATE = -30 }
    StickState leftStick1 = StickState.IDLE;
    StickState leftStick2 = StickState.IDLE;
    StickState leftStick3 = StickState.IDLE;
    StickState rightStick1 = StickState.IDLE;
    StickState rightStick2 = StickState.IDLE;
    StickState rightStick3 = StickState.IDLE;

    public enum EngineState { ON = 1, IGNITE = 0, OFF = -1}
    private EngineState engineState = EngineState.OFF;
    public EngineState ExcavatorEngineState { get { return engineState; } }

    public enum GearState { THIRD = 3, SECOND = 2, FIRST = 1, NEUTRAL = 0, REVERSE = -1}
    private GearState gearState = GearState.NEUTRAL;
    public GearState ExcavatorGearState { get { return gearState; } }




    protected override void Start()
    {
        base.Start();

        HardwareManager.Instance.OnStick2ChangeAction += IgniteListener;

        InitExcavator();        
    }


    protected override void Update()
    {
        base.Update();

        if (!useKeyBoard)
        {
            UpdateJoyStickState(ref leftStick1, stickNames[0]);
            UpdateJoyStickState(ref leftStick2, stickNames[1]);
            UpdateJoyStickState(ref leftStick3, stickNames[2]);
            UpdateJoyStickState(ref rightStick1, stickNames[3]);
            UpdateJoyStickState(ref rightStick2, stickNames[4]);
            UpdateJoyStickState(ref rightStick3, stickNames[5]);
        }
        else
            UpdateJoyStickState();
        
        if (GameplayManager.Instance.gameState == GameplayManager.GameState.STARTED)
        {
            UpdateGearState();
            UpdateEngineRMP();

            UpdateCamera();
            UpdateExcavatorMovement();
            UpdateExcavatorRotation();
            UpdateCabRotation();
            UpdateBoomRotation();
            UpdateArmRotation();

            BucketListener();
            HornListener();
        }


        /* Debug */
        //UpdateMovementTemp();
        //Debug.Log($"Boom angle: {boom.transform.localEulerAngles}");
        //Debug.Log($"Arm angle: {arm.transform.localEulerAngles}");
        //Debug.Log($"Bucket angle: {bucket.transform.localEulerAngles}");
    }


    private void InitExcavator()
    {
        if (isDebugMode)
        {
            engineState = EngineState.ON;
        }
    }


    private void UpdateJoyStickState(ref StickState stick, string stickName)
    {
        float value;

        /* Right joystick 3 use the input from external hardware */
        if (stickName == stickNames[5])
            value = HardwareManager.Instance.Joystick1;
        else
            value = Input.GetAxis(stickName);

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


    private void UpdateJoyStickState()
    {
        if(Input.GetKeyUp(KeyCode.Q))
            leftStick1 = leftStick1 == StickState.ACCELERATE ? StickState.ACCELERATE : leftStick1 + speedRate;
        if (Input.GetKeyUp(KeyCode.W))
            leftStick1 = leftStick1 == StickState.DECELERATE ? StickState.DECELERATE : leftStick1 - speedRate;

        if (Input.GetKeyUp(KeyCode.E))
            leftStick2 = leftStick2 == StickState.ACCELERATE ? StickState.ACCELERATE : leftStick2 + speedRate;
        if (Input.GetKeyUp(KeyCode.R))
            leftStick2 = leftStick2 == StickState.DECELERATE ? StickState.DECELERATE : leftStick2 - speedRate;

        if (Input.GetKeyUp(KeyCode.T))
            leftStick3 = leftStick3 == StickState.ACCELERATE ? StickState.ACCELERATE : leftStick3 + speedRate;
        if (Input.GetKeyUp(KeyCode.Y))
            leftStick3 = leftStick3 == StickState.DECELERATE ? StickState.DECELERATE : leftStick3 - speedRate;

        if (Input.GetKeyUp(KeyCode.A))
            rightStick1 = rightStick1 == StickState.ACCELERATE ? StickState.ACCELERATE : rightStick1 + speedRate;
        if (Input.GetKeyUp(KeyCode.S))
            rightStick1 = rightStick1 == StickState.DECELERATE ? StickState.DECELERATE : rightStick1 - speedRate;

        if (Input.GetKeyUp(KeyCode.D))
            rightStick2 = rightStick2 == StickState.ACCELERATE ? StickState.ACCELERATE : rightStick2 + speedRate;
        if (Input.GetKeyUp(KeyCode.F))
            rightStick2 = rightStick2 == StickState.DECELERATE ? StickState.DECELERATE : rightStick2 - speedRate;

        if (Input.GetKeyUp(KeyCode.G))
            rightStick3 = rightStick3 == StickState.ACCELERATE ? StickState.ACCELERATE : rightStick3 + speedRate;
        if (Input.GetKeyUp(KeyCode.H))
            rightStick3 = rightStick3 == StickState.DECELERATE ? StickState.DECELERATE : rightStick3 - speedRate;
    }


    private void UpdateExcavatorMovement()
    {
        if (engineState == EngineState.ON)
        {
            float leftSpeed, rightSpeed, speed;

            /* First control plan: two joystick control two vehicle tracks respectively */
            if (leftStick1 == StickState.DECELERATE)
                leftSpeed = 0.0f;
            else if (leftStick1 == StickState.BACKWARD || leftStick1 == StickState.IDLE)
                leftSpeed = speedRate * 1;
            else
                leftSpeed = speedRate * 2;

            if (rightStick1 == StickState.DECELERATE)
                rightSpeed = 0.0f;
            else if (rightStick1 == StickState.BACKWARD || rightStick1 == StickState.IDLE)
                rightSpeed = speedRate * 1;
            else
                rightSpeed = speedRate * 2;

            speed = Mathf.Min(leftSpeed, rightSpeed);

            ///* Second control plan: one joystick control forward movement, another control rotation */
            //switch (leftStick1)
            //{
            //    case StickState.DECELERATE:
            //        speed = 0.0f;
            //        break;
            //    case StickState.BACKWARD:
            //    case StickState.IDLE:
            //        speed = speedRate * 1;
            //        break;
            //    case StickState.FORWARD:
            //    case StickState.ACCELERATE:
            //        speed = speedRate * 2;
            //        break;
            //    default:
            //        speed = 0.0f;
            //        break;
            //}

            Vector3 direction = transform.right;
            speed = speed * (int)gearState * speedDamp * Time.deltaTime;
            transform.Translate(direction * speed, Space.World);


            /* Play sound effect */
            SoundEffectManager manager = SoundEffectManager.Instance;
            if ((leftStick1 == StickState.DECELERATE && rightStick1 == StickState.DECELERATE) ||
                gearState == GearState.NEUTRAL)
            {
                if (engineAccelSource != null)
                {
                    manager.Stop(engineAccelSource);
                    engineAccelSource = null;
                }
            }
            else
            {
                if (engineAccelSource == null)
                    engineAccelSource = manager.FindAvailableLoopAudioSource();

                if (engineAccelSource != null && 
                    engineAccelSource.isPlaying == false)
                    manager.PlayInLoop(engineAccelSource, "Engine_Accelerating");
            }

            /* Play model animation */
            rightTrackAnimator.SetFloat("Speed", speed);
            leftTrackAnimator.SetFloat("Speed", speed);
        }
    }


    private void UpdateExcavatorRotation()
    {
        if (engineState == EngineState.ON)
        {
            float angularSpeed;
            Vector3 axis = transform.up;

            /* First control plan: two joystick control two vehicle tracks respectively */
            angularSpeed = ((int)leftStick1 - (int)rightStick1) * (int)gearState * Time.deltaTime;

            ///* Second control plan: one joystick control forward movement, another control rotation */
            //angularSpeed = -1 * (int)rightStick1 * (int)gearState * Time.deltaTime;

            transform.Rotate(axis, angularSpeed, Space.World);

            ///* Play sound effect */
            //SoundEffectManager manager = SoundEffectManager.Instance;
            //if ((leftStick1 == StickState.DECELERATE && rightStick1 == StickState.IDLE) ||
            //    gearState == GearState.NEUTRAL)
            //{
            //    if (engineAccelSource != null)
            //    {
            //        manager.Stop(engineAccelSource);
            //        engineAccelSource = null;
            //    }
            //}
            //else
            //{
            //    if (engineAccelSource == null)
            //        engineAccelSource = manager.FindAvailableLoopAudioSource();

            //    if (engineAccelSource != null &&
            //        engineAccelSource.isPlaying == false)
            //        manager.PlayInLoop(engineAccelSource, "Engine_Accelerating");
            //}
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

        /* Limit boom rotation within specific angle, 20 > x > 335 (across 0 degree) */
        Vector3 eulerAngle = boom.transform.localEulerAngles;
        if (eulerAngle.z > boomUpperBound && eulerAngle.z < 180)
            boom.transform.localRotation = Quaternion.Euler(eulerAngle.x, eulerAngle.y, boomUpperBound);
        if (eulerAngle.z < boomLowerBound && eulerAngle.z > 180)
            boom.transform.localRotation = Quaternion.Euler(eulerAngle.x, eulerAngle.y, boomLowerBound);
        //if (eularAngle.z > boomUpperBound)
        //    boom.transform.localRotation = Quaternion.Euler(eularAngle.x, eularAngle.y, boomUpperBound);
        //if (eularAngle.z < boomLowerBound)
        //    boom.transform.localRotation = Quaternion.Euler(eularAngle.x, eularAngle.y, boomLowerBound);
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
        if (engineState == EngineState.ON)
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

            if (currentState != gearState && !Input.GetKey(KeyCode.Joystick1Button2) && !isDebugMode)
            {
                engineState = EngineState.OFF;
                gearState = GearState.NEUTRAL;

                SoundEffectManager manager = SoundEffectManager.Instance;
                AudioSource flameoutSource = manager.FindAvailableSingleAudioSource();
                manager.PlayOneShot(flameoutSource, "Engine_Flameout");
                if (engineIdleSource != null)
                    manager.Stop(engineIdleSource);
                if (engineAccelSource != null)
                {
                    manager.Stop(engineAccelSource);
                    engineAccelSource = null;
                }

                JoystickStatePanel.Instance.Show();
            }
            else
            {
                gearState = currentState;
            }
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
        if (engineState == EngineState.OFF && GameplayManager.Instance.gameState == GameplayManager.GameState.STARTED)
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
        SoundEffectManager manager = SoundEffectManager.Instance;
        if (Input.GetKeyUp(KeyCode.Joystick1Button0) ||  Input.GetKeyUp(KeyCode.V))
        {
            manager.PlayOneShot(manager.singleAudioSourceList[0], "Horn_Farting");
            hornAction.Invoke("horn");
        }
        else if (Input.GetKeyUp(KeyCode.Joystick1Button1) || Input.GetKeyUp(KeyCode.Joystick1Button3) || 
                 Input.GetKeyUp(KeyCode.B))
        {
            manager.PlayOneShot(manager.singleAudioSourceList[0], "Horn_Truck");
            hornAction.Invoke("horn");
        }
        else if (Input.GetKeyUp(KeyCode.Joystick1Button4) || Input.GetKeyUp(KeyCode.Joystick1Button5))
        {
            manager.PlayOneShot(manager.singleAudioSourceList[0], "Horn_Song");
            hornAction.Invoke("horn");
        }
    }


    private IEnumerator IgniteCoroutine()
    {
        float time = 0f;
        float initialCount = HardwareManager.Instance.Joystick2;
        
        SoundEffectManager manager = SoundEffectManager.Instance;
        AudioSource igniteSource = manager.FindAvailableLoopAudioSource();
        manager.PlayInLoop(igniteSource, "Engine_Igniting");

        engineState = EngineState.IGNITE;

        while (time <= igniteInterval)
        {
            //Debug.Log($"Igniter init: {initialCount}, curr: {HardwareManager.Instance.Joystick2}, time: {time}");


            if (Math.Abs(HardwareManager.Instance.Joystick2 - initialCount) >= igniteThreshold)
            {
                engineState = EngineState.ON;
                
                AudioSource successSource = manager.FindAvailableSingleAudioSource();
                manager.Stop(igniteSource);
                manager.PlayOneShot(successSource, "Engine_Ignite_Success");

                yield return new WaitForSecondsRealtime(2f);
                engineIdleSource = manager.FindAvailableLoopAudioSource();
                manager.PlayInLoop(engineIdleSource, "Engine_Idling");

                Debug.Log("Engine ON");
                yield break;
            }

            time += Time.deltaTime;
            yield return null;
        }

        manager.Stop(igniteSource);
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
        GUI.TextArea(new Rect(0, 50, 250, 40), $"Left stick : {leftStick1} : {leftStick2} : {leftStick3}");
        GUI.TextArea(new Rect(0, 100, 250, 40), $"Right stick : {rightStick1} : {rightStick2} : {rightStick3}");
        GUI.TextArea(new Rect(0, 150, 250, 40), $"Engine State : {engineState}");
        GUI.TextArea(new Rect(0, 200, 250, 40), $"Gear State : {gearState}");
        GUI.TextArea(new Rect(0, 250, 250, 40), $"Engine RPM : {engineRPM}");
        //GUI.TextArea(new Rect(0, 240, 250, 40), $"JoyStick Button : {Input.GetKey(KeyCode.Joystick1Button2)}");
    }



}
