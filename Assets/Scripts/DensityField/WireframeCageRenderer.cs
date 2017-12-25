using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WireframeCageRenderer : DensityFieldRenderer {

    public float isoLevel = 1.0f;
    public enum LineOrientationType
    {
        X_Axis,
        Minus_X_Axis,
        Y_Axis,
        Minus_Y_Axis,
        Z_Axis,
        Minus_Z_Axis,
        Camera
    };
    public LineOrientationType lineOrientation = LineOrientationType.Camera;

    public enum SampleOrientationType
    {
        X_Axis,
        Y_Axis,
        Z_Axis,
        Camera
    };
    public SampleOrientationType sampleOrientation = SampleOrientationType.Z_Axis;

    [Range(0f, 4f)]
    public float lineWidth = 0.5f;

    [Range(1,8)]
    public int downsampleRate = 1;

    private void Awake()
    {
        AwakeRenderer("WireframeCage");
    }

    // Use this for initialization
    void Start()
    {
        StartRenderer();
    }

    // Update is called once per frame
    void Update()
    {
        rendererCS.SetTexture(kernelMain, "_densityTexture", DFMRef_.densityTexture_);
        rendererCS.SetTexture(kernelMain, "_colorTexture", DFMRef_.colorTexture_);
        appendVertexBuffer.SetCounterValue(0);

        rendererCS.SetFloat("_isoLevel", isoLevel);
        rendererCS.SetFloat("_lineWidth", lineWidth);
        Vector4 orientationVector = Vector4.zero;

        if (lineOrientation == LineOrientationType.X_Axis)
            orientationVector.x = -1.0f;
        else if (lineOrientation == LineOrientationType.Minus_X_Axis)
            orientationVector.x = 1.0f;
        else if (lineOrientation == LineOrientationType.Y_Axis)
            orientationVector.y = -1.0f;
        else if (lineOrientation == LineOrientationType.Minus_Y_Axis)
            orientationVector.y = 1.0f;
        else if (lineOrientation == LineOrientationType.Z_Axis)
            orientationVector.z = -1.0f;
        else if (lineOrientation == LineOrientationType.Minus_Z_Axis)
            orientationVector.z = 1.0f;
        else if (lineOrientation == LineOrientationType.Camera)
            orientationVector = Camera.main.transform.forward;

        rendererCS.SetVector("_orientationVector", orientationVector);

        Vector3 axis0 = Vector3.zero;
        Vector3 axis2 = Vector3.zero;
//        axis0.x = 1.0f;
//        axis2.y = 1.0f;

        if (sampleOrientation == SampleOrientationType.X_Axis)
        {
            axis0 = Vector3.forward;
            axis2 = Vector3.up;
        }
        else if (sampleOrientation == SampleOrientationType.Y_Axis)
        {
            axis0 = Vector3.right;
            axis2 = Vector3.forward;
        }
        else if (sampleOrientation == SampleOrientationType.Z_Axis)
        {
            axis0 = Vector3.right;
            axis2 = Vector3.up;
        }
        else if (sampleOrientation == SampleOrientationType.Camera)
        {
            axis0 = Camera.main.transform.right;
            axis2 = Camera.main.transform.up;
        }

        Vector3 axis1 = axis0 + axis2;
        rendererCS.SetVector("_sampleAxis0", axis0);
        rendererCS.SetVector("_sampleAxis1", axis1);
        rendererCS.SetVector("_sampleAxis2", axis2);

        int resolution = Mathf.Max( 8, DFMRef_.Resolution / downsampleRate );
        rendererCS.SetFloat("gridSizeRcp_", 1.0f / (resolution - 1.0f));
        rendererCS.SetInt("_downsampleRate", downsampleRate);

        rendererCS.Dispatch(kernelMain, resolution / 8, resolution / 8, resolution / 8);

        ComputeBuffer.CopyCount(appendVertexBuffer, argBuffer, 0);

        //int[] args2 = new int[] { 0, 1, 0, 0, 0 };
        //argBuffer.GetData(args2);
        //Debug.Log("Index count:" + args2[0] + "Instances count:" + args2[1]);

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
