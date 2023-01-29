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

    public enum GameState { PAUSED, STARTED}
    public GameState gameState = GameState.STARTED;

    [Header("Joystick")]
    public List<JoystickLevelConfig> stickLevels = new List<JoystickLevelConfig>();



    protected override void Start()
    {
        base.Start();

        InitGame();
    }


    public void PauseGame()
    {
        UpateGameState(GameState.PAUSED);
    }

    public void UnpauseGame()
    {
        UpateGameState(GameState.STARTED);
    }

    public void UpateGameState(GameState state)
    {
        gameState = state;
    }

    public void InitGame()
    {
        PauseGame();
        JoystickStatePanel.Instance.Show();
    }
}
