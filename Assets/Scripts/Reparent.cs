using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Reparent : MonoBehaviour {

    public Transform childTransform = null;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        childTransform.position = transform.position;
        childTransform.rotation = transform.rotation;
        childTransform.localScale = transform.localScale;
    }
}
