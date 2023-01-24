using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Victim : MonoBehaviour
{
    public Rigidbody headRb;
    public Rigidbody bodyRb;
    public Rigidbody arm1Rb;
    public Rigidbody arm2Rb;
    public Rigidbody leg1Rb;
    public Rigidbody leg2Rb;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Excavator"))
        {
            SelfDestroy();
        }
    }


    private void SelfDestroy()
    {
        headRb.useGravity = true;
        bodyRb.useGravity = true;
        arm1Rb.useGravity = true;  
        arm2Rb.useGravity = true;  
        leg1Rb.useGravity = true;
        leg2Rb.useGravity = true;

        StartCoroutine(DespawnCoroutine());
    }

    IEnumerator DespawnCoroutine()
    {
        yield return new WaitForSecondsRealtime(4.0f);
        Destroy(gameObject);
    }
}
