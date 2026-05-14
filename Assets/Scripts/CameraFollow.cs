using UnityEditor.Rendering;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private SlingShot slingShot;

    private Transform initialCameraTransform;

    private void Start()
    {
        initialCameraTransform = gameObject.transform;
    }

    private void Update()
    {
        if (slingShot.gameState == SlingShot.State.elasticBandIdle)
        {
            gameObject.transform.SetParent(null);
            gameObject.transform.position = initialCameraTransform.position;
            gameObject.transform.rotation = initialCameraTransform.rotation;
            gameObject.transform.localScale = initialCameraTransform.localScale;
        }
        else if (slingShot.gameState == SlingShot.State.elasticBandReleased)
        { 
            gameObject.transform.SetParent(slingShot.projectile.transform);
        }
    }
}
