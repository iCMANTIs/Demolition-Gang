using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayPanel : DestroyableSingleton<GameplayPanel>
{

    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;


    protected override void Update()
    {
        base.Update();

        UpdateGameTimer();
        UpdateGameSocre();
    }


    public void UpdateGameTimer()
    {
        GameplayController controller = GameplayController.Instance;
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


    public void UpdateGameSocre()
    {

        scoreText.text = $"{GameplayController.Instance.TotalScore}";

    }


}
