using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricityBox : MonoBehaviour
{
    private bool isBroken = false;
    public bool IsBroken { get { return isBroken; } set { isBroken = value; } }


    public GameObject originModel;
    public GameObject brokenModel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Excavator"))
        {
            if (!isBroken)
            {
                isBroken = true;
                GameplayController controller = GameplayController.Instance;
                controller.UpdateGameScore(controller.socreAward);
                
                originModel.SetActive(!isBroken);
                brokenModel.SetActive(isBroken);

                StartCoroutine(DespawnCoroutine());
            }
        }
    }


    IEnumerator DespawnCoroutine()
    {
        yield return new WaitForSecondsRealtime(4.0f);
        Destroy(gameObject);
    }

}
