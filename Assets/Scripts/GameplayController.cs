using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayController : DestroyableSingleton<GameplayController>
{
    [Serializable]
    public struct JoystickLevelConfig
    {
        public float lowerBound;
        public float upperBound;
    }

    [Header("Joystick")]
    public List<JoystickLevelConfig> stickLevels = new List<JoystickLevelConfig>();

    [Header("GameplaySetting")]
    public float totalGameTime = 0f;
    public float scoreFactor = 100f;
    public float socrePunishment = 500f;
    public float socreAward = 2000f;

    public enum GameState { PAUSED, STARTED}
    public GameState gameState = GameState.PAUSED;
    public enum AlertState { ALERT, STEALTH}
    public AlertState alertState = AlertState.STEALTH;




    private float currentGameTime = 0f;
    private float timeScore = 0f;
    private float actionScore = 0f;
    public float CurrentGameTime { get { return currentGameTime; } }
    public int TotalScore { get { return Mathf.FloorToInt(timeScore + actionScore); } }

    protected override void Start()
    {
        base.Start();

        InitGame();
    }

    protected override void Update()
    {
        base.Update();

        UpdateGameScore();





        if (Input.GetKey(KeyCode.PageUp))
        {
            alertState = AlertState.ALERT;
        }


    }


    public void InitGame()
    {
        currentGameTime = 0f;

        PauseGame();
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

    public void UpdateGameState(GameState state)
    {
        gameState = state;
    }

    public void UpdateGameScore(float actionScore = 0)
    {
        this.actionScore += actionScore;

        if (alertState == AlertState.ALERT)
            scoreFactor = 0f;

        timeScore = scoreFactor * (totalGameTime - currentGameTime);
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
