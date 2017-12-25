using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DensityFieldInstancer : MonoBehaviour
{
    public Material material = null;
    public MarchingCubesRenderer marchingCubesRenderer = null;
    public WireframeCageRenderer wireCageRenderer = null;
    public WireframeMarchingCubesRenderer wireframeMarchingCubesRenderer = null;

    [HideInInspector]
    public MaterialPropertyBlock matPropertyBlock = null;

    private void Start()
    {
        matPropertyBlock = new MaterialPropertyBlock();
    }
    // Update is called once per frame
    void Update()
    {
        if (matPropertyBlock == null)
            return;

        matPropertyBlock.SetMatrix("_LocalToWorld", Matrix4x4.Translate(-transform.position) * transform.localToWorldMatrix);
        matPropertyBlock.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);

        Material mat = material;
        
        if (marchingCubesRenderer && marchingCubesRenderer.isActiveAndEnabled)
        {
            if (mat == null)
                mat = marchingCubesRenderer.mat;

            Graphics.DrawMeshInstancedIndirect(
                 marchingCubesRenderer.emptyMesh, 0, mat,
                 new Bounds(transform.position, transform.lossyScale * 1.5f),
                 marchingCubesRenderer.argBuffer, 0, matPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, null);
        }
        if (wireCageRenderer && wireCageRenderer.isActiveAndEnabled)
        {
            if (mat == null)
                mat = wireCageRenderer.mat;

            Graphics.DrawMeshInstancedIndirect(
                 wireCageRenderer.emptyMesh, 0, mat,
                 new Bounds(transform.position, transform.lossyScale * 1.5f),
                 wireCageRenderer.argBuffer, 0, matPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, null);
        }
        if (wireframeMarchingCubesRenderer && wireframeMarchingCubesRenderer.isActiveAndEnabled)
        {
            if (mat == null)
                mat = wireframeMarchingCubesRenderer.mat;

            Graphics.DrawMeshInstancedIndirect(
                 wireframeMarchingCubesRenderer.emptyMesh, 0, mat,
                 new Bounds(transform.position, transform.lossyScale * 1.5f),
                 wireframeMarchingCubesRenderer.argBuffer, 0, matPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false, 0, null);
        }
    }
}
