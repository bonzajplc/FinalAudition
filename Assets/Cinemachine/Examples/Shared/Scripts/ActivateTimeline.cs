using UnityEngine;
using UnityEngine.Playables;

namespace Cinemachine.Examples
{

[AddComponentMenu("")] // Don't display in add component menu
public class ActivateTimeline : MonoBehaviour
{

    private PlayableDirector director;

	// Use this for initialization
	void Start ()
	{
	    director = GetComponent<PlayableDirector>();
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            director.Play();
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            director.Stop();
        }
    }
}

}