using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : DestroyableSingleton<GameplayManager>
{
    [Serializable]
    public struct JoystickLevelConfig
    {
        public float lowerBound;
        public float upperBound;
    }

    [Serializable]
    public struct JoySitckConfig
    {
        public string stickName;
        public StickState stickState;
    }

    [Header("Joystick")]
    /* Left stick 1 (Q-W): control the left track; 
     * Left stick 2 (E-R): control the cab rotation;
     * Left stick 3 (T-Y): control the boom rotation; 
     * Right stick 1 (U-I): control the arm rotation;
     * Right stick 2 (O-P): control the gear shifting; 
     * Right stick 3 ([-]): control the right track */
    public List<JoySitckConfig> sticks = new List<JoySitckConfig>();
    public List<JoystickLevelConfig> stickLevels = new List<JoystickLevelConfig>();

    [Header("GameplaySetting")]
    public int frameRate = 60;
    public float totalGameTime = 0f;
    public float scoreFactor = 100f;
    public float socrePunishment = 500f;
    public float socreAward = 2000f;

    [Header("Debug")]
    /* Left btn 1 (A): Boom quick fix; 
     * Left btn 2 (S): Arm quick fix;
     * Right btn 2 (X): bucket; 
     * Right btn 3 (C): change camera
     * Right btn 4 (V): horn 
     * Right btn 5 (B): horn */
    public bool manualIgnite = false;
    public bool useKeyBoard = false;
    public bool useCluthBtn = false;
    public bool useQuickFix = false;
    public bool closeBreak = false;

    public enum StickState { ACCELERATE = 2, FORWARD = 1, IDLE = 0, BACKWARD = -1, DECELERATE = -2 }
    public enum GameState { UNSTARTED, PAUSED, STARTED }
    public GameState gameState = GameState.UNSTARTED;


    private float currentGameTime = 0f;
    private float timeScore = 0f;
    private float actionScore = 0f;
    public float CurrentGameTime { get { return currentGameTime; } }
    public int TimeScore { get { return Mathf.FloorToInt(timeScore); } }
    public int ActionScore { get { return Mathf.FloorToInt(actionScore); } }
    public int TotalScore { get { return Mathf.FloorToInt(timeScore + actionScore); } }

    
    
    protected override void Start()
    {
        base.Start();

        InitGame();
    }

    protected override void Update()
    {
        base.Update();

        UpdateJoyStickState();
        UpdateGameScore();


        /* Debug */
        if (Input.GetKey(KeyCode.Home))
            JoystickStatePanel.Instance.Show();
        if (Input.GetKey(KeyCode.Escape))
            ExitGame();





        //if (Input.GetKey(KeyCode.M))
        //    Debug.LogWarning("Get key is true!");
        //if (Input.GetKeyDown(KeyCode.M))
        //    Debug.LogWarning("Get key down is true!");
        //if (Input.GetKeyUp(KeyCode.M))
        //    Debug.LogWarning("Get key up is true!");


    }


    public void InitGame()
    {
        currentGameTime = 0f;
        Application.targetFrameRate = frameRate;
    }

    public void StartGame()
    {
        UpdateGameState(GameState.STARTED);
        StartCoroutine(TimeCountDownCoroutine());
    }

    public void PauseGame()
    {
        UpdateGameState(GameState.PAUSED);
    }

    public void UnpauseGame()
    {
        UpdateGameState(GameState.STARTED);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void UpdateGameState(GameState state)
    {
        gameState = state;
    }

    public void UpdateGameScore(float actionScore = 0)
    {
        this.actionScore += actionScore;

        if (AlertManager.Instance.alertState == AlertManager.AlertState.ALERT)
            scoreFactor = 0f;

        timeScore = scoreFactor * (totalGameTime - currentGameTime);
    }



    private void UpdateJoyStickState()
    {
        if (!useKeyBoard)
        {
            for (int i = 0; i < sticks.Count; i++)
            {
                JoySitckConfig config = sticks[i];
                float stickValue;

                /* Right joystick 3 use the input from external hardware */
                //if (config.stickName == "RightJoyStickS3")
                //    stickValue = HardwareManager.Instance.Joystick1;
                //else
                //    stickValue = Input.GetAxis(config.stickName);
                stickValue = Input.GetAxis(config.stickName);

                if (stickValue <= 1 && stickValue > 0.6)
                    config.stickState = StickState.ACCELERATE;
                else if (stickValue <= 0.6 && stickValue > 0.2)
                    config.stickState = StickState.FORWARD;
                else if (stickValue <= 0.2 && stickValue > -0.2)
                    config.stickState = StickState.IDLE;
                else if (stickValue <= -0.2 && stickValue > -0.6)
                    config.stickState = StickState.BACKWARD;
                else if (stickValue <= -0.6 && stickValue >= -1)
                    config.stickState = StickState.DECELERATE;

                sticks[i] = config;
            }
        }
        else
        {
            JoySitckConfig config = sticks[0];
            if (Input.GetKeyUp(KeyCode.Q) && config.stickState != StickState.ACCELERATE)
                config.stickState = config.stickState + 1;
            else if (Input.GetKeyUp(KeyCode.W) && config.stickState != StickState.DECELERATE)
                config.stickState = config.stickState - 1;
            sticks[0] = config;

            config = sticks[1];
            if (Input.GetKeyUp(KeyCode.E) && config.stickState != StickState.ACCELERATE)
                config.stickState = config.stickState + 1;
            else if (Input.GetKeyUp(KeyCode.R) && config.stickState != StickState.DECELERATE)
                config.stickState = config.stickState - 1;
            sticks[1] = config;

            config = sticks[2];
            if (Input.GetKeyUp(KeyCode.T) && config.stickState != StickState.ACCELERATE)
                config.stickState = config.stickState + 1;
            else if (Input.GetKeyUp(KeyCode.Y) && config.stickState != StickState.DECELERATE)
                config.stickState = config.stickState - 1;
            sticks[2] = config;

            config = sticks[3];
            if (Input.GetKeyUp(KeyCode.U) && config.stickState != StickState.ACCELERATE)
                config.stickState = config.stickState + 1;
            else if (Input.GetKeyUp(KeyCode.I) && config.stickState != StickState.DECELERATE)
                config.stickState = config.stickState - 1;
            sticks[3] = config;

            config = sticks[4];
            if (Input.GetKeyUp(KeyCode.O) && config.stickState != StickState.ACCELERATE)
                config.stickState = config.stickState + 1;
            else if (Input.GetKeyUp(KeyCode.P) && config.stickState != StickState.DECELERATE)
                config.stickState = config.stickState - 1;
            sticks[4] = config;

            config = sticks[5];
            if (Input.GetKeyUp(KeyCode.LeftBracket) && config.stickState != StickState.ACCELERATE)
                config.stickState = config.stickState + 1;
            else if (Input.GetKeyUp(KeyCode.RightBracket) && config.stickState != StickState.DECELERATE)
                config.stickState = config.stickState - 1;
            sticks[5] = config;
        }
    }



    IEnumerator TimeCountDownCoroutine()
    {
        while (currentGameTime <= totalGameTime)
        {
            if (gameState == GameState.STARTED)
            {
                currentGameTime += Time.deltaTime;  
            }

            yield return new WaitForFixedUpdate();
        }

        yield break;
    }



}
