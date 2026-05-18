using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.WSA;

public class SlingShot : MonoBehaviour
{
    public Transform bikeTransform;
    public enum State { bikeIdle, bikeDragged, bikeLaunched, bikeStopped };
    public State gameState = State.bikeIdle;

    private Plane dragPlane;

    [SerializeField] private float maxPullDistance = 2f;
    [SerializeField] private float launchForce = 2000f;

    [SerializeField] private float bikeDragSensitivity = 0.5f;

    [SerializeField] private float randomSideDrfitForce = 1f;
    [SerializeField] private float minFrontFlipTorque = 0.2f;
    [SerializeField] private float maxFrontFlipTorque = 4f;
    [SerializeField] private float randomLeftRightSpinTorque = 1f;
    [SerializeField] private float randomSideLeanTorque = 2f;

    [SerializeField] private float randomNudgeInAir = 1.5f;

    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private Transform drawFrom;

    [SerializeField] private LineRenderer elasticBand;
    [SerializeField] private Rigidbody bikeRigidbody;

    [SerializeField] private LayerMask bikeLayerMask;

    [SerializeField] private WheelCollider[] wheelColliders;
    

    private void Start()
    {
        ResetBike();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            FullReset();
        }
        switch(gameState)
        {
            case State.bikeIdle: HandleBikeIdle();  break;
            case State.bikeDragged: HandleBikeDragged(); break;
        }
    }

    private void HandleBikeIdle()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, bikeLayerMask))
        {
            dragPlane = new Plane(Vector3.up, drawFrom.position);
            gameState = State.bikeDragged;
            bikeRigidbody.isKinematic = true;
            SetWheelColliders(false);
        }
        
    }

    public void SetWheelColliders(bool enable)
    {
        foreach(Collider wheelCollider in wheelColliders)
        {
            wheelCollider.enabled = enable;
        }
    }

    private void HandleBikeDragged()
    {
        if (Input.GetMouseButtonUp(0))
        {
            LaunchBike(); return;
        }
        
        //do planecast and check distance to plane which gives us the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!dragPlane.Raycast(ray, out float distanceToPlane)) return;
        Vector3 worldPoint = ray.GetPoint(distanceToPlane);
        worldPoint.y = drawFrom.position.y;

        Vector3 offSet = (worldPoint - drawFrom.position) * bikeDragSensitivity;//backward bike drag/pull
        float forwardAmount = Vector3.Dot(offSet, drawFrom.forward); // calculate forward offset(or check bike push)
        if(forwardAmount > 0)
        {
            offSet -= forwardAmount * drawFrom.forward;
        }
        if(offSet.magnitude > maxPullDistance)
        {
            offSet = offSet.normalized * maxPullDistance;
        }

        bikeTransform.position = offSet + drawFrom.position;

        Vector3 toCenter = drawFrom.position - bikeTransform.position;
        if(toCenter != Vector3.zero)
        {
            bikeTransform.forward = toCenter.normalized;
        }

        UpdateElasticBand();
    }

    private void UpdateElasticBand()
    {
        elasticBand.SetPositions(new Vector3[3] {leftPoint.position,bikeTransform.position, rightPoint.position});
    }

    private void LaunchBike()
    {
        Vector3 launchDirection = (drawFrom.position - bikeTransform.position).normalized;
        float stretch = Vector3.Distance(drawFrom.position, bikeTransform.position);

        bikeRigidbody.isKinematic = false;

        //apply launch, side and torque forces
        bikeRigidbody.AddForce(launchDirection * launchForce * stretch, ForceMode.Impulse);
        float sideTwist = UnityEngine.Random.Range(-randomSideDrfitForce, randomSideDrfitForce);
        bikeRigidbody.AddForce(drawFrom.right * sideTwist, ForceMode.Impulse);
        Vector3 randomTorque = new Vector3(
            UnityEngine.Random.Range(minFrontFlipTorque,maxFrontFlipTorque),
            UnityEngine.Random.Range(-randomLeftRightSpinTorque,randomLeftRightSpinTorque),
            UnityEngine.Random.Range(-randomSideLeanTorque,randomSideLeanTorque)
            );
        bikeRigidbody.AddTorque(randomTorque,ForceMode.Impulse);

        gameState = State.bikeLaunched;
        ResetBand();
        StartCoroutine(ApplyAirPhysics());

    }

    private IEnumerator ApplyAirPhysics()
    {
        while (gameState == State.bikeLaunched)
        {
            yield return new WaitForFixedUpdate();

            //apply air friction force, spin and nudge
            bikeRigidbody.angularDamping = 0.2f;
            bikeRigidbody.linearDamping = 0.1f;

            if(UnityEngine.Random.value < 0.05f)
            {
                Vector3 randomAirForce = new Vector3 (UnityEngine.Random.Range(-randomNudgeInAir, randomNudgeInAir),0f,0f);
                bikeRigidbody.AddForce(randomAirForce, ForceMode.Force);
            }
        }

    }

    private void FullReset()
    {
        StopAllCoroutines();
        SetWheelColliders(false);
        ResetBike();
        gameState = State.bikeIdle;
        StartCoroutine(EnableWheelsAfterReset()); 
    }

    private IEnumerator EnableWheelsAfterReset()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        SetWheelColliders(true);
    }

    private void ResetBike()
    {
        bikeRigidbody.linearVelocity = Vector3.zero;
        bikeRigidbody.angularVelocity = Vector3.zero;
        bikeTransform.position = drawFrom.position;
        bikeTransform.rotation = drawFrom.rotation;
        ResetBand();
    }

    private void ResetBand()
    {
        elasticBand.SetPositions(new Vector3[3] { leftPoint.position, drawFrom.position, rightPoint.position });
    }
}
