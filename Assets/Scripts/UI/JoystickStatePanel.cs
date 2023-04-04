using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class JoystickStatePanel : DestroyableSingleton<JoystickStatePanel>
{

    [Serializable]
    public struct StickIndicatorConfig
    {
        public string stickID;
        public int levelIndex;
        public RectTransform stickIndicator;
    }


    public List<StickIndicatorConfig> stickIndicators = new List<StickIndicatorConfig>();
    public Button startButton;
    public Button closeButton;


    private GameplayManager gameManager;
    private bool isReadyToStart = false;
    public GameObject LittleControl;


    protected override void Awake()
    {
        base.Awake();

        startButton.onClick.AddListener(OnClickStartButton);
        closeButton.onClick.AddListener(OnClickCloseButton);
    }


    protected override void Start()
    {
        base.Start();

        ShowLittleControl(false);

        gameManager = GameplayManager.Instance;

        InitStickIndicator();
    }


    protected override void Update()
    {
        base.Update();

        UpdateStickIndicator();
        UpdateStartButton();
    }


    public void Show()
    {
        startButton.gameObject.SetActive(gameManager.gameState == GameplayManager.GameState.UNSTARTED);
        closeButton.gameObject.SetActive(gameManager.gameState != GameplayManager.GameState.UNSTARTED);
        gameObject.SetActive(true);
    }


    public void Hide()
    {
        gameObject.SetActive(false);
    }


    private void InitStickIndicator()
    {
        foreach (StickIndicatorConfig config in stickIndicators)
        {
            float stickScale = 2.0f; // The value of the joystick goes from -1 to 1;
            var stickLevel = gameManager.stickLevels[config.levelIndex];
            RectTransform target = config.stickIndicator.GetChild(1).GetComponent<RectTransform>();

            /* Update target area starting position */
            float targetStartPos = (stickLevel.lowerBound + 1) / stickScale * config.stickIndicator.sizeDelta.y;
            Vector3 anchorPos = target.anchoredPosition;
            anchorPos.y = targetStartPos;
            target.anchoredPosition = anchorPos;

            //Debug.Log($"localPos: {target.localPosition}, anchorPos: {target.anchoredPosition}, Pos: {target.position}");

            /* Update target area size */
            float targetSizeDelta = (stickLevel.upperBound - stickLevel.lowerBound) / stickScale * config.stickIndicator.sizeDelta.y;
            Vector2 sizeDelta = target.sizeDelta;
            sizeDelta.y = targetSizeDelta;
            target.sizeDelta = sizeDelta;
        }
    }


    private void UpdateStickIndicator()
    {
        isReadyToStart = true;

        foreach(StickIndicatorConfig config in stickIndicators)
        {
            float stickScale = 2.0f; // The value of the joystick goes from -1 to 1;
            float stickValue;

            if (gameManager.useKeyBoard == false)
            {
                //if (config.stickID != stickIndicators[5].stickID)
                //    stickValue = Input.GetAxis(config.stickID);
                //else
                //    stickValue = HardwareManager.Instance.Joystick1;
                stickValue = Input.GetAxis(config.stickID);
            }
            else
            {
                GameplayManager.JoySitckConfig stick = gameManager.sticks.Find(stick => stick.stickName == config.stickID);
                stickValue = (int)stick.stickState / 2.0f;
            }

            var stickLevel = gameManager.stickLevels[config.levelIndex];
            Slider slider = config.stickIndicator.GetChild(0).GetComponent<Slider>();

            /* Update slider position */
            float ratio = (stickValue + 1) / stickScale;
            slider.value = ratio;

            /* Update label */
            TextMeshProUGUI labelText = slider.transform.Find("Handle Slide Area/Handle/Label/Text").GetComponent<TextMeshProUGUI>();
            GameObject greenBackground = slider.transform.Find("Handle Slide Area/Handle/Label/GreenBackground").gameObject;
            GameObject redBackground = slider.transform.Find("Handle Slide Area/Handle/Label/RedBackground").gameObject;

            bool isReady = (stickValue <= stickLevel.upperBound && stickValue >= stickLevel.lowerBound);
            labelText.text = isReady ? $"Ready" : $"Unset";
            greenBackground.SetActive(isReady);
            redBackground.SetActive(!isReady);
            
            if (!isReady)
                isReadyToStart = false;
        }
    }


    private void UpdateStartButton()
    {
        startButton.interactable = isReadyToStart;
        startButton.transform.Find("ActiveBackground").gameObject.SetActive(isReadyToStart);
        startButton.transform.Find("InactiveBackground").gameObject.SetActive(!isReadyToStart);
    }


    private void OnClickStartButton()
    {
        Hide();
        gameManager.StartGame();
        ShowLittleControl(true);
    }


    private void OnClickCloseButton()
    {
        Hide();
    }

    public void ShowLittleControl(bool value)
    {
        if(LittleControl == null)
        {
            return;
        }
        LittleControl.SetActive(value);
    }
}
