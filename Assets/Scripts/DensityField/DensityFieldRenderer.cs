using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DensityFieldRenderer : MonoBehaviour
{
    public Material mat;
    [HideInInspector]
    public Mesh emptyMesh;

    public bool renderDensityField = true;

    public ComputeShader rendererCS;
    public ComputeShader densityFieldRendererCS;

    [HideInInspector]
    protected DensityFieldManager DFMRef_ = null;

    protected int kernelMain;
    protected int kernelInstances;

    [HideInInspector]
    public ComputeBuffer appendVertexBuffer;
    [HideInInspector]
    public ComputeBuffer argBuffer;
    protected int[] args = new int[] { 0, 0, 0, 0, 0 };

    static private int maxEmptyMeshVertices = 64998;
    public int maxTriangles = 1024 * 1024;

    protected void AwakeRenderer( string rendereKernelName )
    {
        kernelMain = rendererCS.FindKernel( rendereKernelName );

        kernelInstances = densityFieldRendererCS.FindKernel("CalculateInstances");
    }

    // Use this for initialization
    protected void StartRenderer ()
    {
#if UNITY_EDITOR
        DestroyRenderer();
#endif
        DFMRef_ = GetComponent<DensityFieldManager>();

        appendVertexBuffer = new ComputeBuffer(maxTriangles, sizeof(float) * (3 * (3 + 3 + 3)), ComputeBufferType.Append);
        argBuffer = new ComputeBuffer(args.Length, sizeof(int), ComputeBufferType.IndirectArguments);

        args[0] = maxEmptyMeshVertices; //num indices
        args[1] = 1; //instance count

        argBuffer.SetData(args);

        rendererCS.SetFloat("_gridSizeRcp", 1.0f/DFMRef_.Resolution );

        rendererCS.SetBuffer(kernelMain, "triangleRW", appendVertexBuffer);

        emptyMesh = new Mesh { name = "Empty Mesh" };

        // Because we have to support older platforms (GLES2/3, DX9 etc) we can't do all of
        // this directly in the vertex shader using vertex ids :(
        emptyMesh.vertices = new Vector3[maxEmptyMeshVertices];

        int[] indices = new int[maxEmptyMeshVertices];
        for (int i = 0; i < maxEmptyMeshVertices; i++)
            indices[i] = i;

        emptyMesh.SetIndices(indices, MeshTopology.Triangles, 0, false);
        emptyMesh.UploadMeshData(true);
    }

    protected void DestroyRenderer()
    {
        if(appendVertexBuffer != null)
        {
            appendVertexBuffer.Release();
            appendVertexBuffer = null;
        }
        if( argBuffer != null )
        {
            argBuffer.Release();
            argBuffer = null;
        }
    }
}
