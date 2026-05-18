using JetBrains.Annotations;
using System;
using UnityEngine;

public class WheellAnimation : MonoBehaviour
{
    [System.Serializable]
    public struct WheelData
    {
        public WheelCollider wheelCollider;
        public Transform wheelMesh;
    }

    public WheelData[] wheels;

    [SerializeField] private SlingShot slingShot;
    [SerializeField] private float pullRotationSpeed = 180f;  
    [SerializeField] private Rigidbody bikeRigidbody;


    private void Update()
    {
        HandleRotationOfWheel();
    }


    private void HandleRotationOfWheel()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].wheelMesh == null) continue;

            if (slingShot.gameState == SlingShot.State.bikeDragged)
            {
                wheels[i].wheelMesh.Rotate(Vector3.right, -pullRotationSpeed * Time.deltaTime); //rotate wheel backwards
            }
            else if (slingShot.gameState == SlingShot.State.bikeLaunched)
            {
                if (wheels[i].wheelCollider != null && wheels[i].wheelCollider.enabled)
                {
                    wheels[i].wheelCollider.GetWorldPose(out Vector3 wheelPos, out Quaternion wheelRot);
                    wheels[i].wheelMesh.rotation = wheelRot;
                }
                else
                {
                    //Debug.Log("...");
                    //float speed = bikeRigidbody.linearVelocity.magnitude;
                    //float wheelRadius = 0.35f; // adjust to match your wheel size
                    //float rpm = (speed / (2 * Mathf.PI * wheelRadius)) * 360f;
                    //wheels[i].wheelMesh.Rotate(Vector3.right, rpm * Time.deltaTime);
                }
            }
           
        }
    }
}
