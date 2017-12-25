using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VR = UnityEngine.VR;
using VRStandardAssets.Utils;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class CameraScript : MonoBehaviour {

    [SerializeField] private LayerMask m_ExclusionLayers = -1;           // Layers to exclude from the raycast.

    [SerializeField] private bool showDebugRay = false;                  // Optionally show the debug ray.
    [SerializeField] private float debugRayLength = 5f;           // Debug ray length.
    [SerializeField] private float debugRayDuration = 1f;         // How long the Debug ray will remain visible.
    [SerializeField] private float rayLength = 500f;              // How far into the scene the ray is cast.
    [SerializeField] private bool showReticle = true;
    [SerializeField] private Reticle m_Reticle = null;

    [HideInInspector] // Hides var below
    public GameObject visibleObject = null;           // active marker to hide

    public float horizontalMouseSpeed = 2.0F;
	public float verticalMouseSpeed = 2.0F;

    public FadeController m_VRCameraFade = null;

    /// <summary>
    /// If true, dynamic resolution will be enabled
    /// </summary>
    public bool enableAdaptiveResolution = false;

    [RangeAttribute(0.5f, 2.0f)]
    public float renderScale = 1.5f;
    /// <summary>
    /// Max RenderScale the app can reach under adaptive resolution mode ( enableAdaptiveResolution = ture );
    /// </summary>
    [RangeAttribute(0.5f, 2.0f)]
    public float maxRenderScale = 1.0f;

    /// <summary>
    /// Min RenderScale the app can reach under adaptive resolution mode ( enableAdaptiveResolution = ture );
    /// </summary>
    [RangeAttribute(0.5f, 2.0f)]
    public float minRenderScale = 0.7f;

    private static bool _isUserPresentCached = false;
    private static bool _isUserPresent = false;

    public static Vector3 cameraHeadWorldSpace = Vector3.zero;

    private static float _gpuScale = 1.0f;
    /// <summary>
    /// True if the user is currently wearing the display.
    /// </summary>
    public bool IsUserPresent
    {
        get
        {
            if (!_isUserPresentCached)
            {
                _isUserPresentCached = true;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                _isUserPresent = OVRPlugin.userPresent;
#else
				_isUserPresent = false;
#endif
            }

            return _isUserPresent;
        }

        private set
        {
            _isUserPresentCached = true;
            _isUserPresent = value;
        }
    }

    void Awake () {
#if UNITY_ANDROID
        GetComponent<PostProcessLayer>().enabled = false;
        renderScale = 0.75f;
#endif
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = renderScale;

        transform.localPosition = new Vector3(0.0f, 1.7f, 0.0f);
    }

    private void Start()
    {
        if (!showReticle)
            m_Reticle.Hide();
    }

    // Update is called once per frame
    void Update () {

		float h = horizontalMouseSpeed * Input.GetAxis("Mouse X");
		float v = verticalMouseSpeed * Input.GetAxis("Mouse Y");

        if (!IsUserPresent)
            transform.Rotate(v, h, 0);

        // Show the debug ray if required
        if (showDebugRay)
        {
            Debug.DrawRay(transform.position, transform.forward * debugRayLength, Color.blue, debugRayDuration);
        }

        // Create a ray that points forwards from the camera.
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

		visibleObject = null;

        // Do the spherecast forweards to see if we hit an interactive item
		if( Physics.Raycast( ray, out hit, rayLength, ~m_ExclusionLayers ) )
        {
			if (m_Reticle)
				m_Reticle.SetPosition( hit );

            LookAtObject lookAtObject = hit.transform.GetComponent<LookAtObject>();

            if (lookAtObject != null)
            {
                //Debug.Log("hit " + hit.transform.name);
                visibleObject = hit.transform.gameObject;

                lookAtObject.ProcessHit();
            }
        }
        else
        {
            // Position the reticle at default distance.
			if (m_Reticle)
				m_Reticle.SetPosition();
        }

        //adaptive resolution
        if (enableAdaptiveResolution)
        {
            float scalingFactor = GetPerformanceScaleFactor();

            if (UnityEngine.XR.XRSettings.eyeTextureResolutionScale < maxRenderScale)
            {
                // Allocate renderScale to max to avoid re-allocation
                UnityEngine.XR.XRSettings.eyeTextureResolutionScale = maxRenderScale;
            }
            else
            {
                // Adjusting maxRenderScale in case app started with a larger renderScale value
                maxRenderScale = Mathf.Max(maxRenderScale, UnityEngine.XR.XRSettings.eyeTextureResolutionScale);
            }

            scalingFactor = Mathf.Clamp(scalingFactor, minRenderScale, maxRenderScale);
            UnityEngine.XR.XRSettings.eyeTextureResolutionScale = scalingFactor;
            //VR.VRSettings.renderViewportScale = scalingFactor;

            //Debug.Log("scaleFactor: " + scalingFactor);
        }

        cameraHeadWorldSpace = transform.position;
    }

    float GetPerformanceScaleFactor()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        OVRPlugin.AppPerfStats stats = OVRPlugin.GetAppPerfStats();
        float scale = Mathf.Sqrt(stats.AdaptiveGpuPerformanceScale);
#else
		float scale = 1.0f;
#endif
        _gpuScale = Mathf.Clamp(_gpuScale * scale, 0.5f, 2.0f);
        return _gpuScale;
    }
}
