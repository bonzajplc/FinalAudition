using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UIPositioner : MonoBehaviour
{
    public Camera AttachCamera;

    public Vector3 v3Pos = new Vector3(0.0f, 1.0f, 0.25f);
	
    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.position = AttachCamera.ViewportToWorldPoint(v3Pos);
    }
}
