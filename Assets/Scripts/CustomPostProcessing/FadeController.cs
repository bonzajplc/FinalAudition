using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;

[ExecuteInEditMode]
public class FadeController : MonoBehaviour
{
    public event Action OnFadeComplete;                   // This is called when the fade in or out has finished.

    public Color fadeColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

    public LayerMask postProcessingLayer = -1;           

    public Color autoFadeInColor = Color.black;
    public Color autoFadeOutColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    private Color internalFadeColor = Color.black;


    public AudioMixerSnapshot m_DefaultSnapshot = null;   // Settings for the audio mixer to use normally.
    public AudioMixerSnapshot m_FadedSnapshot = null;     // Settings for the audio mixer to use when faded out.

    private PostProcessVolume m_Volume;

    private bool m_IsFading = false;                      // Whether the screen is currently auto fading.
    private bool m_IsFadingNow = false;                      // Whether the screen is currently manually fading.
    private Fade fadeProfile;

    void Start()
    {
        fadeProfile = ScriptableObject.CreateInstance<Fade>();
        fadeProfile.enabled.Override(true);
        fadeProfile.color.Override(fadeColor);

        m_Volume = PostProcessManager.instance.QuickVolume(Mathf.RoundToInt(Mathf.Log(postProcessingLayer, 2)), 100f, fadeProfile);
    }

    void Update()
    {
        if (m_IsFading|| m_IsFadingNow)
        {
            fadeProfile.color.value = internalFadeColor;
            m_IsFadingNow = false;
        }
        else
            fadeProfile.color.value = fadeColor;
    }

    public void FadeInNow(float blend)
    {
        m_IsFadingNow = true;
        internalFadeColor = Color.Lerp(autoFadeOutColor, autoFadeInColor, blend);
    }

    public void FadeOutNow(float blend)
    {
        m_IsFadingNow = true;
        internalFadeColor = Color.Lerp(autoFadeInColor, autoFadeOutColor, blend);
    }

    public void FadeOut(float duration, bool fadeAudio)
    {
        // If not already fading start a coroutine to fade from the fade out colour to the fade colour.
        if (m_IsFading)
            return;
        StartCoroutine(BeginFade(autoFadeInColor, autoFadeOutColor, duration));

        // Fade out the audio over the same duration.
        if (m_FadedSnapshot && fadeAudio)
            m_FadedSnapshot.TransitionTo(duration);
    }

    public void FadeIn(float duration, bool fadeAudio)
    {
        // If not already fading start a coroutine to fade from the fade colour to the fade out colour.
        if (m_IsFading)
            return;
        StartCoroutine(BeginFade(autoFadeOutColor, autoFadeInColor, duration));

        // Fade in the audio over the same duration.
        if (m_DefaultSnapshot && fadeAudio)
            m_DefaultSnapshot.TransitionTo(duration);
    }

    public IEnumerator BeginFadeOut(float duration, bool fadeAudio)
    {
        // Fade out the audio over the given duration.
        if (m_FadedSnapshot && fadeAudio)
            m_FadedSnapshot.TransitionTo(duration);

        yield return StartCoroutine(BeginFade(autoFadeInColor, autoFadeOutColor, duration));
    }

    public IEnumerator BeginFadeIn(float duration, bool fadeAudio)
    {
        // Fade in the audio over the given duration.
        if (m_DefaultSnapshot && fadeAudio)
            m_DefaultSnapshot.TransitionTo(duration);

        yield return StartCoroutine(BeginFade(autoFadeOutColor, autoFadeInColor, duration));
    }

    public IEnumerator FadeWithAction(float duration, LookAtObject.ProcessObjectMethod ProcessObject)
    {
        LookAtObject.lockCameraRaycast = true;
        yield return StartCoroutine(BeginFade(autoFadeOutColor, autoFadeInColor, duration));

        ProcessObject();

        yield return StartCoroutine(BeginFade(autoFadeOutColor, autoFadeInColor, duration));
        LookAtObject.lockCameraRaycast = false;
    }

    private IEnumerator BeginFade(Color startCol, Color endCol, float duration)
    {
        // Fading is now happening.  This ensures it won't be interupted by non-coroutine calls.
        m_IsFading = true;

        // Execute this loop once per frame until the timer exceeds the duration.
        float timer = 0f;
        while (timer <= duration)
        {
            // Set the colour based on the normalised time.
            internalFadeColor = Color.Lerp(startCol, endCol, Mathf.Pow( timer / duration, 2.2f ) );

            // Increment the timer by the time between frames and return next frame.
            timer += Time.deltaTime;
            yield return null;
        }

        internalFadeColor = endCol;

        // Fading is finished so allow other fading calls again.
        m_IsFading = false;

        // If anything is subscribed to OnFadeComplete call it.
        if (OnFadeComplete != null)
            OnFadeComplete();
    }

    void Destroy()
    {
        RuntimeUtilities.DestroyVolume(m_Volume, true);
    }
}
