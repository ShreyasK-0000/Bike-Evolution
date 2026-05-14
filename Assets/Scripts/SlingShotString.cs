using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingShotString : MonoBehaviour
{
    public Transform LeftPoint;
    public Transform RightPoint;
    public Transform CenterPoint;

    LineRenderer slingshotString;

    // Start is called before the first frame update
    void Start()
    {
        slingshotString = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        slingshotString.SetPositions(new Vector3[3] { LeftPoint.position, CenterPoint.position, RightPoint.position });
    }
}
