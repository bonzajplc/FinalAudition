using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MarchingSquaresRenderer : DensityFieldRenderer {

    public float isoLevel = 1.0f;
    public float cutout = 2.0f;

    public enum SampleOrientationType
    {
        X_Axis,
        Y_Axis,
        Z_Axis
    };
    public SampleOrientationType sampleOrientation = SampleOrientationType.Z_Axis;


    public Gradient gradientX;
    public Gradient gradientY;
    public Gradient gradientZ;

    Texture2D gradientXTexture;
    Texture2D gradientYTexture;
    Texture2D gradientZTexture;

    [Range(8, 256)]
    public int gradientWidth = 128;

    private void Awake()
    {
        AwakeRenderer("MarchingSquares");
    }

    // Use this for initialization
    void Start ()
    {
        StartRenderer();

        gradientXTexture = new Texture2D(gradientWidth, 1, TextureFormat.ARGB32, false);
        gradientYTexture = new Texture2D(gradientWidth, 1, TextureFormat.ARGB32, false);
        gradientZTexture = new Texture2D(gradientWidth, 1, TextureFormat.ARGB32, false);

        BakeGradients();

        gradientXTexture.filterMode = FilterMode.Point;
        gradientXTexture.wrapMode = TextureWrapMode.Clamp;

        gradientYTexture.filterMode = FilterMode.Point;
        gradientYTexture.wrapMode = TextureWrapMode.Clamp;

        gradientZTexture.filterMode = FilterMode.Point;
        gradientZTexture.wrapMode = TextureWrapMode.Clamp;
    }

    // Update is called once per frame
    void Update ()
    {
        rendererCS.SetTexture(kernelMain, "_gradientX", gradientXTexture);
        rendererCS.SetTexture(kernelMain, "_gradientY", gradientYTexture);
        rendererCS.SetTexture(kernelMain, "_gradientZ", gradientZTexture);

        rendererCS.SetTexture(kernelMain, "_densityTexture", DFMRef_.densityTexture_);
        rendererCS.SetTexture(kernelMain, "_colorTexture", DFMRef_.colorTexture_);
        appendVertexBuffer.SetCounterValue(0);

        rendererCS.SetFloat("_isoLevel", isoLevel);
        rendererCS.SetFloat("_cutout", cutout);

        Vector3 orientationVector = Vector3.zero;

        Vector3 axis0 = Vector3.zero;
        Vector3 axis2 = Vector3.zero;

        if (sampleOrientation == SampleOrientationType.X_Axis)
        {
            orientationVector = Vector3.right;

            axis0 = Vector3.forward;
            axis2 = Vector3.up;
        }
        else if (sampleOrientation == SampleOrientationType.Y_Axis)
        {
            orientationVector = Vector3.up;

            axis0 = Vector3.right;
            axis2 = Vector3.forward;
        }
        else if (sampleOrientation == SampleOrientationType.Z_Axis)
        {
            orientationVector = Vector3.back;

            axis0 = Vector3.right;
            axis2 = Vector3.up;
        }

        Vector3 axis1 = axis0 + axis2;
        rendererCS.SetVector("_orientationVector", orientationVector);
        rendererCS.SetVector("_sampleAxis0", axis0);
        rendererCS.SetVector("_sampleAxis1", axis1);
        rendererCS.SetVector("_sampleAxis2", axis2);

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

    private void BakeGradients()
    {
        //bake gradients to textures
        for (int i = 0; i < gradientWidth; i++)
            gradientXTexture.SetPixel(i, 0, gradientX.Evaluate((float)i / (float)gradientWidth));
        for (int i = 0; i < gradientWidth; i++)
            gradientYTexture.SetPixel(i, 0, gradientY.Evaluate((float)i / (float)gradientWidth));
        for (int i = 0; i < gradientWidth; i++)
            gradientZTexture.SetPixel(i, 0, gradientZ.Evaluate((float)i / (float)gradientWidth));

        gradientXTexture.Apply();
        gradientYTexture.Apply();
        gradientZTexture.Apply();
    }

    private void OnDestroy()
    {
        DestroyRenderer();
    }
}
