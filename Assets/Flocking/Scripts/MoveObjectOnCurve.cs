using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObjectOnCurve : MonoBehaviour
{
    public GameObject objectOnCurve;
    public BezierCurve curve;
    public float moveSpeed = 1f;
    private float curCurveProgress = 0f;
    private int curCurveId = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        curCurveProgress += Time.deltaTime * moveSpeed / curve.GetFirstDerivative(curCurveId, curCurveProgress).magnitude;
        if(curCurveProgress >= 1f)
        {
            curCurveId++;
            if (curCurveId >= (curve.points.Length - 1) / 3)
            {
                curCurveId = 0;
            }
            curCurveProgress = 0f;
        }
        objectOnCurve.transform.position = curve.GetPoint(curCurveId, curCurveProgress);
        objectOnCurve.transform.LookAt (curve.GetPoint(curCurveId, curCurveProgress + 0.01f));

    }
}
