using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class SlingShot : MonoBehaviour
{

    [SerializeField] private float maxPullDistance = 2f;
    [SerializeField] private float shotForce = 10f;

    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    [SerializeField] public Transform projectile;
    [SerializeField] private Transform drawFrom;
        
    [SerializeField] private LineRenderer elasticBand;

    [SerializeField] private Rigidbody rigidBodyOfProjectile;

    [SerializeField] private LayerMask bikePrefabMask;

    public enum State { bikeIdle, bikedragged, bikelaunched, bikeStopped}
    public State gameState = State.bikeIdle;

    private Plane dragPlane;

    [SerializeField] private float dragSensitivity = 0.5f; 
    [SerializeField] private WheelCollider[] wheelColliders;
    [SerializeField] private float stabilizationForce = 50f;

    [Header("Launch Randomness")]
    public float randomSideForce = 2f;      
    public float minFlipTorque = 1f;        
    public float maxFlipTorque = 8f;        
    public float randomYawTorque = 1f;      
    public float randomSideLean = 2f;       

    [Header("Air Physics")]
    public float airSideForce = 1.5f;       
    public float balanceTorque = 0.05f;     


    private void Start()
    {

        ResetBand();

    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            FullReset();
        }


        switch (gameState)
        {
            case State.bikeIdle: HandleIdle(); break;
            case State.bikedragged: HandleDragging(); break;
        }
    }

    private void HandleIdle()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, bikePrefabMask))
        {
            dragPlane = new Plane(Vector3.up, drawFrom.position); // create a invisible plane at slingshot height
            gameState = State.bikedragged;
            rigidBodyOfProjectile.isKinematic = true;
            //SetWheelColliders(false);
        }
    }

    private void HandleDragging()
    {
        if (Input.GetMouseButtonUp(0))
        {
            LaunchBike();
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!dragPlane.Raycast(ray, out float enter)) return; 

        Vector3 worldPoint = ray.GetPoint(enter); // gets the world position fo mouse by projecting the ray on plane 
        worldPoint.y = drawFrom.position.y; 

        Vector3 offset = (worldPoint - drawFrom.position) * dragSensitivity; //gets direction and distance of pull

        float forwardAmount = Vector3.Dot(offset, drawFrom.forward); // calculate dot product to get the direction of pull
        if (forwardAmount > 0f)
            offset -= forwardAmount * drawFrom.forward;

        if (offset.magnitude > maxPullDistance)
            offset = offset.normalized * maxPullDistance; // clamp the maximum pull distance

        projectile.position = drawFrom.position + offset;

        Vector3 toCenter = drawFrom.position - projectile.position;
        if (toCenter != Vector3.zero) 
            projectile.forward = toCenter.normalized;

        UpdateBand();
    }

    private void LaunchBike()
    {
        Vector3 launchDir = (drawFrom.position - projectile.position).normalized;
        float stretch = Vector3.Distance(projectile.position, drawFrom.position);
        float forceMagnitude = stretch * shotForce;

        rigidBodyOfProjectile.isKinematic = false;

        
        rigidBodyOfProjectile.AddForce(launchDir * forceMagnitude, ForceMode.Impulse);//launch force

        
        float sideTwist = UnityEngine.Random.Range(-randomSideForce, randomSideForce);//Random side force 
        rigidBodyOfProjectile.AddForce(drawFrom.right * sideTwist, ForceMode.Impulse);

        Vector3 randomTorque = new Vector3(
            UnityEngine.Random.Range(minFlipTorque, maxFlipTorque),   // front or back flip
            UnityEngine.Random.Range(-randomYawTorque, randomYawTorque), // side spin
            UnityEngine.Random.Range(-randomSideLean, randomSideLean)    // side lean
        );
        rigidBodyOfProjectile.AddTorque(randomTorque, ForceMode.Impulse);

        gameState = State.bikelaunched;
        ResetBand();
        //StartCoroutine(EnableWheelsWhenGrounded());
        StartCoroutine(ApplyAirPhysics()); 
    }


    private void FullReset()
    {
        StopAllCoroutines();
        SetWheelColliders(false);

        rigidBodyOfProjectile.isKinematic = true;
        rigidBodyOfProjectile.linearVelocity = Vector3.zero;
        rigidBodyOfProjectile.angularVelocity = Vector3.zero;
        rigidBodyOfProjectile.constraints = RigidbodyConstraints.None;

        projectile.position = drawFrom.position;
        projectile.rotation = drawFrom.rotation;

        elasticBand.SetPositions(new Vector3[3]
        {
            leftPoint.position,
            drawFrom.position,
            rightPoint.position
        });

        rigidBodyOfProjectile.isKinematic = false;
        //StartCoroutine(EnableWheelsAfterReset());
        gameState = State.bikeIdle;
    }

    private void UpdateBand()
    {
        elasticBand.SetPositions(new Vector3[3]
        {
            leftPoint.position,
            projectile.position, // keep updating middle point of band to follow bike
            rightPoint.position
        });
    }

    private void ResetBand()
    {
        projectile.position = drawFrom.position;
        elasticBand.SetPositions(new Vector3[3]
        {
            leftPoint.position,
            drawFrom.position,
            rightPoint.position
        });
    }

    public void SetWheelColliders(bool enabled)
    {
        foreach (WheelCollider wc in wheelColliders)
        {
            wc.enabled = enabled;
            //Debug.Log(wc.enabled);
        }
    }
    private IEnumerator ApplyAirPhysics()
    {
        while (gameState == State.bikelaunched)
        {
            yield return new WaitForFixedUpdate();

            rigidBodyOfProjectile.angularDamping = 0.2f; // resist large spin
            rigidBodyOfProjectile.linearDamping = 0.1f; // let's have slight air resistance

            //randomly sideways nudge
            if (UnityEngine.Random.value < 0.05f) 
            {
                Vector3 randomAirForce = new Vector3(
                    UnityEngine.Random.Range(-airSideForce, airSideForce),
                    0f,
                    0f
                );
                rigidBodyOfProjectile.AddForce(randomAirForce, ForceMode.Force);
            }
        }
    }
}
