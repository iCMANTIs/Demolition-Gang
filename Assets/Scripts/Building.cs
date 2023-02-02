using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public GameObject victimPrefab;
    public GameObject originModel;
    public GameObject brokenModel;
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

                originModel.SetActive(!isBroken);
                brokenModel.SetActive(isBroken);

                StartCoroutine(DespawnCoroutine());
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

    IEnumerator DespawnCoroutine()
    {
        yield return new WaitForSecondsRealtime(4.0f);
        ObjectPoolManager.Instance.Despawn(gameObject);
    }

}
