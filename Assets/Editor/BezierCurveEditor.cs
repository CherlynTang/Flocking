using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(BezierCurve))]
public class BezierCurveEditor : Editor
{
    Transform controllerTransform;
    Quaternion handleRotation;
    BezierCurve curve;
    int selectedIndex = -1;
    private void OnSceneGUI()
    {
        curve = target as BezierCurve;
        Handles.color = Color.white;
        controllerTransform = curve.transform;
        handleRotation  = Tools.pivotRotation == PivotRotation.Local ? 
            controllerTransform.rotation : Quaternion.identity;

        Vector3 p0 = ShowPoint(0);
        for (int i = 1; i < curve.points.Length; i+=3)
        {
            Vector3 p1 = ShowPoint(i);
            Vector3 p2 = ShowPoint(i+1);
            Vector3 p3 = ShowPoint(i+2);
            Handles.color = Color.white;
            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 10f);
            Handles.color = Color.green;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);
            p0 = p3;
        }

    }
    Vector3 ShowPoint(int id)
    {
        Vector3 point = controllerTransform.TransformPoint(curve.points[id]);
        Handles.color = Color.green;
        if(Handles.Button(point, handleRotation, 0.1f, 0.13f, Handles.DotHandleCap))
        {
            selectedIndex = id;
            Repaint();
        }
        if(selectedIndex ==  id)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.PositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(curve, "Move Point");
                EditorUtility.SetDirty(curve);
                curve.SetPoint(id, controllerTransform.InverseTransformPoint(point));
            }
        }
        return point;
    }

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        curve = target as BezierCurve;
        if (selectedIndex>= 0 && selectedIndex < curve.points.Length)
        {
            GUILayout.Label("Selected Point");
            EditorGUI.BeginChangeCheck();
            Vector3 point = EditorGUILayout.Vector3Field("Position", curve.points[selectedIndex]);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(curve, "Move Point");
                EditorUtility.SetDirty(curve);     
                curve.SetPoint(selectedIndex, point);
            }
        }
        EditorGUI.BeginChangeCheck();
        BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", curve.GetBezierControlPointMode(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curve, "ChangePointMode");
            EditorUtility.SetDirty(curve);
            curve.SetBezierControlPointMode(selectedIndex, mode);
        }

        if (GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(curve, "Add Curve");
            curve.AddCurve();
            EditorUtility.SetDirty(curve);
        }
    }
}
