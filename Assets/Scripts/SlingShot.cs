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

    public enum State { elasticBandIdle, elasticBandDragging, elasticBandReleased}
    public State gameState = State.elasticBandIdle;

    private Plane dragPlane;

    [SerializeField] private float dragSensitivity = 0.5f; // tune 0.1 to 1.0
    [SerializeField] private WheelCollider[] wheelColliders;
    [SerializeField] private float stabilizationForce = 50f;

    private void Start()
    {

        ResetBand();

    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            FullReset();

        switch (gameState)
        {
            case State.elasticBandIdle: HandleIdle(); break;
            case State.elasticBandDragging: HandleDragging(); break;
        }
    }

    private void HandleIdle()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, bikePrefabMask))
        {
            dragPlane = new Plane(Vector3.up, drawFrom.position);
            gameState = State.elasticBandDragging;
            rigidBodyOfProjectile.isKinematic = true;
            SetWheelColliders(false);
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

        Vector3 worldPoint = ray.GetPoint(enter);
        worldPoint.y = drawFrom.position.y;

        Vector3 offset = (worldPoint - drawFrom.position) * dragSensitivity;

        // Block forward pull
        float forwardAmount = Vector3.Dot(offset, drawFrom.forward);
        if (forwardAmount > 0f)
            offset -= forwardAmount * drawFrom.forward;

        if (offset.magnitude > maxPullDistance)
            offset = offset.normalized * maxPullDistance;

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
        rigidBodyOfProjectile.AddForce(launchDir * forceMagnitude, ForceMode.Impulse);

        gameState = State.elasticBandReleased;
        ResetBand();

        //StartCoroutine(EnableWheelsWhenGrounded());
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
        StartCoroutine(EnableWheelsAfterReset());
        gameState = State.elasticBandIdle;
    }

    private void UpdateBand()
    {
        elasticBand.SetPositions(new Vector3[3]
        {
            leftPoint.position,
            projectile.position,
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
            Debug.Log(wc.enabled);
        }
    }

    private IEnumerator EnableWheelsWhenGrounded()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            // Increased from 1.2f — needs to reach the ground from bike center
            bool nearGround = Physics.Raycast(projectile.position, Vector3.down, 2.0f);
            bool movingDownOrStopped = rigidBodyOfProjectile.linearVelocity.y <= 0.5f;

            if (nearGround && movingDownOrStopped)
            {
                SetWheelColliders(true);
                yield break;
            }
        }
    }

    private IEnumerator EnableWheelsAfterReset()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        SetWheelColliders(true);
    }
}
