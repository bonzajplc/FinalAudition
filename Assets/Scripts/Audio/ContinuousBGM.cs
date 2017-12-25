using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousBGM : MonoBehaviour
{
    [HideInInspector]
    public AudioSource                audioSource_     =   null;
    [HideInInspector]
    public static  ContinuousBGM       staticBGMObject_ =   null;

    #if UNITY_EDITOR
    public  float   sceneTime   =   0.0f;
    public  bool    seekAudio   =   false;
    #endif

    void Start()
    {
        audioSource_    =   GetComponent<AudioSource>();

        if (audioSource_ != null)
        {
            if (staticBGMObject_ ==  null)
            {
                if (audioSource_.clip != null)
                {
                    staticBGMObject_    =   this;
                    #if UNITY_EDITOR
                    if (seekAudio)
                        staticBGMObject_.audioSource_.time  =   sceneTime;
                    #endif
                    staticBGMObject_.audioSource_.Play();
                    DontDestroyOnLoad(this.gameObject);
                }
                else
                {
                    Destroy(this.gameObject);
                }
            }
            else
            {
                if (this.audioSource_ != null)
                {
                    if (this.audioSource_.clip.name != staticBGMObject_.audioSource_.clip.name)
                    {
                        Destroy(staticBGMObject_.gameObject);
                        staticBGMObject_ =   this;
                        #if UNITY_EDITOR
                        if (seekAudio)
                            staticBGMObject_.audioSource_.time  =   sceneTime;
                        #endif
                        staticBGMObject_.audioSource_.Play();
                        DontDestroyOnLoad(this.gameObject);
                    }
                    else
                    {
                        Destroy(this.gameObject);
                    }
                }
                else
                {
                    staticBGMObject_.audioSource_.Stop();
                    Destroy(this.gameObject);
                    Destroy(staticBGMObject_.gameObject);
                    staticBGMObject_ =   null;
                }
            }
        }
    }
}
