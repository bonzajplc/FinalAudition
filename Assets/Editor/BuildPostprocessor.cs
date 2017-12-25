using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BuildPostprocessor : IPostprocessBuild
{
    public int callbackOrder { get { return 0; } }

    public void
    OnPostprocessBuild(BuildTarget target, string path)
    {
        int last_slash = path.LastIndexOf('/');
        int last_dot = path.LastIndexOf('.');
        string buildPath = path.Substring(0, last_slash+1);
          
        if (Directory.Exists(buildPath + "PointClouds"))
        {
            string dataDirectory = path.Substring(0, last_dot);
            dataDirectory   +=  "_Data";
            if (Directory.Exists(dataDirectory + "/PointsClouds"))
                Directory.Move(buildPath + "PointClouds", dataDirectory + "/PointsClouds");
        }
    }
}
