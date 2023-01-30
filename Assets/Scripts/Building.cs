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
    public int alertThreshold = 0;
    public float alertFactor = 0.3f;
    
    public int hornPunishment = 0;

    public GameObject victimPrefab;
    public List<Transform> victimPosList = new List<Transform>();


    private int alert = 0;
    public int Alert { get { return alert; } }

    private bool isBroken = false;
    public bool IsBroken { get { return isBroken; } set { isBroken = value; } }


    private void Start()
    {
        Excavator.Instance.hornAction += OnNoiseOccur;
        GameplayManager.Instance.victimSpawnAction += SpawnVictims;
    }

    private void Update()
    {
        UpdateAlert();
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Excavator"))
        {
            if (!isBroken)
            {
                isBroken = true;
                GameplayManager controller = GameplayManager.Instance;
                controller.UpdateGameScore(controller.socreAward);
            }
        }
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

        alert += Mathf.FloorToInt(RPMFactor * distanceFactor * Time.deltaTime * alertFactor);
        alert -= Mathf.FloorToInt(alertDecay * Time.deltaTime);
        alert = Math.Clamp(alert, alertLowerBound, alertUpperBound);

        Debug.Log($"{gameObject.name}, " +
                  $"alert increase: {Mathf.FloorToInt(RPMFactor * distanceFactor * Time.deltaTime)}, " +
                  $"alert decay: {Mathf.FloorToInt(alertDecay * Time.deltaTime)}, " +
                  $"alert: {alert}");

        GameplayPanel panel = GameplayPanel.Instance;
        panel.enemyAlert = Math.Max(panel.enemyAlert, alert);
    }


    private void OnNoiseOccur(string noiseType)
    {
        switch (noiseType)
        {
            case "horn":
                alert += hornPunishment;
                break;
            default:
                alert += 0;
                break;

        }
    }


    private void SpawnVictims()
    {
        for (int i = 0; i < victimPosList.Count; i++)
        {
            GameObject victim = ObjectPoolManager.Instance.Spawn(victimPrefab, victimPosList[i].position, Quaternion.identity);
        }
    }


    void OnGUI()
    {
        GUI.TextArea(new Rect(0, 300, 250, 40), $"Alert : {alert}");
    }

}
