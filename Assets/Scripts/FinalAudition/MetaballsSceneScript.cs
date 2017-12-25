using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaballsSceneScript : MonoBehaviour {

    GameObject[] cubes = null;
    Quaternion[] cubeRotations = null;
    public float rotationScale = 1.0f;

    GameObject[] spheres = null;
    Vector3[] sphereBounces = null;
    Vector3[] spherePositions = null;
    public float bounceScale = 1.0f;

    // Use this for initialization
    void Start () {
        //find all objects marked ass cube
        cubes = GameObject.FindGameObjectsWithTag("Cube");
        cubeRotations = new Quaternion[cubes.Length];

        for (int i = 0; i < cubes.Length; i++)
        {
            cubeRotations[i] = Quaternion.AngleAxis(rotationScale * Random.Range(-1.0f, 1.0f), Random.onUnitSphere);
        }

        spheres = GameObject.FindGameObjectsWithTag("Sphere");
        sphereBounces = new Vector3[spheres.Length];
        spherePositions = new Vector3[spheres.Length];

        for (int i = 0; i < spheres.Length; i++)
        {
            Vector3 v3 = bounceScale * Random.onUnitSphere;
            sphereBounces[i].x = v3.x;
            sphereBounces[i].y = v3.y;
            sphereBounces[i].z = v3.z;

            spherePositions[i] = spheres[i].transform.position;
        }
    }

    Vector3 tmp;

    // Update is called once per frame
    void Update ()
    {
        for (int i = 0; i < cubes.Length; i++)
        {
            cubes[i].transform.rotation *= cubeRotations[i];
        }

        for (int i = 0; i < spheres.Length; i++)
        {
            tmp = spherePositions[i];
            tmp.x += sphereBounces[i].x * Mathf.Sin(Time.timeSinceLevelLoad);
            tmp.y += sphereBounces[i].y * Mathf.Cos(Time.timeSinceLevelLoad);
            tmp.z += sphereBounces[i].z * Mathf.Sin(-Time.timeSinceLevelLoad);
            spheres[i].transform.position = tmp;
        }
    }
}
