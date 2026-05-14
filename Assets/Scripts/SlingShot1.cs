using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingShot1 : MonoBehaviour
{
    public Transform Projectile;
    public Transform DrawFrom;
    public Transform DrawTo;

    public SlingShotString slingshotString;
    public int NrDrawIncrements = 10;

    private Transform currentProjectile;

    private bool draw;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            DrawSlingShot(1);
        if (Input.GetKeyUp(KeyCode.LeftShift))
            ReleaseAndShoot(50);
    }

    public void ReleaseAndShoot(float shotForce)
    {
        draw = false;
        currentProjectile.transform.parent = null;
        Rigidbody projectileRigidBody = currentProjectile.GetComponent<Rigidbody>();
        projectileRigidBody.isKinematic = false;
        projectileRigidBody.AddForce(transform.forward * shotForce, ForceMode.Impulse);
        slingshotString.CenterPoint = DrawFrom;
    }

    public void DrawSlingShot(float speed)
    {
        draw = true;
        currentProjectile = Instantiate(Projectile, DrawFrom.position, Quaternion.identity, transform);
        currentProjectile.forward = transform.forward;
        slingshotString.CenterPoint = currentProjectile.transform;

        float waitTimeBetweenDraws = speed / NrDrawIncrements;
        StartCoroutine(drawSlingShotWithIncrements(waitTimeBetweenDraws));
    }

    private IEnumerator drawSlingShotWithIncrements(float waitTimeBetweenDraws)
    {
        for (int i = 0; i < NrDrawIncrements; i++)
        {
            if (draw)
            {
                currentProjectile.transform.position = Vector3.Lerp(DrawFrom.position, DrawTo.position, (float)i / NrDrawIncrements);
                yield return new WaitForSeconds(waitTimeBetweenDraws);
            }
            else
            {
                i = NrDrawIncrements;
            }
        }
    }

}
