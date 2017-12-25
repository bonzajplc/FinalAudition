using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlasticApplication : MonoBehaviour {

    public Camera XRCamera = null;
    public Camera FlatCamera = null;

    public PlayableDirector XRTimeline = null;
    public PlayableDirector FlatTimeline = null;

    public bool quitApplication = false;
    public float quitTime = 0.0f;

    private void Awake()
    {
        //setup cameras
        if (UnityEngine.XR.XRSettings.enabled)
        {
            //enable XR camera
            if (XRCamera != null)
                XRCamera.gameObject.SetActive(true);

            //disable Flat camera
            if (FlatCamera != null)
                FlatCamera.gameObject.SetActive(false);
        }
        else
        {
            //disable XR camera
            if (XRCamera != null)
            {
                XRCamera.gameObject.SetActive(false);

                //disable parent structure (character controller, floor)
                XRCamera.transform.parent.gameObject.SetActive(false);
            }

            //enable Flat camera
            if (FlatCamera != null)
                FlatCamera.gameObject.SetActive(true);
        }

        //setup timelines
        if (UnityEngine.XR.XRSettings.enabled)
        {
            //enable XR timeline
            if (XRTimeline != null)
                XRTimeline.gameObject.SetActive(true);

            //disable Flat timeline
            if (FlatTimeline != null)
                FlatTimeline.gameObject.SetActive(false);
        }
        else
        {
            //disable XR timeline
            if (XRTimeline != null)
                XRTimeline.gameObject.SetActive(false);

            //enable Flat timeline
            if (FlatTimeline != null)
                FlatTimeline.gameObject.SetActive(true);
        }

        // Check if BGM object exists on scene, if not destroy static BGM
        ContinuousBGM[] BGMs    =   FindObjectsOfType<ContinuousBGM>();

		if (BGMs.Length == 1)
        {
            if (ContinuousBGM.staticBGMObject_ != null)
            {
                Destroy(ContinuousBGM.staticBGMObject_.gameObject);
                ContinuousBGM.staticBGMObject_  =   null;
            }
        }
    }
    // Use this for initialization
    void Start()
    {
        Cursor.visible = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey("escape"))
            Application.Quit(); 

        if( quitApplication && Time.timeSinceLevelLoad > quitTime )
            Application.Quit();
    }
}
