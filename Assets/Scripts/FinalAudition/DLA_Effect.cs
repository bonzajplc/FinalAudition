using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode]
public class DLA_Effect : MonoBehaviour {

    public Material mat;
    [HideInInspector]
    public Mesh emptyMesh;

    public string Precomputed_DLA_Filename = "";

    public float maxRadius = 1.0f;
    public float minRadius = 0.2f;

    public float particleCount = 1000;
    [Range(0.0f,1.0f)]
    public float CubeScale = 0.1f;

    public ComputeShader DLA_Effect_CS = null;
    ComputeBuffer DLA_ParticlesBuffer = null;

    int kernelMain;
    int kernelInstances;

    ComputeBuffer appendVertexBuffer = null;
    ComputeBuffer argBuffer = null;
    int[] args = new int[] { 0, 0, 0, 0, 0 };

    int maxEmptyMeshVertices = 64998;
    int maxTriangles = 1024 * 1024;
    
    // Use this for initialization
    void Start ()
    {
        TextAsset DLA_binary = Resources.Load(Precomputed_DLA_Filename) as TextAsset;
        Stream s = new MemoryStream(DLA_binary.bytes);
        BinaryReader br = new BinaryReader(s);

        byte[] DLA_Data = br.ReadBytes((int)s.Length);
        float[] DLA_Particles = new float[s.Length/4];

        Buffer.BlockCopy(DLA_Data, 0, DLA_Particles, 0, DLA_Data.Length);

        DLA_ParticlesBuffer = new ComputeBuffer(DLA_Particles.Length / 3, 3 * sizeof(float));  // positions
        DLA_ParticlesBuffer.SetData(DLA_Particles);
        DLA_Effect_CS.SetBuffer(kernelMain, "DLAParticles", DLA_ParticlesBuffer);

        kernelMain = DLA_Effect_CS.FindKernel("DLA_Compute");
        kernelInstances = DLA_Effect_CS.FindKernel("CalculateInstances");

        appendVertexBuffer = new ComputeBuffer(maxTriangles, sizeof(float) * (3 * (3 + 3 + 3)), ComputeBufferType.Append);
        argBuffer = new ComputeBuffer(args.Length, sizeof(int), ComputeBufferType.IndirectArguments);

        args[0] = maxEmptyMeshVertices; //num indices
        args[1] = 1; //instance count

        argBuffer.SetData(args);

        DLA_Effect_CS.SetBuffer(kernelMain, "triangleRW", appendVertexBuffer);

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

    // Update is called once per frame
    void Update ()
    {
        appendVertexBuffer.SetCounterValue(0);

        DLA_Effect_CS.SetFloat("_cubeScale", CubeScale);
        DLA_Effect_CS.SetFloat("_minRadius", minRadius);
        DLA_Effect_CS.SetFloat("_maxRadius", maxRadius);
        
        DLA_Effect_CS.Dispatch(kernelMain, (int)particleCount / 64, 1, 1);
  
        ComputeBuffer.CopyCount(appendVertexBuffer, argBuffer, 0);

        //int[] args2 = new int[] { 0, 1, 0, 0, 0 };
        //argBuffer.GetData(args2);
        //Debug.Log("Index count:" + args2[0] + " Instances count:" + args2[1]);

        DLA_Effect_CS.SetBuffer(kernelInstances, "_numVertices", argBuffer);
        DLA_Effect_CS.Dispatch(kernelInstances, 1, 1, 1);

        mat.SetPass(0);
        mat.SetBuffer("triangles", appendVertexBuffer);
        mat.SetBuffer("indexStructure", argBuffer);
        mat.SetMatrix("_LocalToWorld", Matrix4x4.Translate(-transform.position) * transform.localToWorldMatrix);
        mat.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);

        Graphics.DrawMeshInstancedIndirect(
              emptyMesh, 0, mat,
              new Bounds(transform.position, transform.lossyScale * 1.5f),
              argBuffer);
    }

    void OnDestroy()
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
        if (DLA_ParticlesBuffer != null)
        {
            DLA_ParticlesBuffer.Release();
            DLA_ParticlesBuffer = null;
        }
    }
}
