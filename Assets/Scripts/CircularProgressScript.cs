using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircularProgressScript : MonoBehaviour {

	public float selectionDuration = 2.0f;
    public float fadeDuration = 0.5f;
    public float worldScale = 0.05f;
	public GameObject parentCanvas = null;

    private LookAtObject ownerObject = null;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {	
	}

    public IEnumerator FillCircularProgress(GameObject selectedObject, bool fadeWithAction)
    {
        //set up owner Object
        LookAtObject lao = selectedObject.GetComponent<LookAtObject>();
        ownerObject = lao;

        // Create a timer and reset the fill amount.
        float timer = 0f;

        GameObject mainCamera = Camera.main.gameObject;

        float perspectiveScale = Vector3.Magnitude(selectedObject.transform.position - mainCamera.transform.position);

//      if (mainCamera) {
//			transform.localScale = transform.localScale / mainCamera.transform.parent.localScale.x * perspectiveScale;
//		}
		
		Image image = gameObject.GetComponent<Image>();
		image.fillAmount = 0f;

		CameraScript cScript = null;
        Camera cam = null;
        Vector3 sideVec = Vector3.zero;

		if (mainCamera) {
            cScript = mainCamera.GetComponent<CameraScript>();
            cam = mainCamera.GetComponent<Camera>();
            sideVec = mainCamera.transform.right;
        }

        // This loop is executed once per frame until the timer exceeds the duration.
        while (timer < selectionDuration && timer >= 0.0f)
		{
			// The image's fill amount requires a value from 0 to 1 so we normalise the time.
			image.fillAmount = timer / selectionDuration;

            bool circularProgressInView = false;
            Vector3 screenPos3 = cam.WorldToScreenPoint(selectedObject.transform.position);
            Vector3 screenPos3ForRadius = cam.WorldToScreenPoint(selectedObject.transform.position + sideVec * worldScale * perspectiveScale);

            //GameObject DebugSphere = GameObject.Find("DebugSphere");
            //DebugSphere.transform.position = selectedMarker.transform.position;
            //DebugSphere.transform.localScale = Vector3.one * worldScale * perspectiveScale;

            Vector2 screenPos2 = new Vector2(screenPos3.x / cam.pixelWidth, screenPos3.y / cam.pixelHeight);
            Vector2 screenPos2ForRadius = new Vector2(screenPos3ForRadius.x / cam.pixelWidth, screenPos3ForRadius.y / cam.pixelHeight);

            float screenRadius = Vector2.Distance(screenPos2, screenPos2ForRadius) * 0.5f;

            if (Vector2.Distance(screenPos2, new Vector2(0.5f, 0.5f)) < screenRadius)
                circularProgressInView = true;
            else
                circularProgressInView = false;

            // Increase the timer by the time between frames and wait for the next frame.
            if ( cScript && ( cScript.visibleObject == selectedObject || circularProgressInView ) )
				timer += Time.deltaTime;
			else
				timer -= Time.deltaTime;

			//transform.position = selectedMarker.transform.position;

			yield return null;
		}

        // When the loop is finished set the fill amount to be full.
        if (timer > 0.0f)
        {
            image.fillAmount = 1f;

            //stop circlullar progress routines for other objects
            CircularProgressScript[] circularProgressScripts = FindObjectsOfType<CircularProgressScript>();

            foreach (CircularProgressScript script in circularProgressScripts)
            {
                if (script != this)
                {
                    if (script.ownerObject.circularProgressCoroutine != null)
                    {
                        script.ownerObject.StopCoroutine(script.ownerObject.circularProgressCoroutine);
                        script.ownerObject.circularProgressCoroutine = null;
                    }
                    Destroy(script.gameObject);
                }
            }

            if (lao != null)
            {
                if (fadeWithAction)
                {
                    yield return StartCoroutine(cScript.m_VRCameraFade.FadeWithAction(fadeDuration, lao._processObjectMethodToCall));
                }
                else
                    lao.ProcessObject();
            }
        }
        else
        {
            image.fillAmount = 0f;
        }

        Destroy(parentCanvas);

        if (lao != null)
            lao.circularProgressCoroutine = null;
    }
}
