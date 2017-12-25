using UnityEngine;

namespace Cinemachine.Examples
{

[AddComponentMenu("")] // Don't display in add component menu
public class ActivateObject : MonoBehaviour
{
    public GameObject cartToMove;
    public CinemachineVirtualCameraBase switchToCam;
    public CinemachinePathBase.PositionUnits positionUnits = CinemachinePathBase.PositionUnits.Distance;
    public float speed = 5f;

    private CinemachineDollyCart dCartComp;
    void Start()
    {
        if (cartToMove)
        {
            dCartComp = cartToMove.GetComponent<CinemachineDollyCart>();
            dCartComp.m_Speed = 0f;    
        }
        
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player") && switchToCam && cartToMove)
        {
            if (Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera.Name != switchToCam.Name)
            {
                dCartComp.m_PositionUnits = positionUnits;
                dCartComp.m_Speed = speed;

                switchToCam.VirtualCameraGameObject.SetActive(false);
                switchToCam.VirtualCameraGameObject.SetActive(true);        
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player") && switchToCam != null && cartToMove)
        {
            dCartComp.m_Speed = 0f;
            switchToCam.VirtualCameraGameObject.SetActive(false);
        }
    }
}

}
