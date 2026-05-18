using System;
using System.Collections;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;

public class HighScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalDistanceTravelled;
    [SerializeField] private GameObject bike;
    [SerializeField] private SlingShot slingshot;
    [SerializeField] private GameObject distanceTravelledFlag;
    private Vector3 startingPositionOfBike;
    private int highScore = 0;
    private int currentDistance = 0;
    private bool isCoroutineRunning = false;
    private Rigidbody bikeRigidbody;
    private bool bikeStopped = false;
    private GameObject flagPrefab;
    private Vector3 flagOffSet = new Vector3(-4, 0, 2);

    /*Basic scoring system for now */

    private void Start()
    {
        //PlayerPrefs.DeleteKey("HighScore");//for testing purpose only
        highScore = PlayerPrefs.GetInt("HighScore");
        Debug.Log("highscore = " + highScore);
        startingPositionOfBike = bike.transform.position;
    }

    private void Update()
    {
        HandleBikeLaunched();
        HandleBikeStopped();
    }


    private void HandleBikeLaunched()
    {
        if(slingshot.gameState == SlingShot.State.bikelaunched)
        {
            bikeRigidbody = bike.GetComponent<Rigidbody>();
            //if bike is moving then calculate realtime distance and show
            if (bikeRigidbody.angularVelocity.sqrMagnitude > 0.001f || bikeRigidbody.linearVelocity.sqrMagnitude > 0.001f)
            {
                Vector3 distance = bike.transform.position - startingPositionOfBike;
                currentDistance = Convert.ToInt16(distance.z);
                totalDistanceTravelled.text = currentDistance.ToString() + "m";
            }
            else
            {
                if(!isCoroutineRunning)
                {
                    isCoroutineRunning = true;
                    StartCoroutine(WaitForConfirmation());
                }
                if(bikeStopped)
                {
                    if(flagPrefab == null)
                    {
                        flagPrefab = Instantiate(distanceTravelledFlag, bikeRigidbody.transform.position + flagOffSet, Quaternion.identity);
                    }
                    else
                    {
                        // not needed in build
                        flagPrefab.transform.position = bikeRigidbody.transform.position + flagOffSet;
                    }
                    slingshot.gameState = SlingShot.State.bikeStopped;
                    return;
                }

            }
        }
    }

    private IEnumerator WaitForConfirmation()
    {
        yield return new WaitForSeconds(1);

        Debug.Log(bikeRigidbody.angularVelocity.sqrMagnitude + "\t" + bikeRigidbody.linearVelocity.sqrMagnitude);
        if (bikeRigidbody.angularVelocity.sqrMagnitude > 0.001f || bikeRigidbody.linearVelocity.sqrMagnitude > 0.001f)
        {
            bikeStopped = false;
        }
        else
        {
            bikeStopped = true;
        }
        isCoroutineRunning = false;
    }

    private void HandleBikeStopped()
    {
        if (slingshot.gameState == SlingShot.State.bikeStopped)
        {
            if (currentDistance > highScore)
            {
                highScore = currentDistance;
                PlayerPrefs.SetInt("HighScore", highScore);
                Debug.Log("new highscore = " + currentDistance);
            }
        } 
    }
}
