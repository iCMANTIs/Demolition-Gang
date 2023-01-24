using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour
{

    public AudioSource audioSource;


    private void OnTriggerEnter(Collider other)
    {
        Victim victim = other.transform.GetComponent<Victim>();
        if (victim != null && victim.IsDead == false)
        {
            audioSource.Play();
            victim.IsDead = true;
        }
    }



    //private void OnCollisionEnter(Collision collision)
    //{
    //    Victim victim = collision.transform.GetComponent<Victim>();
    //    if (victim != null && victim.IsDead == false)
    //    {
    //        audioSource.Play();
    //        victim.IsDead = true;
    //    }
    //}
}
