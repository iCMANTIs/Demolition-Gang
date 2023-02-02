using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictimMovePath : MonoBehaviour
{

    public Vector3 Pos1;
    public Vector3 Pos2;

    public float speed = 0f;






    private void Start()
    {
        StartCoroutine(PatrolCoroutine());
    }

    IEnumerator PatrolCoroutine()
    {
        Vector3 direction = Vector3.Normalize(Pos1 - Pos2);

        while (true)
        {


            transform.Translate(direction * speed * Mathf.Sin(Time.fixedTime) * Time.deltaTime);


            //Debug.Log($"{Mathf.Sin(Time.fixedTime)}");



            //Vector3 currentPos = Vector3.Lerp(Pos1, Pos2, Mathf.Abs(Mathf.Sin(Time.fixedTime)));
            //currentPos.y = y;
            //transform.position = currentPos;

            yield return null;
        }
    }


}
