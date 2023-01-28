using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flashlight : MonoBehaviour
{

    public GameObject flashlightON;
    public GameObject flashlightOFF;
    private bool isON;


    // Start is called before the first frame update
    void Start()
    {
        flashlightON.SetActive(false);
        flashlightOFF.SetActive(true);
        isON = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {

            if(isON)
            {
                flashlightON.SetActive(false);
                flashlightOFF.SetActive(true);
            }

            else
            {
                flashlightON.SetActive(true);
                flashlightOFF.SetActive(false);
            }

            isON = !isON;
        }
    }
}