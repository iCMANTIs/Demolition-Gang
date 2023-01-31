using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Victim : MonoBehaviour
{
    private bool isDead = false;
    public bool IsDead { get { return isDead; } set { isDead = value; } }


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
            if (!isDead)
            {
                isDead = true;
                GameplayManager controller = GameplayManager.Instance;
                controller.UpdateGameScore(-1 * controller.socrePunishment);
                SelfDestroy();
            }
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
        ObjectPoolManager.Instance.Despawn(gameObject);
    }
}
