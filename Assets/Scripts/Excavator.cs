using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundEffect;

public class Excavator : DestroyableSingleton<Excavator>
{
    [Header("Speed")]
    public float speedRate = 15f;
    public float speedDamp = 0.10f;

    [Header("Mechanical Arm")]
    public float angualrSpeedRate = 15f;
    public float boomUpperBound = 0.0f;
    public float boomLowerBound = 0.0f;
    public float boomBreakTime = 3.0f;
    public float armUpperBound = 0.0f;
    public float armLowerBound = 0.0f;
    public float armBreakTime = 3.0f;
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

    private int engineRPM = 0;
    private bool isBucketRotating = false;
    private bool isBoomReachLimit = false;
    private bool isArmReachLimit = false;
    private float boomLimitStartTime = 0.0f;
    private float armLimitStartTime = 0.0f;
    private AudioSource engineIdleSource;
    private AudioSource engineAccelSource;
    private GameplayManager gameManager;
    private HardwareManager hardwareManager;
    private SoundEffectManager soundManager;
    public int EngineRPM => engineRPM;
    public Action<string> hornAction;


    public enum EngineState { ON = 1, IGNITE = 0, OFF = -1}
    private EngineState engineState = EngineState.OFF;
    public EngineState ExcavatorEngineState { get { return engineState; } }

    public enum GearState { THIRD = 3, SECOND = 2, FIRST = 1, NEUTRAL = 0, REVERSE = -1}
    private GearState gearState = GearState.NEUTRAL;
    public GearState ExcavatorGearState { get { return gearState; } }

    public enum DamageState { FIXED = 1, BROKEN = 0 }
    private DamageState boomState = DamageState.FIXED;
    private DamageState armState = DamageState.FIXED;





    protected override void Start()
    {
        base.Start();

        gameManager = GameplayManager.Instance;
        hardwareManager = HardwareManager.Instance;
        soundManager = SoundEffectManager.Instance;
        hardwareManager.OnStick2ChangeAction += IgniteListener;

        InitExcavator();        
    }


    protected override void Update()
    {
        base.Update();

        
        if (gameManager.gameState == GameplayManager.GameState.STARTED)
        {
            UpdateGearState();
            UpdateEngineRMP();

            UpdateCamera();
            UpdateExcavatorMovement();
            UpdateExcavatorRotation();
            UpdateCabRotation();
            UpdateBoomRotation();
            UpdateArmRotation();
            UpdateDamageState();

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
        if (gameManager.manualIgnite)
        {
            engineState = EngineState.ON;
        }
    }


    private void UpdateExcavatorMovement()
    {
        if (engineState == EngineState.ON)
        {
            float leftSpeed, rightSpeed, speed;
            GameplayManager.JoySitckConfig leftStick = gameManager.sticks[0];
            GameplayManager.JoySitckConfig rightStick = gameManager.sticks[3];

            /* First control plan: two joystick control two vehicle tracks respectively */
            if (leftStick.stickState == GameplayManager.StickState.DECELERATE)
                leftSpeed = 0.0f;
            else if (leftStick.stickState == GameplayManager.StickState.BACKWARD || 
                     leftStick.stickState == GameplayManager.StickState.IDLE)
                leftSpeed = speedRate * 1;
            else
                leftSpeed = speedRate * 2;

            if (rightStick.stickState == GameplayManager.StickState.DECELERATE)
                rightSpeed = 0.0f;
            else if (rightStick.stickState == GameplayManager.StickState.BACKWARD || 
                     rightStick.stickState == GameplayManager.StickState.IDLE)
                rightSpeed = speedRate * 1;
            else
                rightSpeed = speedRate * 2;

            speed = Mathf.Min(leftSpeed, rightSpeed);

            ///* Second control plan: one joystick control forward movement, another control rotation */
            //switch (leftStick.stickState)
            //{
            //    case GameplayManager.StickState.DECELERATE:
            //        speed = 0.0f;
            //        break;
            //    case GameplayManager.StickState.BACKWARD:
            //    case GameplayManager.StickState.IDLE:
            //        speed = speedRate * 1;
            //        break;
            //    case GameplayManager.StickState.FORWARD:
            //    case GameplayManager.StickState.ACCELERATE:
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
            if ((leftStick.stickState == GameplayManager.StickState.DECELERATE && 
                 rightStick.stickState == GameplayManager.StickState.DECELERATE) ||
                gearState == GearState.NEUTRAL)
            {
                if (engineAccelSource != null)
                {
                    soundManager.Stop(engineAccelSource);
                    engineAccelSource = null;
                }
            }
            else
            {
                if (engineAccelSource == null)
                    engineAccelSource = soundManager.FindAvailableLoopAudioSource();

                if (engineAccelSource != null && 
                    engineAccelSource.isPlaying == false)
                    soundManager.PlayInLoop(engineAccelSource, "Engine_Accelerating");
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
            GameplayManager.JoySitckConfig leftStick = gameManager.sticks[0];
            GameplayManager.JoySitckConfig rightStick = gameManager.sticks[3];

            /* First control plan: two joystick control two vehicle tracks respectively */
            angularSpeed = ((int)leftStick.stickState - (int)rightStick.stickState) * (int)gearState * angualrSpeedRate * Time.deltaTime;

            ///* Second control plan: one joystick control forward movement, another control rotation */
            //angularSpeed = -1 * (int)rightStick.stickState * (int)gearState * angualrSpeedRate * Time.deltaTime;

            transform.Rotate(axis, angularSpeed, Space.World);

            ///* Play sound effect */
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
            GameplayManager.JoySitckConfig stick = gameManager.sticks[1];
            float angularSpeed = (int)stick.stickState * angualrSpeedRate * Time.deltaTime;
            Vector3 axis = cab.transform.up; 
            cab.transform.Rotate(axis, angularSpeed, Space.World);
        }
    }


    private void UpdateBoomRotation()
    {
        float angularSpeed = 0.0f;
        if (engineState == EngineState.ON && boomState == DamageState.FIXED)
        {
            GameplayManager.JoySitckConfig stick = gameManager.sticks[2];
            angularSpeed = (int)stick.stickState * angualrSpeedRate * Time.deltaTime;
            Vector3 axis = boom.transform.forward;
            boom.transform.Rotate(axis, angularSpeed, Space.World);
        }

        /* Limit boom rotation within specific angle, 20 > x > 335 (across 0 degree) */
        Vector3 eulerAngle = boom.transform.localEulerAngles;
        if (eulerAngle.z > boomUpperBound && eulerAngle.z < 180)
            boom.transform.localRotation = Quaternion.Euler(eulerAngle.x, eulerAngle.y, boomUpperBound);
        if (eulerAngle.z < boomLowerBound && eulerAngle.z > 180)
            boom.transform.localRotation = Quaternion.Euler(eulerAngle.x, eulerAngle.y, boomLowerBound);

        /* Detect if boom has reached limit position but player is still keep adding force
         * If so, the boom will be broken after "boomBreakTime" seconds */
        if ((angularSpeed > 0 && boom.transform.localEulerAngles.z >= boomUpperBound) ||
            (angularSpeed < 0 && boom.transform.localEulerAngles.z <= boomLowerBound))
        {
            if (isBoomReachLimit == false)
            {
                isBoomReachLimit = true;
                boomLimitStartTime = Time.fixedTime;
            }
            else if (Time.fixedTime - boomLimitStartTime >= boomBreakTime)
            {
                Debug.LogWarning("Boom is borken!!");
                isBoomReachLimit = false;
                boomState = DamageState.BROKEN;
            }
        }
        else
            isBoomReachLimit = false;

    }


    private void UpdateArmRotation()
    {
        float angularSpeed = 0.0f;
        if (engineState == EngineState.ON && armState == DamageState.FIXED)
        {
            GameplayManager.JoySitckConfig stick = gameManager.sticks[4];
            angularSpeed = (int)stick.stickState * angualrSpeedRate * Time.deltaTime;
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

        /* Detect if arm has reached limit position but player is still keep adding force
         * If so, the arm will be broken after "armBreakTime" seconds */
        if ((angularSpeed > 0 && arm.transform.localEulerAngles.z >= armUpperBound) ||
            (angularSpeed < 0 && arm.transform.localEulerAngles.z <= armLowerBound))
        {
            if (isArmReachLimit == false)
            {
                isArmReachLimit = true;
                armLimitStartTime = Time.fixedTime;
            }
            else if (Time.fixedTime - armLimitStartTime >= armBreakTime)
            {
                Debug.LogWarning("Arm is borken!!");
                isArmReachLimit = false;
                armState = DamageState.BROKEN;
            }
        }
        else
            isArmReachLimit = false;
    }


    private void UpdateGearState()
    {
        if (engineState == EngineState.ON)
        {
            GearState currentState;
            GameplayManager.JoySitckConfig stick = gameManager.sticks[5];
            switch (stick.stickState)
            {
                case GameplayManager.StickState.ACCELERATE:
                    currentState = GearState.SECOND;
                    break;
                case GameplayManager.StickState.FORWARD:
                    currentState = GearState.FIRST;
                    break;
                case GameplayManager.StickState.IDLE:
                    currentState = GearState.NEUTRAL;
                    break;
                case GameplayManager.StickState.BACKWARD:
                case GameplayManager.StickState.DECELERATE:
                    currentState = GearState.REVERSE;
                    break;
                default:
                    currentState = GearState.NEUTRAL;
                    break;
            }

            if (currentState != gearState && gameManager.useCluthBtn == true && !Input.GetKey(KeyCode.Joystick1Button2))
            {
                engineState = EngineState.OFF;
                gearState = GearState.NEUTRAL;

                AudioSource flameoutSource = soundManager.FindAvailableSingleAudioSource();
                soundManager.PlayOneShot(flameoutSource, "Engine_Flameout");
                if (engineIdleSource != null)
                    soundManager.Stop(engineIdleSource);
                if (engineAccelSource != null)
                {
                    soundManager.Stop(engineAccelSource);
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


    private void UpdateDamageState()
    {
        if (gameManager.useQuickFix)
        {
            if (boomState == DamageState.BROKEN && Input.GetKeyUp(KeyCode.A))
                boomState = DamageState.FIXED;

            if (armState == DamageState.BROKEN && Input.GetKeyUp(KeyCode.S))
                armState = DamageState.FIXED;
        }
    }

    public void UpdateEngineRMP()
    {
        engineRPM -= Mathf.FloorToInt(RPMDecay * Time.deltaTime);

        if (engineState == EngineState.ON)
        {
            float stickValue = Input.GetAxis(gameManager.sticks[0].stickName) + 1.0f;
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
        if (engineState == EngineState.OFF && gameManager.gameState == GameplayManager.GameState.STARTED)
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
        if (Input.GetKeyUp(KeyCode.Joystick1Button0) ||  Input.GetKeyUp(KeyCode.V))
        {
            soundManager.PlayOneShot(soundManager.singleAudioSourceList[0], "Horn_Farting");
            hornAction.Invoke("horn");
        }
        else if (Input.GetKeyUp(KeyCode.Joystick1Button1) || Input.GetKeyUp(KeyCode.Joystick1Button3) || 
                 Input.GetKeyUp(KeyCode.B))
        {
            soundManager.PlayOneShot(soundManager.singleAudioSourceList[0], "Horn_Truck");
            hornAction.Invoke("horn");
        }
        else if (Input.GetKeyUp(KeyCode.Joystick1Button4) || Input.GetKeyUp(KeyCode.Joystick1Button5))
        {
            soundManager.PlayOneShot(soundManager.singleAudioSourceList[0], "Horn_Song");
            hornAction.Invoke("horn");
        }
    }


    private IEnumerator IgniteCoroutine()
    {
        float time = 0f;
        float initialCount = hardwareManager.Joystick2;
        
        AudioSource igniteSource = soundManager.FindAvailableLoopAudioSource();
        soundManager.PlayInLoop(igniteSource, "Engine_Igniting");

        engineState = EngineState.IGNITE;

        while (time <= igniteInterval)
        {
            //Debug.Log($"Igniter init: {initialCount}, curr: {hardwareManager.Joystick2}, time: {time}");


            if (Math.Abs(hardwareManager.Joystick2 - initialCount) >= igniteThreshold)
            {
                engineState = EngineState.ON;
                
                /* Play sound effect */
                AudioSource successSource = soundManager.FindAvailableSingleAudioSource();
                soundManager.Stop(igniteSource);
                soundManager.PlayOneShot(successSource, "Engine_Ignite_Success");

                yield return new WaitForSecondsRealtime(2f);
                engineIdleSource = soundManager.FindAvailableLoopAudioSource();
                soundManager.PlayInLoop(engineIdleSource, "Engine_Idling");

                Debug.Log("Engine ON");
                yield break;
            }

            time += Time.deltaTime;
            yield return null;
        }

        soundManager.Stop(igniteSource);
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



   


    // Show some data 
    void OnGUI()
    {
        List<GameplayManager.JoySitckConfig> sticks = gameManager.sticks;
        GUI.TextArea(new Rect(0, 50, 250, 40), $"Left stick : {sticks[0].stickState} : {sticks[1].stickState} : {sticks[2].stickState}");
        GUI.TextArea(new Rect(0, 100, 250, 40), $"Right stick : {sticks[3].stickState} : {sticks[4].stickState} : {sticks[5].stickState}");
        GUI.TextArea(new Rect(0, 150, 250, 40), $"Engine State : {engineState}");
        GUI.TextArea(new Rect(0, 200, 250, 40), $"Gear State : {gearState}");
        GUI.TextArea(new Rect(0, 250, 250, 40), $"Engine RPM : {engineRPM}");
        //GUI.TextArea(new Rect(0, 240, 250, 40), $"JoyStick Button : {Input.GetKey(KeyCode.Joystick1Button2)}");
    }



}
