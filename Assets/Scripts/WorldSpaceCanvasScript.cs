using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceCanvasScript : MonoBehaviour
{
    public float worldScale = 0.05f;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt( Camera.main.transform.position, Vector3.up);
        float perspectiveScale = worldScale * Vector3.Magnitude(transform.position - Camera.main.transform.position);

        transform.localScale = new Vector3(perspectiveScale,perspectiveScale,perspectiveScale);
    }
}
