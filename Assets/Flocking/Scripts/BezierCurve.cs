using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum BezierControlPointMode
{
    Free,
    Aligned,
    Mirrored,
}
public class BezierCurve : MonoBehaviour
{

    public Vector3[] points = new Vector3[4]
    {
        new Vector3(1f,0f, 0f),
        new Vector3(2f,0f, 0f),
        new Vector3(3f,0f, 0f),
        new Vector3(4f,0f, 0f)
    };

    public BezierControlPointMode[] modes = new BezierControlPointMode[2]{
        BezierControlPointMode.Free,
        BezierControlPointMode.Free
    };
    public void AddCurve()
    {
        Vector3 point = points[points.Length-1];
        Array.Resize(ref points, points.Length+3);
        point.x += 1f;
        points[points.Length-3] = point;
        point.x += 1f;
        points[points.Length - 2] = point;
        point.x += 1f;
        points[points.Length - 1] = point;

        Array.Resize(ref modes, modes.Length+1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
    }

    public void SetPoint(int index, Vector3 point)
    {
        if(index %3 == 0)
        {
            Vector3 delta= point - points[index];
            if (index > 0)
            {
                points[index - 1] += delta;
            }
            if(index < points.Length -1) {
                points[index + 1] += delta;
            }
        }
        
        points[index] = point;
        EnforceMode(index);
    }
    public BezierControlPointMode GetBezierControlPointMode(int index)
    {
        return modes[(index + 1) / 3];
    }

    public void SetBezierControlPointMode(int index, BezierControlPointMode mode)
    {
        modes[(index + 1) / 3] = mode;
        EnforceMode(index);
    }

    void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = modes[modeIndex];
        if(mode == BezierControlPointMode.Free || modeIndex == 0 || modeIndex  == modes.Length - 1)
        {
            return;
        }
        int middleIndex = modeIndex * 3;
        int fixIndex, enforcedIndex;
        if(index <= middleIndex)
        {
            fixIndex = middleIndex - 1;
            enforcedIndex = middleIndex + 1;
        }
        else
        {
            fixIndex = middleIndex + 1;
            enforcedIndex = middleIndex - 1;
        }

        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixIndex];

        if(mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }

        points[enforcedIndex] = middle + enforcedTangent;
    }
    public Vector3 GetPoint(int curveId, float t)
    {
        Vector3 p0 = points[curveId * 3];
        Vector3 p1 = points[curveId * 3 + 1];
        Vector3 p2 = points[curveId * 3 + 2];
        Vector3 p3 = points[curveId * 3 + 3];
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        Vector3 posLocal =
            oneMinusT * oneMinusT * oneMinusT * p0 +
            3f * oneMinusT * oneMinusT * t * p1 +
            3f * oneMinusT * t * t * p2 +
            t * t * t * p3;
        return transform.TransformPoint(posLocal);
    }

    public Vector3 GetFirstDerivative(int curveId, float t)
    {
        Vector3 p0 = points[curveId * 3];
        Vector3 p1 = points[curveId * 3 + 1];
        Vector3 p2 = points[curveId * 3 + 2];
        Vector3 p3 = points[curveId * 3 + 3];
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            3f * oneMinusT * oneMinusT * (p1 - p0) +
            6f * oneMinusT * t * (p2 - p1) +
            3f * t * t * (p3 - p2);
    }


}
