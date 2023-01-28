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


    private int alert = 0;




    private void Update()
    {
        
    }


    private void UpdateAlert()
    {
        float distance = Vector3.Distance(transform.position, Excavator.Instance.transform.position);

        float DistanceFactor = Math.Clamp(DistanceThreshold - distance, 0, DistanceThreshold);
        int RPMFactor = Math.Clamp(Excavator.Instance.EngineRPM - RPMThreashold, 0, int.MaxValue);

        alert += Mathf.FloorToInt(RPMFactor * DistanceFactor * Time.deltaTime);




    }

}
