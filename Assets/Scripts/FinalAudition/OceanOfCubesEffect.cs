using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OceanOfCubesEffect : MonoBehaviour
{
    public Material mat;
    [HideInInspector]
    public Mesh emptyMesh;

    [Range(1,100)]
    public int CubesUResolution = 40;
    [Range(1, 100)]
    public int CubesVResolution = 40;
    public ComputeShader OceanOfCubesCS;

    public float CubeScale = 1.0f;
    public float minU = 0.0f;
    public float maxU = Mathf.PI;
    public float minV = 0.0f;
    public float maxV = Mathf.PI;

    public float timelineTime = 0.0f;

    int kernelMain;
    int kernelInstances;

    [HideInInspector]
    public ComputeBuffer appendVertexBuffer;
    [HideInInspector]
    public ComputeBuffer argBuffer;
    protected int[] args = new int[] { 0, 0, 0, 0, 0 };

    static private int maxEmptyMeshVertices = 64998;
    public int maxTriangles = 1024 * 1024;

    MaterialPropertyBlock matPropertyBlock = null;

    //colliders
    public GameObject[] Colliders;
    public float ColliderInfluence = 1.0f;

    public struct ColliderStruct
    {
        public Vector3 position;
        public float influence;
    };

    ColliderStruct[] collidersData = null;
    ComputeBuffer collidersBuffer = null;

    private void Awake()
    {
        kernelMain = OceanOfCubesCS.FindKernel( "OceanOfCubes" );

        kernelInstances = OceanOfCubesCS.FindKernel("CalculateInstances");

        matPropertyBlock = new MaterialPropertyBlock();
    }

    // Use this for initialization
    private void Start()
    {
        if (appendVertexBuffer != null)
        {
            appendVertexBuffer.Release();
            appendVertexBuffer = null;
        }
        if (argBuffer != null)
        {
            argBuffer.Release();
            argBuffer = null;
        }

        appendVertexBuffer = new ComputeBuffer(maxTriangles, sizeof(float) * (3 * (3 + 3 + 3)), ComputeBufferType.Append);
        argBuffer = new ComputeBuffer(args.Length, sizeof(int), ComputeBufferType.IndirectArguments);

        args[0] = maxEmptyMeshVertices; //num indices
        args[1] = 1; //instance count

        argBuffer.SetData(args);

        emptyMesh = new Mesh { name = "Empty Mesh" };

        // Because we have to support older platforms (GLES2/3, DX9 etc) we can't do all of
        // this directly in the vertex shader using vertex ids :(
        emptyMesh.vertices = new Vector3[maxEmptyMeshVertices];

        int[] indices = new int[maxEmptyMeshVertices];
        for (int i = 0; i < maxEmptyMeshVertices; i++)
            indices[i] = i;

        emptyMesh.SetIndices(indices, MeshTopology.Triangles, 0, false);
        emptyMesh.UploadMeshData(true);

        if (Colliders.Length > 0)
        {
            collidersData = new ColliderStruct[Colliders.Length];
            collidersBuffer = new ComputeBuffer(Colliders.Length, sizeof(float) * (3 + 1), ComputeBufferType.Default); //position + influence
        }
    }

    void Update()
    {
        appendVertexBuffer.SetCounterValue(0);

        OceanOfCubesCS.SetFloat("_gridSizeURcp", 1.0f / CubesUResolution);
        OceanOfCubesCS.SetFloat("_gridSizeVRcp", 1.0f / CubesVResolution);
        OceanOfCubesCS.SetFloat("_cubeScale", CubeScale);
        OceanOfCubesCS.SetFloat("_minU", minU);
        OceanOfCubesCS.SetFloat("_minV", minV);
        OceanOfCubesCS.SetFloat("_maxU", maxU);
        OceanOfCubesCS.SetFloat("_maxV", maxV);
        OceanOfCubesCS.SetFloat("_time", timelineTime);
        OceanOfCubesCS.SetBuffer(kernelMain, "triangleRW", appendVertexBuffer);

        if (Colliders.Length > 0)
        {

            int i = 0;
            foreach (var go in Colliders)
            {
                if (go.activeInHierarchy)
                {
                    collidersData[i].position = transform.worldToLocalMatrix.MultiplyPoint(go.transform.position);
                    collidersData[i].influence = 1.0f / ColliderInfluence;
                    i++;
                }
            }
            OceanOfCubesCS.SetInt("_numberOfColliders", i);

            collidersBuffer.SetData(collidersData);
            OceanOfCubesCS.SetBuffer(kernelMain, "colliders", collidersBuffer);
        }
        else
            OceanOfCubesCS.SetInt("_numberOfColliders", 0);

        OceanOfCubesCS.Dispatch(kernelMain, CubesUResolution/8, CubesVResolution/8, 1);

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

        OceanOfCubesCS.SetBuffer(kernelInstances, "_numVertices", argBuffer);
        OceanOfCubesCS.Dispatch(kernelInstances, 1, 1, 1);

        mat.SetPass(0);
        matPropertyBlock.SetBuffer("triangles", appendVertexBuffer);
        matPropertyBlock.SetBuffer("indexStructure", argBuffer);
        matPropertyBlock.SetMatrix("_LocalToWorld", Matrix4x4.Translate(-transform.position) * transform.localToWorldMatrix);
        matPropertyBlock.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);

        Graphics.DrawMeshInstancedIndirect(
              emptyMesh, 0, mat,
              new Bounds(transform.position, transform.lossyScale * 10.0f),
              argBuffer, 0, matPropertyBlock);
    }

    private void OnDestroy()
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
