using UnityEngine;

namespace Cinemachine.Examples
{

[AddComponentMenu("")] // Don't display in add component menu
public class ActivateCamera : MonoBehaviour
{
    public CinemachineVirtualCameraBase switchToCam;

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            if (switchToCam) 
            {
                switchToCam.MoveToTopOfPrioritySubqueue();
            }  
        }
    }
    
}

}
