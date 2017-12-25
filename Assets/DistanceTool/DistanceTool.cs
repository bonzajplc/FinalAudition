/*
 * Created by Wes McDermott - 2011 - the3dninja.com/blog
*/

using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]

public class DistanceTool: MonoBehaviour
{
	public Color lineColor = Color.yellow;
    [HideInInspector]
    public bool _initialized = false;
	public Transform startPoint = null;
	public Transform endPoint = null;

    public float gizmoRadius = 0.1f;
    public float distanceScale = 1.0f;
    public bool scaleToPixels = false;
	public int pixelPerUnit = 128;

    private void Update()
    {
    }

    void OnEnable()
	{
		
	}
	
	void OnDrawGizmosSelected()
	{
		Gizmos.color = this.lineColor;

        if( startPoint && endPoint)
        {
            Gizmos.DrawLine(startPoint.position, endPoint.position);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
	{
        if (!endPoint || !startPoint)
            return;

        Gizmos.color = this.lineColor;

        Gizmos.DrawLine(startPoint.position, endPoint.position);
        startPoint.localScale = new Vector3(gizmoRadius, gizmoRadius, gizmoRadius);
        endPoint.localScale = new Vector3(gizmoRadius, gizmoRadius, gizmoRadius);

        //lables and handles:
        GUIStyle style = new GUIStyle();

        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;

        float distance = Vector3.Distance(startPoint.position, endPoint.position) * distanceScale;
        float scalePerPixel = distance * pixelPerUnit;

        if (scaleToPixels)
        {
            UnityEditor.Handles.Label(endPoint.position, "       Distance from Start point: " + distance + " - Scale per pixel: " + scalePerPixel + "px", style);
        }
        else
        {
            UnityEditor.Handles.Label(endPoint.position, "        Distance from Start point: " + distance, style);
        }
    }
#endif
}

