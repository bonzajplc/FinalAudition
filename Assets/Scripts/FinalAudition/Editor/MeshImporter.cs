// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FinalAudition
{
    [ScriptedImporter(1, "famesh")]
    class FAmeshImporter : ScriptedImporter
    {
        static Material GetDefaultMaterial()
        {
            return AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Materials/DefaultSurfaceMaterial.mat"
            );
        }

        public override void OnImportAsset(AssetImportContext context)
        {
            // Mesh container
            // Create a prefab with MeshFilter/MeshRenderer.
            var gameObject = new GameObject();
            var mesh = ImportAsMesh(context.assetPath);

            //mesh.RecalculateNormals();

            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = GetDefaultMaterial();

            //            context.AddSubAsset("prefab", gameObject);
            if (gameObject != null) context.AddObjectToAsset("prefab", gameObject);
            if (mesh != null) context.AddObjectToAsset("mesh", mesh);

            context.SetMainObject(gameObject);
        }

        class DataBody
        {
            public List<Vector3> vertices;
            public List<Vector3> normals;
            public List<Vector2> uvs;
            public List<int> indices;

            public DataBody(List<Vector3> inVertices,            List<Vector3> inNormals,             List<Vector2> inUvs,             List<int> inIndices) 
            {
                vertices = inVertices;
                normals = inNormals;
                uvs = inUvs;
                indices = inIndices;
            }
        }

        Mesh ImportAsMesh(string path)
        {
            var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var body = ReadDataBody(new BinaryReader(stream));

            var mesh = new Mesh();
            mesh.name = Path.GetFileNameWithoutExtension(path);

            mesh.SetVertices(body.vertices);
            mesh.SetNormals(body.normals);

            if (body.uvs != null)
                mesh.SetUVs(0, body.uvs);

            mesh.SetTriangles(body.indices, 0, true);

            mesh.UploadMeshData(false);
            return mesh;
        }

        DataBody ReadDataBody( BinaryReader reader)
        {
            int bufferType = reader.ReadInt32();
            int bufferSize = reader.ReadInt32();
            List<Vector3> vertices = null;
            List<Vector3> normals = null;
            List<Vector2> uvs = null;
            List<int> indices = null;

            if (bufferType == 0)
            {
                int vertexCount = bufferSize / ((3 + 3) * 4); // ( pos + norm ) * sizeof float
                vertices = new List<Vector3>(vertexCount);
                normals = new List<Vector3>(vertexCount);

                for (var i = 0; i < vertexCount; i++)
                {
                    Vector3 vertex;
                    vertex.x = reader.ReadSingle();
                    vertex.y = reader.ReadSingle();
                    vertex.z = reader.ReadSingle();

                    vertices.Add(vertex);

                    Vector3 normal;
                    normal.x = reader.ReadSingle();
                    normal.y = reader.ReadSingle();
                    normal.z = reader.ReadSingle();

                    normals.Add(normal);
                }

                bufferType = reader.ReadInt32();
                bufferSize = reader.ReadInt32();
            }

            if (bufferType == 1)
            {
                int vertexCount = bufferSize / (2 * 4); // uv * sizeof float
                uvs = new List<Vector2>(vertexCount);

                for (var i = 0; i < vertexCount; i++)
                {
                    Vector2 uv;
                    uv.x = reader.ReadSingle();
                    uv.y = reader.ReadSingle();

                    uvs.Add(uv);
                }

                bufferType = reader.ReadInt32();
                bufferSize = reader.ReadInt32();
            }

            if (bufferType == 2)
            {
                int indexCount = bufferSize / 2; // sizeof of short
                indices = new List<int>(indexCount);

                for (var i = 0; i < indexCount; i++)
                {
                    indices.Add(reader.ReadInt16());
                }

                //indices.Reverse(); //CCW
            }

            var data = new DataBody( vertices, normals, uvs, indices );

            return data;
        }
    }
}
