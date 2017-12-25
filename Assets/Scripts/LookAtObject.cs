using UnityEngine;
using System.Collections;

public class LookAtObject : MonoBehaviour
{
    public delegate void ProcessHitMethod(); // This defines what type of method you're going to call.
    public ProcessHitMethod _processHitMethodToCall = null; // This is the variable holding the method you're going to call.

    public delegate void ProcessObjectMethod(); // This defines what type of method you're going to call.
    public ProcessObjectMethod _processObjectMethodToCall = null; // This is the variable holding the method you're going to call.

    public GameObject worldSpaceCanvasPrefab;

    public bool fadeWithAction = false;

    public Coroutine circularProgressCoroutine = null;

    private static bool _lockCameraRaycast = false;

    public static bool lockCameraRaycast
    {
        set { _lockCameraRaycast = value; }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ProcessHit()
    {
        if (_lockCameraRaycast)
            return;

        if ( _processHitMethodToCall != null )
            _processHitMethodToCall();

        if (circularProgressCoroutine == null )
        {
            GameObject worldSpaceCanvas = (GameObject)Instantiate(worldSpaceCanvasPrefab);


            CircularProgressScript cpScript = worldSpaceCanvas.GetComponentInChildren<CircularProgressScript>();
            cpScript.parentCanvas = worldSpaceCanvas;
            worldSpaceCanvas.transform.position = transform.position;

            CameraScript ccScript = Camera.main.GetComponent<CameraScript>();

            if( ccScript )
                circularProgressCoroutine = ccScript.StartCoroutine(cpScript.FillCircularProgress(gameObject, fadeWithAction));
        }
    }

    public void ProcessObject()
    {
        if (_processObjectMethodToCall != null)
            _processObjectMethodToCall();
    }
}
