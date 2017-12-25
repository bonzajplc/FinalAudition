using UnityEngine;
using UnityEditor;
using System;

public class FBXImportOverride : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        ModelImporter importer = assetImporter as ModelImporter;
        String name = importer.assetPath.ToLower();

        if (name.Substring(name.Length - 4, 4) == ".fbx")
        {
            importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        }
    }
}