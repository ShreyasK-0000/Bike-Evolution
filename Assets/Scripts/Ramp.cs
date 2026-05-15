using System;
using System.Collections;
using UnityEngine;

public class Ramp : MonoBehaviour
{
    [SerializeField]private SlingShot slingShot;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bike"))
        {
            Debug.Log("Hello");
            slingShot.StopAllCoroutines();
            slingShot.SetWheelColliders(false); 
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("bike"))
        {
            StartCoroutine(Wait());
            Debug.Log("...");

        }
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
        slingShot.SetWheelColliders(true);
    }
}
