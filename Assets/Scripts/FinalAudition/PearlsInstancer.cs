using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PearlsInstancer : MonoBehaviour {

    public Mesh inMesh = null;
    public Transform inMeshTransform = null;

    public bool drawBounds = true;
    public Bounds meshBounds = new Bounds(Vector3.zero, Vector3.one);

    ComputeBuffer inMeshVertexBuffer = null;
    ComputeBuffer inMeshNormalBuffer = null;

    public Mesh Lod_0_mesh;
    public float Lod_0_distance;
    ComputeBuffer Lod_0_positionBuffer = null;
    ComputeBuffer Lod_0_instancesArgBuffer = null;

    public Mesh Lod_1_mesh;
    public float Lod_1_distance;
    ComputeBuffer Lod_1_positionBuffer = null;
    ComputeBuffer Lod_1_instancesArgBuffer = null;

    public Mesh Lod_2_mesh;
    public float Lod_2_distance;
    ComputeBuffer Lod_2_positionBuffer = null;
    ComputeBuffer Lod_2_instancesArgBuffer = null;

    public Mesh Lod_3_mesh;
    public float Lod_3_distance;
    ComputeBuffer Lod_3_positionBuffer = null;
    ComputeBuffer Lod_3_instancesArgBuffer = null;

    public Mesh Lod_4_mesh;
    public float Lod_4_distance;
    ComputeBuffer Lod_4_positionBuffer = null;
    ComputeBuffer Lod_4_instancesArgBuffer = null;

    public Material mat;

    public ComputeShader pearlsInstancerCS;

    public Cubemap envTexture;

    int[] args = new int[] { 0, 1, 0, 0, 0 };

    [Range(1, 100)]
    public int PearlsUResolution = 40;
    [Range(1, 100)]
    public int PearlsVResolution = 40;

    [Range(0.0f, 2.0f)]
    public float PearlScale = 1.0f;
    public float NormalOffset = 0.0f;
    public float MinU = 0.0f;
    public float MaxU = Mathf.PI;
    public float MinV = 0.0f;
    public float MaxV = Mathf.PI;

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

    public float timelineTime = 0.0f;

    int maxInstancesPerLOD;

    int kernelMain;
    int kernelMainForMesh;
    float[] floatMatrix = new float[16];

    MaterialPropertyBlock matPropertyBlock_lod0 = null;
    MaterialPropertyBlock matPropertyBlock_lod1 = null;
    MaterialPropertyBlock matPropertyBlock_lod2 = null;
    MaterialPropertyBlock matPropertyBlock_lod3 = null;
    MaterialPropertyBlock matPropertyBlock_lod4 = null;

    private Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

    // Use this for initialization
    void Start()
    {
#if UNITY_EDITOR
        OnDestroy();
#endif
        kernelMain = pearlsInstancerCS.FindKernel("CalculatePearlsInstances");
        kernelMainForMesh = pearlsInstancerCS.FindKernel("CalculatePearlsInstancesForMesh");

        if (inMesh)
        {
            inMeshVertexBuffer = new ComputeBuffer(inMesh.vertexCount, sizeof(float) * 3, ComputeBufferType.Default); //position
            inMeshVertexBuffer.SetData(inMesh.vertices);
            inMeshNormalBuffer = new ComputeBuffer(inMesh.vertexCount, sizeof(float) * 3, ComputeBufferType.Default); //position
            inMeshNormalBuffer.SetData(inMesh.normals);
            pearlsInstancerCS.SetBuffer(kernelMainForMesh, "meshPositions", inMeshVertexBuffer);
            pearlsInstancerCS.SetBuffer(kernelMainForMesh, "meshNormals", inMeshNormalBuffer);
        }

        matPropertyBlock_lod0 = new MaterialPropertyBlock();
        matPropertyBlock_lod1 = new MaterialPropertyBlock();
        matPropertyBlock_lod2 = new MaterialPropertyBlock();
        matPropertyBlock_lod3 = new MaterialPropertyBlock();
        matPropertyBlock_lod4 = new MaterialPropertyBlock();

        maxInstancesPerLOD = PearlsUResolution * PearlsVResolution;

        if (inMesh)
            maxInstancesPerLOD = inMesh.vertexCount;

        Lod_0_positionBuffer = new ComputeBuffer(maxInstancesPerLOD, sizeof(float) * (3 + 1 + 3), ComputeBufferType.Append); //position + scale + color
        Lod_0_instancesArgBuffer = new ComputeBuffer(args.Length, sizeof(int), ComputeBufferType.IndirectArguments);

        args[0] = (Lod_0_mesh != null) ? (int)Lod_0_mesh.GetIndexCount(0) : 0;
        Lod_0_instancesArgBuffer.SetData(args);

        Lod_1_positionBuffer = new ComputeBuffer(maxInstancesPerLOD, sizeof(float) * (3 + 1 + 3), ComputeBufferType.Append); //position + scale + color
        Lod_1_instancesArgBuffer = new ComputeBuffer(args.Length, sizeof(int), ComputeBufferType.IndirectArguments);

        args[0] = (Lod_1_mesh != null) ? (int)Lod_1_mesh.GetIndexCount(0) : 0;
        Lod_1_instancesArgBuffer.SetData(args);

        Lod_2_positionBuffer = new ComputeBuffer(maxInstancesPerLOD, sizeof(float) * (3 + 1 + 3), ComputeBufferType.Append); //position + scale + color
        Lod_2_instancesArgBuffer = new ComputeBuffer(args.Length, sizeof(int), ComputeBufferType.IndirectArguments);

        args[0] = (Lod_2_mesh != null) ? (int)Lod_2_mesh.GetIndexCount(0) : 0;
        Lod_2_instancesArgBuffer.SetData(args);

        Lod_3_positionBuffer = new ComputeBuffer(maxInstancesPerLOD, sizeof(float) * (3 + 1 + 3), ComputeBufferType.Append); //position + scale + color
        Lod_3_instancesArgBuffer = new ComputeBuffer(args.Length, sizeof(int), ComputeBufferType.IndirectArguments);

        args[0] = (Lod_3_mesh != null) ? (int)Lod_3_mesh.GetIndexCount(0) : 0;
        Lod_3_instancesArgBuffer.SetData(args);

        Lod_4_positionBuffer = new ComputeBuffer(maxInstancesPerLOD, sizeof(float) * (3 + 1 + 3), ComputeBufferType.Append); //position + scale + color
        Lod_4_instancesArgBuffer = new ComputeBuffer(args.Length, sizeof(int), ComputeBufferType.IndirectArguments);

        args[0] = (Lod_4_mesh != null) ? (int)Lod_4_mesh.GetIndexCount(0) : 0;
        Lod_4_instancesArgBuffer.SetData(args);

        if (Colliders.Length > 0)
        {
            collidersData = new ColliderStruct[Colliders.Length];
            collidersBuffer = new ComputeBuffer(Colliders.Length, sizeof(float) * (3 + 1), ComputeBufferType.Default); //position + influence
        }

        pearlsInstancerCS.SetTexture(kernelMain, "_envMap", envTexture);
        pearlsInstancerCS.SetTexture(kernelMainForMesh, "_envMap", envTexture);
    }

    // Update is called once per frame
    void Update()
    {
        Lod_0_positionBuffer.SetCounterValue(0);
        Lod_1_positionBuffer.SetCounterValue(0);
        Lod_2_positionBuffer.SetCounterValue(0);
        Lod_3_positionBuffer.SetCounterValue(0);
        Lod_4_positionBuffer.SetCounterValue(0);

        pearlsInstancerCS.SetFloat("_gridSizeURcp", 1.0f / PearlsUResolution);
        pearlsInstancerCS.SetFloat("_gridSizeVRcp", 1.0f / PearlsVResolution);
        pearlsInstancerCS.SetFloat("_pearlScale", PearlScale);
        pearlsInstancerCS.SetFloat("_normalOffset", NormalOffset);
        pearlsInstancerCS.SetFloat("_minU", MinU);
        pearlsInstancerCS.SetFloat("_minV", MinV);
        pearlsInstancerCS.SetFloat("_maxU", MaxU);
        pearlsInstancerCS.SetFloat("_maxV", MaxV);
        pearlsInstancerCS.SetFloat("_time", timelineTime);

        pearlsInstancerCS.SetFloat("_Lod_0_distance", Lod_0_distance);
        pearlsInstancerCS.SetFloat("_Lod_1_distance", Lod_1_distance);
        pearlsInstancerCS.SetFloat("_Lod_2_distance", Lod_2_distance);
        pearlsInstancerCS.SetFloat("_Lod_3_distance", Lod_3_distance);
        pearlsInstancerCS.SetFloat("_Lod_4_distance", Lod_4_distance);

        pearlsInstancerCS.SetVector("_cameraWorldPos", Camera.main.transform.position);

        Matrix4x4 matrix = transform.localToWorldMatrix;

        floatMatrix[0] = matrix[0, 0];
        floatMatrix[1] = matrix[1, 0];
        floatMatrix[2] = matrix[2, 0];
        floatMatrix[3] = matrix[3, 0];

        floatMatrix[4] = matrix[0, 1];
        floatMatrix[5] = matrix[1, 1];
        floatMatrix[6] = matrix[2, 1];
        floatMatrix[7] = matrix[3, 1];

        floatMatrix[8] = matrix[0, 2];
        floatMatrix[9] = matrix[1, 2];
        floatMatrix[10] = matrix[2, 2];
        floatMatrix[11] = matrix[3, 2];

        floatMatrix[12] = matrix[0, 3];
        floatMatrix[13] = matrix[1, 3];
        floatMatrix[14] = matrix[2, 3];
        floatMatrix[15] = matrix[3, 3];

        pearlsInstancerCS.SetFloats("_instancerMatrix", floatMatrix);

        if (inMeshTransform)
            matrix = inMeshTransform.localToWorldMatrix;
        else
            matrix = Matrix4x4.identity;

        floatMatrix[0] = matrix[0, 0];
        floatMatrix[1] = matrix[1, 0];
        floatMatrix[2] = matrix[2, 0];
        floatMatrix[3] = matrix[3, 0];

        floatMatrix[4] = matrix[0, 1];
        floatMatrix[5] = matrix[1, 1];
        floatMatrix[6] = matrix[2, 1];
        floatMatrix[7] = matrix[3, 1];

        floatMatrix[8] = matrix[0, 2];
        floatMatrix[9] = matrix[1, 2];
        floatMatrix[10] = matrix[2, 2];
        floatMatrix[11] = matrix[3, 2];

        floatMatrix[12] = matrix[0, 3];
        floatMatrix[13] = matrix[1, 3];
        floatMatrix[14] = matrix[2, 3];
        floatMatrix[15] = matrix[3, 3];

        pearlsInstancerCS.SetFloats("_meshMatrix", floatMatrix);

        if (inMesh)
        {
            pearlsInstancerCS.SetBuffer(kernelMainForMesh, "Lod_0_positions", Lod_0_positionBuffer);
            pearlsInstancerCS.SetBuffer(kernelMainForMesh, "Lod_1_positions", Lod_1_positionBuffer);
            pearlsInstancerCS.SetBuffer(kernelMainForMesh, "Lod_2_positions", Lod_2_positionBuffer);
            pearlsInstancerCS.SetBuffer(kernelMainForMesh, "Lod_3_positions", Lod_3_positionBuffer);
            pearlsInstancerCS.SetBuffer(kernelMainForMesh, "Lod_4_positions", Lod_4_positionBuffer);
        }
        else
        {
            pearlsInstancerCS.SetBuffer(kernelMain, "Lod_0_positions", Lod_0_positionBuffer);
            pearlsInstancerCS.SetBuffer(kernelMain, "Lod_1_positions", Lod_1_positionBuffer);
            pearlsInstancerCS.SetBuffer(kernelMain, "Lod_2_positions", Lod_2_positionBuffer);
            pearlsInstancerCS.SetBuffer(kernelMain, "Lod_3_positions", Lod_3_positionBuffer);
            pearlsInstancerCS.SetBuffer(kernelMain, "Lod_4_positions", Lod_4_positionBuffer);
        }

        if (Colliders.Length > 0)
        {

            int i = 0;
            foreach( var go in Colliders )
            {
                if( go.activeInHierarchy )
                {
                    collidersData[i].position = transform.worldToLocalMatrix.MultiplyPoint( go.transform.position );
                    collidersData[i].influence = 1.0f/ColliderInfluence;
                    i++;
                }
            }
            pearlsInstancerCS.SetInt("_numberOfColliders", i);

            collidersBuffer.SetData(collidersData);
            if (inMesh)
                pearlsInstancerCS.SetBuffer(kernelMainForMesh, "colliders", collidersBuffer);
            else
                pearlsInstancerCS.SetBuffer(kernelMain, "colliders", collidersBuffer);
        }
        else
            pearlsInstancerCS.SetInt("_numberOfColliders", 0);


        if (inMesh)
            pearlsInstancerCS.Dispatch(kernelMainForMesh, inMesh.vertexCount / 64, 1, 1);
        else
            pearlsInstancerCS.Dispatch(kernelMain, PearlsUResolution / 8, PearlsVResolution / 8, 1);

        ComputeBuffer.CopyCount(Lod_0_positionBuffer, Lod_0_instancesArgBuffer, 4);
        ComputeBuffer.CopyCount(Lod_1_positionBuffer, Lod_1_instancesArgBuffer, 4);
        ComputeBuffer.CopyCount(Lod_2_positionBuffer, Lod_2_instancesArgBuffer, 4);
        ComputeBuffer.CopyCount(Lod_3_positionBuffer, Lod_3_instancesArgBuffer, 4);
        ComputeBuffer.CopyCount(Lod_4_positionBuffer, Lod_4_instancesArgBuffer, 4);

        /*        int[] args2 = new int[] { 0, 1, 0, 0, 0 };
                Lod_0_instancesArgBuffer.GetData(args2);
                Debug.Log("Lod_0_instancesArgBuffer Indices count:" + args2[0] + " Indstances count:" + args2[1]);
                Lod_1_instancesArgBuffer.GetData(args2);
                Debug.Log("Lod_1_instancesArgBuffer Indices count:" + args2[0] + " Indstances count:" + args2[1]);
                Lod_2_instancesArgBuffer.GetData(args2);
                Debug.Log("Lod_2_instancesArgBuffer Indices count:" + args2[0] + " Indstances count:" + args2[1]);
                Lod_3_instancesArgBuffer.GetData(args2);
                Debug.Log("Lod_3_instancesArgBuffer Indices count:" + args2[0] + " Indstances count:" + args2[1]);
                Lod_4_instancesArgBuffer.GetData(args2);
                Debug.Log("Lod_4_instancesArgBuffer Indices count:" + args2[0] + " Indstances count:" + args2[1]);
        */
        mat.SetPass(0);

        matPropertyBlock_lod0.SetBuffer("positions", Lod_0_positionBuffer);
        matPropertyBlock_lod1.SetBuffer("positions", Lod_1_positionBuffer);
        matPropertyBlock_lod2.SetBuffer("positions", Lod_2_positionBuffer);
        matPropertyBlock_lod3.SetBuffer("positions", Lod_3_positionBuffer);
        matPropertyBlock_lod4.SetBuffer("positions", Lod_4_positionBuffer);

        if (inMesh)
        {
            meshBounds = inMesh.bounds;
            if (inMeshTransform)
                meshBounds.center = inMeshTransform.position;

            Matrix4x4 transformedMatrix = Matrix4x4.Translate(-meshBounds.center) * Matrix4x4.Inverse(transform.localToWorldMatrix) * transform.localToWorldMatrix;

            matPropertyBlock_lod0.SetMatrix("_LocalToWorld", transformedMatrix);
            matPropertyBlock_lod1.SetMatrix("_LocalToWorld", transformedMatrix);
            matPropertyBlock_lod2.SetMatrix("_LocalToWorld", transformedMatrix);
            matPropertyBlock_lod3.SetMatrix("_LocalToWorld", transformedMatrix);
            matPropertyBlock_lod4.SetMatrix("_LocalToWorld", transformedMatrix);
        }
        else
        {
            Matrix4x4 transformedMatrix = Matrix4x4.Translate( -meshBounds.center - transform.position) * transform.localToWorldMatrix;

            matPropertyBlock_lod0.SetMatrix("_LocalToWorld", transformedMatrix);
            matPropertyBlock_lod1.SetMatrix("_LocalToWorld", transformedMatrix);
            matPropertyBlock_lod2.SetMatrix("_LocalToWorld", transformedMatrix);
            matPropertyBlock_lod3.SetMatrix("_LocalToWorld", transformedMatrix);
            matPropertyBlock_lod4.SetMatrix("_LocalToWorld", transformedMatrix);
        }

        bounds.size = meshBounds.size;
        bounds.center = transform.position + meshBounds.center;

        Graphics.DrawMeshInstancedIndirect(
              Lod_0_mesh, 0, mat,
              bounds,
              Lod_0_instancesArgBuffer, 0, matPropertyBlock_lod0);

        Graphics.DrawMeshInstancedIndirect(
              Lod_1_mesh, 0, mat,
              bounds,
              Lod_1_instancesArgBuffer, 0, matPropertyBlock_lod1);

        Graphics.DrawMeshInstancedIndirect(
              Lod_2_mesh, 0, mat,
              bounds,
              Lod_2_instancesArgBuffer, 0, matPropertyBlock_lod2);

        Graphics.DrawMeshInstancedIndirect(
              Lod_3_mesh, 0, mat,
              bounds,
              Lod_3_instancesArgBuffer, 0, matPropertyBlock_lod3);

        Graphics.DrawMeshInstancedIndirect(
              Lod_4_mesh, 0, mat,
              bounds,
              Lod_4_instancesArgBuffer, 0, matPropertyBlock_lod4);
    }

    private void OnDestroy()
    {
        if (Lod_0_positionBuffer != null)
        {
            Lod_0_positionBuffer.Release();
            Lod_0_positionBuffer = null;
        }
        if (Lod_1_positionBuffer != null)
        {
            Lod_1_positionBuffer.Release();
            Lod_1_positionBuffer = null;
        }
        if (Lod_2_positionBuffer != null)
        {
            Lod_2_positionBuffer.Release();
            Lod_2_positionBuffer = null;
        }
        if (Lod_3_positionBuffer != null)
        {
            Lod_3_positionBuffer.Release();
            Lod_3_positionBuffer = null;
        }
        if (Lod_4_positionBuffer != null)
        {
            Lod_4_positionBuffer.Release();
            Lod_4_positionBuffer = null;
        }

        if (Lod_0_instancesArgBuffer != null)
        {
            Lod_0_instancesArgBuffer.Release();
            Lod_0_instancesArgBuffer = null;
        }
        if (Lod_1_instancesArgBuffer != null)
        {
            Lod_1_instancesArgBuffer.Release();
            Lod_1_instancesArgBuffer = null;
        }
        if (Lod_2_instancesArgBuffer != null)
        {
            Lod_2_instancesArgBuffer.Release();
            Lod_2_instancesArgBuffer = null;
        }
        if (Lod_3_instancesArgBuffer != null)
        {
            Lod_3_instancesArgBuffer.Release();
            Lod_3_instancesArgBuffer = null;
        }
        if (Lod_4_instancesArgBuffer != null)
        {
            Lod_4_instancesArgBuffer.Release();
            Lod_4_instancesArgBuffer = null;
        }

        if (inMeshVertexBuffer != null)
        {
            inMeshVertexBuffer.Release();
            inMeshVertexBuffer = null;
        }

        if (inMeshNormalBuffer != null)
        {
            inMeshNormalBuffer.Release();
            inMeshNormalBuffer = null;
        }

        if (collidersBuffer != null)
        {
            collidersBuffer.Release();
            collidersBuffer = null;
        }
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if( drawBounds )
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
#endif
}
