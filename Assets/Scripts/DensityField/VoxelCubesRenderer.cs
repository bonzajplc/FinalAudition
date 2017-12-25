using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VoxelCubesRenderer : DensityFieldRenderer {

    public float isoLevel = 1.0f;

    void Awake()
    {
        AwakeRenderer("VoxelCubes");
    }

    // Use this for initialization
    void Start()
    {
        StartRenderer();
    }

    // Update is called once per frame
    void Update ()
    {
        rendererCS.SetTexture(kernelMain, "_densityTexture", DFMRef_.densityTexture_);
        rendererCS.SetTexture(kernelMain, "_colorTexture", DFMRef_.colorTexture_);
        appendVertexBuffer.SetCounterValue(0);

        rendererCS.SetFloat("_isoLevel", isoLevel);
        rendererCS.SetFloat("_time", Time.time);
        rendererCS.Dispatch(kernelMain, DFMRef_.Resolution / 8, DFMRef_.Resolution / 8, DFMRef_.Resolution / 8);

        //ComputeBuffer.CopyCount(appendVertexBuffer, argBuffer, 0);

        //MarchingCubesCS.SetBuffer(kernelMultiply, "_numVertices", argBuffer);
        //MarchingCubesCS.Dispatch(kernelMultiply, 1, 1, 1);

        //int[] args2 = new int[] { 0, 1, 0, 0 };
        //argBuffer.GetData(args2);
        //args2[0] *= 3;
        //argBuffer.SetData(args);

        //Debug.Log("Vertex count:" + args2[0]);

        ComputeBuffer.CopyCount(appendVertexBuffer, argBuffer, 0);

        //int[] args2 = new int[] { 0, 1, 0, 0, 0 };
        //argBuffer.GetData(args2);
        //Debug.Log("Index count:" + args2[0] + " Instances count:" + args2[1]);

        densityFieldRendererCS.SetBuffer(kernelInstances, "_numVertices", argBuffer);
        densityFieldRendererCS.Dispatch(kernelInstances, 1, 1, 1);

        mat.SetPass(0);
        mat.SetBuffer("triangles", appendVertexBuffer);
        mat.SetBuffer("indexStructure", argBuffer);
        mat.SetMatrix("_LocalToWorld", Matrix4x4.Translate(-transform.position) * transform.localToWorldMatrix);
        mat.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);

        if (renderDensityField)
        {
            Graphics.DrawMeshInstancedIndirect(
                  emptyMesh, 0, mat,
                  new Bounds(transform.position, transform.lossyScale * 1.5f),
                  argBuffer);
        }
    }

    private void OnDestroy()
    {
        DestroyRenderer();
    }
}
