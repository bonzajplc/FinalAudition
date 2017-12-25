using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HelixInstancer : MonoBehaviour {

    [Range(1,16)]
    public int numIterations = 4;
    public Material mat = null;
    public Mesh mesh = null;

    public Vector3 translation = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    [Range(0, 1)]
    public float scale = 1.0f;
    Vector3 scaleVector = Vector3.one;

    [Range(0,1360)]
    public float treeRotation = 0;
    // Use this for initialization

    Matrix4x4[] matrixArray = null;
    int arrayPointer = 0;

    void Start ()
    {
        int q = 2;

        int sumOfElements = 1 * (1 - (int)Mathf.Pow(q, numIterations)) / (1 - q);

        matrixArray = new Matrix4x4[sumOfElements];
    }

    void BuildHelix( int iteration, Matrix4x4 prevMatrix, Matrix4x4 prevRightMatrix)
    {
        if (iteration == numIterations)
            return;


        Quaternion curRotation = Quaternion.AngleAxis(rotation.x, Vector3.right) * Quaternion.AngleAxis(treeRotation, Vector3.up);
        Matrix4x4 matrix = prevMatrix * Matrix4x4.Scale(scaleVector) * Matrix4x4.Translate(translation) * Matrix4x4.Rotate(curRotation);

        matrixArray[arrayPointer++] = matrix;
        //Graphics.DrawMesh(mesh, matrix, mat, 0);

        BuildHelix(iteration + 1, matrix, matrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180, Vector3.up)));

        //right
        curRotation = Quaternion.AngleAxis(rotation.x, Vector3.right) * Quaternion.AngleAxis(treeRotation + 180, Vector3.up);
        Matrix4x4 matrixRight = prevRightMatrix * Matrix4x4.Scale(scaleVector) * Matrix4x4.Translate(translation) * Matrix4x4.Rotate(curRotation);

        matrixArray[arrayPointer++] = matrixRight;
        //Graphics.DrawMesh(mesh, matrixRight, mat, 0);

        //left
        BuildHelix(iteration + 1, matrixRight, matrixRight * Matrix4x4.Rotate(Quaternion.AngleAxis(180, Vector3.up)));
    }

    // Update is called once per frame
    void Update ()
    {
        scaleVector.x = scale;
        scaleVector.y = scale;
        scaleVector.z = scale;

        Matrix4x4 matrix = transform.localToWorldMatrix;
        matrix = matrix * Matrix4x4.Rotate(Quaternion.AngleAxis(treeRotation, matrix.GetRow(1)));

        arrayPointer = 0;

        if ( mesh != null && mat != null )
        {
            matrixArray[arrayPointer++] = matrix;
            //Graphics.DrawMesh(mesh, matrix, mat, 0);

            BuildHelix(1, matrix, matrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180, matrix.GetRow(1))));
        }

        Graphics.DrawMeshInstanced(mesh, 0, mat, matrixArray, Mathf.Min(1023,matrixArray.Length));
    }
}
