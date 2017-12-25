using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DifferentialSurface : MonoBehaviour {

    public Material mat;
    MaterialPropertyBlock matPropertyBlock = null;
    protected Mesh mesh;

    public enum DifferentialSurfaceType
    {
        Tunnel_1_1,
        Tunnel_1_2,
        Tunnel_2_1,
        Surface_1_1,
        Surface_2_1,
        Surface_2_2,
    }

    public DifferentialSurfaceType surfType;

    [Range(1,250)]
    public int subdivisionU = 40;
    [Range(1, 250)]
    public int subdivisionV = 40;

    public float minU = 0.0f;
    public float maxU = 2 * Mathf.PI;
    public float minV = 0.0f;
    public float maxV = Mathf.PI;

    public float Rotate_ = 0.0f;

    public Transform controller = null;

    Vector3[] vertices = null;
    Vector2[] texcoords = null;


    // Use this for initialization
    void Start()
    {
        mesh = new Mesh { name = "DifferentialSurface" };

        // Because we have to support older platforms (GLES2/3, DX9 etc) we can't do all of
        // this directly in the vertex shader using vertex ids :(

        vertices = new Vector3[subdivisionU * subdivisionV];
        texcoords = new Vector2[subdivisionU * subdivisionV];

        int i = 0;
        for (int u = 0; u < subdivisionU; u++)
        {
            for (int v = 0; v < subdivisionV; v++)
            {
                float dU = (float)u / (float)(subdivisionU - 1);
                float dV = (float)v / (float)(subdivisionV - 1);

                vertices[i].x = -500 + 1000 * dU;
                vertices[i].y = -500 + 1000 * (dU+dV);
                vertices[i].z = -500 + 1000 * dV;
                texcoords[i].x = dU;
                texcoords[i].y = dV;

                i++;
            }
        }

        int numIndices = (subdivisionU - 1) * (subdivisionV - 1) * 2 * 3;
        int[] indices = new int[numIndices];

        i = 0;

        for (int u = 0; u < subdivisionU - 1; u++)
        {
            for (int v = 0; v < subdivisionV - 1; v++)
            {
                indices[i++] = u * subdivisionV + v;
                indices[i++] = u * subdivisionV + v + 1;
                indices[i++] = (u + 1) * subdivisionV + v;

                indices[i++] = (u + 1) * subdivisionV + v;
                indices[i++] = u * subdivisionV + v + 1;
                indices[i++] = (u + 1) * subdivisionV + v + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = texcoords;

        mesh.MarkDynamic();

        mesh.SetIndices(indices, MeshTopology.Triangles, 0, false);
        mesh.UploadMeshData(true);

        matPropertyBlock = new MaterialPropertyBlock();
    }

    // Update is called once per frame
    void Update ()
    {
#if UNITY_EDITOR
        if (matPropertyBlock == null)
            return;
#endif

        matPropertyBlock.SetFloat("minU", minU);
        matPropertyBlock.SetFloat("minV", minV);
        matPropertyBlock.SetFloat("maxU", maxU);
        matPropertyBlock.SetFloat("maxV", maxV);
        matPropertyBlock.SetFloat("_Rotate", Rotate_);

        if (controller)
        {
            matPropertyBlock.SetFloat("param0", controller.position.x);
            matPropertyBlock.SetFloat("param1", controller.position.y);
        }
        else
        {
            matPropertyBlock.SetFloat("param0", 0.0f);
            matPropertyBlock.SetFloat("param1", 0.0f);
        }

        matPropertyBlock.SetFloat("surfType", (float)surfType);
        Graphics.DrawMesh(mesh, transform.localToWorldMatrix, mat, 0, null, 0, matPropertyBlock);
    }
}
