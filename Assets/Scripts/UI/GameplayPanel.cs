using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayPanel : DestroyableSingleton<GameplayPanel>
{

    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI alertText;
    public TextMeshProUGUI alertStateText;

    public RectTransform alertIndicator;


    protected override void Start()
    {
        base.Start();

        InitAlertIndicator();
        UpdateAlertState();

        AlertManager.Instance.alertAction += UpdateAlertState;
    }


    protected override void Update()
    {
        base.Update();

        UpdateGameTimer();
        UpdateGameSocre();
        UpdateVictimAlert();
        UpdateAlertIndicator();
    }


    private void UpdateGameTimer()
    {
        GameplayManager controller = GameplayManager.Instance;
        float second = controller.totalGameTime - controller.CurrentGameTime;
        string text = "";
        float sec = second % 60;
        float min = second / 60;

        if (min < 10)
            text += $"0{(int)min}";
        else
            text += $"{(int)min}";

        if (sec < 10)
            text += $":0{(int)sec}";
        else
            text += $":{(int)sec}";

        timerText.text = text;

        if (second < 10f)
            timerText.color = Color.red;
        else
            timerText.color = Color.white;
    }


    private void UpdateGameSocre()
    {
        scoreText.text = $"{GameplayManager.Instance.TotalScore}";
    }


    private void UpdateVictimAlert()
    {
        alertText.text = $"{AlertManager.Instance.Alert}";
    }

    private void UpdateAlertState()
    {
        alertStateText.text = $"State: {AlertManager.Instance.alertState}";
    }

    private void InitAlertIndicator()
    {
        float alertScale = AlertManager.Instance.alertUpperBound;
        float alertThreshold = AlertManager.Instance.alertThreshold;
        RectTransform target = alertIndicator.GetChild(1).GetComponent<RectTransform>();

        /* Update target area starting position */
        float targetStartPos = alertThreshold / alertScale * alertIndicator.sizeDelta.x;
        Vector3 anchorPos = target.anchoredPosition;
        anchorPos.x = targetStartPos;
        target.anchoredPosition = anchorPos;

        //Debug.Log($"localPos: {target.localPosition}, anchorPos: {target.anchoredPosition}, Pos: {target.position}");

        /* Update target area size */
        float targetSizeDelta = (alertScale - alertThreshold) / alertScale * alertIndicator.sizeDelta.x;
        Vector2 sizeDelta = target.sizeDelta;
        sizeDelta.x = targetSizeDelta;
        target.sizeDelta = sizeDelta;
    }


    private void UpdateAlertIndicator()
    {
        float alertScale = AlertManager.Instance.alertUpperBound;
        float alertValue = AlertManager.Instance.Alert;
        Slider slider = alertIndicator.GetChild(0).GetComponent<Slider>();

        /* Update slider position */
        float ratio = alertValue / alertScale;
        slider.value = ratio;
    }
}
