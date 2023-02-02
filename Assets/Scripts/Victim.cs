using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundEffect;

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

    public Animator animator;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Excavator"))
        {
            if (!isDead)
            {
                isDead = true;
                SoundEffectManager manager = SoundEffectManager.Instance;
                manager.PlayOneShot(manager.singleAudioSourceList[1], "Collision");
                GameplayManager controller = GameplayManager.Instance;
                controller.UpdateGameScore(-1 * controller.socrePunishment);

                if (animator != null)
                    animator.SetBool("IsDead", isDead);

                SelfDestroy();
            }
        }
    }


    private void SelfDestroy()
    {
        if(headRb != null)
            headRb.useGravity = true;
        if (bodyRb != null)
            bodyRb.useGravity = true;
        if (arm1Rb != null)
            arm1Rb.useGravity = true;
        if (arm2Rb != null)
            arm2Rb.useGravity = true;
        if (leg1Rb != null)
            leg1Rb.useGravity = true;
        if (leg2Rb != null)
            leg2Rb.useGravity = true;

        StartCoroutine(DespawnCoroutine());
    }

    IEnumerator DespawnCoroutine()
    {
        yield return new WaitForSecondsRealtime(4.0f);
        ObjectPoolManager.Instance.Despawn(gameObject);
    }
}
