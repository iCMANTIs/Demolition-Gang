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
    public int Alert { get { return alert; } }



    private void Start()
    {
        Excavator.Instance.hornAction += OnExcavatorHorn;
    }

    private void Update()
    {
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
    private void UpdateAlert()
    {
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
