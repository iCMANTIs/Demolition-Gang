using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertManager : DestroyableSingleton<AlertManager>
{
    [Serializable]
    public struct AlertConfig
    {
        public Transform building;
        public int alert;
    }
    public enum AlertState { ALERT, STEALTH }


    [Header("Alert Setting")]
    public int distanceThreshold = 0;
    public int RPMThreashold = 0;
    public int alertDecay = 0;
    public int alertUpperBound = 0;
    public int alertLowerBound = 0;
    public int alertThreshold = 0;
    public float alertFactor = 0.3f;

    [Header("Noise Punishment")]
    public int hornPunishment = 0;
    public int crashPunishment = 0;


    public List<AlertConfig> alertConfigs = new List<AlertConfig>();
    public AlertState alertState = AlertState.STEALTH;
    public Action alertAction;


    private int alert = 0;
    public int Alert { get { return alert; } }


    protected override void Start()
    {
        base.Start();

        Excavator.Instance.hornAction += OnNoiseOccur;
    }


    protected override void Update()
    {
        base.Update();


        UpdateBuildingAlert();
        UpdateAlert();

    }

    /* The method to calculate alert is: 
     * 1: Calculate the result of (current RPM) - (RPM threshold)
     * 2: If result smaller than 0, engine RPM will not affect enemy alert.
     * 3: If result larger than 0, enemy alert = result * distance between player and building.
     * 4: Noted that if player is further than the distance threshold, distance will not 
     *    affect enemy alert. As player getting closer to the building, enemy is easier to be
     *    alerted.
     */
    private void UpdateBuildingAlert()
    {
        for (int i = 0; i < alertConfigs.Count; i++)
        {
            AlertConfig config = alertConfigs[i];
            Excavator excavator = Excavator.Instance;
            float distance = Vector3.Distance(config.building.position, excavator.transform.position);
            float distanceFactor = Math.Clamp(distanceThreshold - distance, 0, distanceThreshold);
            int RPMFactor = Math.Clamp(excavator.EngineRPM - RPMThreashold, 0, int.MaxValue);

            config.alert += Mathf.FloorToInt(RPMFactor * distanceFactor * Time.deltaTime * alertFactor);
            config.alert -= Mathf.FloorToInt(alertDecay * Time.deltaTime);
            config.alert = Math.Clamp(config.alert, alertLowerBound, alertUpperBound);

            alertConfigs[i] = config;

            //Debug.Log($"{config.building.name}, " +
            //          $"alert increase: {Mathf.FloorToInt(RPMFactor * distanceFactor * Time.deltaTime * alertFactor)}, " +
            //          $"alert decay: {Mathf.FloorToInt(alertDecay * Time.deltaTime)}, " +
            //          $"alert: {config.alert},  current alert: {alert}" );
        }
    }

    private void UpdateAlert()
    {
        int maxAlert = int.MinValue;

        foreach (AlertConfig config in alertConfigs)
        {
            maxAlert = Math.Max(maxAlert, config.alert);
        }

        alert = maxAlert;
        if (alert > alertThreshold)
            UpdateAlertState();
    }


    public void UpdateAlertState()
    {
        if (alertState != AlertState.ALERT)
        {
            alertState = AlertState.ALERT;
            alertAction.Invoke();
        }
    }

    private void OnNoiseOccur(string noiseType)
    {
        for (int i = 0; i < alertConfigs.Count; i++)
        {
            AlertConfig config = alertConfigs[i];
            switch (noiseType)
            {
                case "horn":
                    config.alert += hornPunishment;
                    break;
                case "crash":
                    config.alert += crashPunishment;
                    break;
                default:
                    config.alert += 0;
                    break;

            }

            alertConfigs[i] = config;
        }
    }



    void OnGUI()
    {
        GUI.TextArea(new Rect(0, 300, 250, 40), $"Alert : {alert}");
    }

}
