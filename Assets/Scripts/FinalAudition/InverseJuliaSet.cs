using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InverseJuliaSet : MonoBehaviour
{
    public  Vector2         c;

    public  int             N;

    public  Material        material;
    [HideInInspector]
    public  Mesh            emptyMesh;

    public  ComputeShader   InverseJuliaSetCS;

    [HideInInspector]
    public ComputeBuffer    pointsInSetBuffer;
    [HideInInspector]
    public ComputeBuffer    randomBuffer;


    private const   int     meshVerticesSize_    =   65000;

    private int kernelMain_;

    private void
    Start()
    {
        kernelMain_     =   InverseJuliaSetCS.FindKernel("InverseJuliaSet");
    
        Vector2[] randomBufferV2 = new Vector2[meshVerticesSize_];

        for (   int i   =   0;
                i   <   65000;
                ++i)
        {
            Vector2 vector2;
            vector2.x   =   Random.Range(-1.0f, 1.0f);
            vector2.y   =   Random.Range(-1.0f, 1.0f);

            randomBufferV2[i] =   vector2;
        }

        pointsInSetBuffer   =   new ComputeBuffer(  meshVerticesSize_,
                                                    sizeof(float) * (3),
                                                    ComputeBufferType.Default);
        randomBuffer        =   new ComputeBuffer(  meshVerticesSize_,
                                                    sizeof(float) * (2),
                                                    ComputeBufferType.Default);
        randomBuffer.SetData(randomBufferV2);

        InverseJuliaSetCS.SetBuffer(kernelMain_,
                                    "pointsRW",
                                    pointsInSetBuffer);
        InverseJuliaSetCS.SetBuffer(kernelMain_,
                                    "randomizedPositions",
                                    randomBuffer);

        emptyMesh   =   new Mesh
        {
            name    =   "Empty Mesh"
        };

        Vector3[] positions = new Vector3[meshVerticesSize_];

        positions[0].x = -1.0f;
        positions[0].y = -1.0f;
        positions[0].z =  0.0f;

        positions[1].x = 1.0f;
        positions[1].y = 1.0f;
        positions[1].z = 0.0f;

        emptyMesh.vertices = positions;
        int[] indices = new int[meshVerticesSize_];

        for (int i = 0; i < meshVerticesSize_; i++)
            indices[i] = i;

        emptyMesh.SetIndices(indices, MeshTopology.Points, 0, false);
        emptyMesh.RecalculateBounds();
        emptyMesh.UploadMeshData(true);
	}
	
	private void
    Update()
    {
        InverseJuliaSetCS.SetFloat("_cx",   c.x);
        InverseJuliaSetCS.SetFloat("_cy",   c.y);
        InverseJuliaSetCS.SetInt("_N",      N);
        
        InverseJuliaSetCS.Dispatch( kernelMain_, 65000 / 64, 1, 1);
   
        material.SetPass(0);
        material.SetBuffer("points", pointsInSetBuffer);
        
        Graphics.DrawMesh(  emptyMesh,
                            transform.position,
                            transform.rotation,
                            material,
                            0);
	}

    private void OnDestroy()
    {
        if(pointsInSetBuffer != null)
        {
            pointsInSetBuffer.Release();
            pointsInSetBuffer = null;
        }
        if (randomBuffer != null)
        {
            randomBuffer.Release();
            randomBuffer = null;
        }
    }
}
