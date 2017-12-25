using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class AsyncLoader : MonoBehaviour
{
    #if UNITY_EDITOR
    public  SceneAsset  nextScene;
    #endif
    public enum TimeType
    {
        SceneTime,
        BGMTime
    }

    public TimeType     timeType = TimeType.SceneTime;
    public  float       sceneLoadTime           =   1.0f;
    public bool         loadSceneInEditor =   false;
    public  string      sceneName   =   "";

    public bool fadeOut = false;
    public float fadeOutTime = 1.0f;
    public bool fadeOutOnlyInXRMode = true;

    public bool fadeIn = false;
    public float fadeInTime = 1.0f;
    public bool fadeInOnlyInXRMode = true;

    private AsyncOperation  asyncOperation_;

    private bool    almostDone_     =   false;

    private static Stopwatch stopwatch = new Stopwatch();

    float fadeOutTimer = 0.0f;

    FadeController fader = null;

    enum FadeType
    {
        FadeIn,
        FadeOut
    }

    void Start()
    {
        PlasticApplication app = FindObjectOfType<PlasticApplication>();
        //Plastic application is needed to perform it

        if (app)
        {
            if (app.XRCamera && app.XRCamera.gameObject.activeSelf)
            {
                fader = app.XRCamera.GetComponent<FadeController>();
                if (!fader)
                {
                    Debug.LogError("FadeController is needed to perform Fade!");
                }
            }
            if (app.FlatCamera && app.FlatCamera.gameObject.activeSelf && (!fadeInOnlyInXRMode || !fadeOutOnlyInXRMode))
            {
                fader = app.FlatCamera.GetComponent<FadeController>();
                if (!fader)
                {
                    Debug.LogError("FadeController is needed to perform Fade!");
                }
            }
        }
        else
        {
            Debug.LogError("Plastic Application is needed to perform FadeIn!");
        }

        if (fadeOut)
        {
            if (fadeOutTimer <= fadeOutTime)
            {
                PerformFade(FadeType.FadeOut, 0.0f);
            }
        }

                if (stopwatch.IsRunning)
        {
            stopwatch.Stop();
            Debug.LogError(SceneManager.GetActiveScene().name + " start/awake time: " + stopwatch.Elapsed);
            stopwatch.Reset();
        }
        #if UNITY_EDITOR
        if (loadSceneInEditor)
        #endif
        if(sceneName != "")
            StartCoroutine(AsyncLoad());
    }

    IEnumerator AsyncLoad()
    {
        asyncOperation_ = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation_.allowSceneActivation = false;
        
        while (!asyncOperation_.isDone)
        {
            if (asyncOperation_.progress >= 0.9f)
            {
                almostDone_ = true;
                break;
            }
            
            yield return null;
        }
    }

    void Update()
    {
        if (almostDone_)
        {
            if (timeType == TimeType.SceneTime && Time.timeSinceLevelLoad > sceneLoadTime)
            {
                stopwatch.Start();
                asyncOperation_.allowSceneActivation = true;
            }
            else if (timeType == TimeType.BGMTime)
            {
                if (ContinuousBGM.staticBGMObject_)
                {
                    if (ContinuousBGM.staticBGMObject_.audioSource_.time > sceneLoadTime)
                    {
                        stopwatch.Start();
                        asyncOperation_.allowSceneActivation = true;
                    }
                }
            }
        }
        //fade out
        if (fadeOut)
        {
            if (fadeOutTimer <= fadeOutTime)
            {
                PerformFade(FadeType.FadeOut, Mathf.Pow(fadeOutTimer / fadeOutTime, 2.2f));
            }
            fadeOutTimer += Time.deltaTime;
        }

        //fade in
        if (fadeIn)
        {
            if (timeType == TimeType.SceneTime && Time.timeSinceLevelLoad > sceneLoadTime - fadeInTime)
            {
                PerformFade(FadeType.FadeIn, Mathf.Pow((Time.timeSinceLevelLoad - sceneLoadTime + fadeInTime) / fadeInTime, 2.2f));
            }
            else if (timeType == TimeType.BGMTime)
            {
                if (ContinuousBGM.staticBGMObject_)
                {
                    if (ContinuousBGM.staticBGMObject_.audioSource_.time > sceneLoadTime - fadeInTime)
                    {
                        PerformFade(FadeType.FadeIn, Mathf.Pow((ContinuousBGM.staticBGMObject_.audioSource_.time - sceneLoadTime + fadeInTime) / fadeInTime, 2.2f));
                    }
                }
            }
        }
    }

    void PerformFade(FadeType type, float blend)
    {
        if (!fader)
            return;

        if (type == FadeType.FadeIn)
            fader.FadeInNow(blend);
        else if(type == FadeType.FadeOut)
            fader.FadeOutNow(blend);
    }
}
