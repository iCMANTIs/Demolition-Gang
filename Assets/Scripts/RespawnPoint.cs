using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundEffect;

public class RespawnPoint : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Excavator"))
        {
            Excavator.Instance.respawnPoint = transform.position;
        }
    }


}
