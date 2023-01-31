using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public GameObject victimPrefab;
    public List<Transform> victimPosList = new List<Transform>();

    private bool isBroken = false;
    public bool IsBroken { get { return isBroken; } set { isBroken = value; } }


    private void Start()
    {
        AlertManager.Instance.alertAction += SpawnVictims;
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


    private void SpawnVictims()
    {
        for (int i = 0; i < victimPosList.Count; i++)
        {
            GameObject victim = ObjectPoolManager.Instance.Spawn(victimPrefab, victimPosList[i].position, Quaternion.identity);
        }
    }

}
