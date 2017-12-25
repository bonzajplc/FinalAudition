using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DensityObject : MonoBehaviour
{
    public enum DensityObjectType
    {
        sphere, box
    };
    public DensityObjectType objectType;

    public float param0 = 1;
    public float param1 = 1;
    public float param2 = 1;

    private void Awake()
    {

    }

    private void Start()
    {
    }

    private void Update()
    {
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, 1.0f * param0);
    }
#endif
}
