using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public int DistanceThreshold = 0;
    public int RPMThreashold = 0;
    public int alertDecay = 0;
    public int alertUpperBound = 0;
    public int alertLowerBound = 0;
    public int hornAlert = 0;


    private int alert = 0;


    private void Start()
    {
        Excavator.Instance.hornAction += OnExcavatorHorn;
    }

    private void Update()
    {
        UpdateAlert();
        
    }


    private void UpdateAlert()
    {
        Transform trans = Excavator.Instance.transform;

        float distance = Vector3.Distance(transform.position, Excavator.Instance.transform.position);
        float distanceFactor = Math.Clamp(DistanceThreshold - distance, 0, DistanceThreshold);
        int RPMFactor = Math.Clamp(Excavator.Instance.EngineRPM - RPMThreashold, 0, int.MaxValue);

        alert += Mathf.FloorToInt(RPMFactor * distanceFactor * Time.deltaTime);
        alert -= Mathf.FloorToInt(alertDecay * Time.deltaTime);
        alert = Math.Clamp(alert, alertLowerBound, alertUpperBound);


    }

    private void OnExcavatorHorn()
    {
        alert += hornAlert;
    }


    void OnGUI()
    {
        GUI.TextArea(new Rect(0, 240, 250, 40), $"Alert : {alert}");
    }

}
