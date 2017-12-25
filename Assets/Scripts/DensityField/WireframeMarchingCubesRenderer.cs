using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WireframeMarchingCubesRenderer : DensityFieldRenderer {

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

    [Range(0f, 4f)]
    public float lineWidth = 0.5f;

    [Range(1,8)]
    public int downsampleRate = 1;

    private void Awake()
    {
        AwakeRenderer("WireframeMarchingCubes");
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
        {
            orientationVector = Camera.main.transform.forward;

            if( UnityEngine.XR.XRSettings.isDeviceActive)
                orientationVector = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head) * Vector3.forward;
        }

        rendererCS.SetVector("_orientationVector", orientationVector);

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

        Vector4 cameraFront = -Camera.main.transform.forward;

        if (UnityEngine.XR.XRSettings.isDeviceActive)
            cameraFront = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head) * -Vector3.forward;         //HACK
 
        mat.SetVector("_CameraFront", cameraFront);

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
