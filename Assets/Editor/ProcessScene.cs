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

public class ProcessScene : IProcessScene
{
    public int callbackOrder { get { return 0; } }

    public void OnProcessScene(UnityEngine.SceneManagement.Scene scene)
    {
        string  path =   EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget);
        int last_slash = path.LastIndexOf('/');
        string  pointCloudDirectory = path.Substring(0, last_slash + 1);
        
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            PlayableDirector    director    =   gameObject.GetComponent<PlayableDirector>();
            if (director != null)
            {
                if (director.playableAsset != null)
                {
                    TimelineAsset   timeline    =   (TimelineAsset) director.playableAsset;
                            
                    foreach (var track in timeline.GetOutputTracks())
                    {
                        foreach (var clip in track.GetClips())
                        {
                            if (clip.asset.GetType() == typeof(PointCloudClip))
                            {
                                uint clip_start_frame = (uint) clip.clipIn * 30;
                                uint clip_duration_in_frames = (uint)clip.duration * 30;
                                uint clip_end_frame = clip_start_frame + clip_duration_in_frames;

                                string[] files  =   new string[0];
                                PointCloudClip pcc = (PointCloudClip)clip.asset;
                                if (Directory.Exists(pcc.template.PointCloudDirectory.Substring(1)))
                                    files = Directory.GetFiles(pcc.template.PointCloudDirectory.Substring(1));

                                foreach (var file in files)
                                {
                                    Directory.CreateDirectory(pointCloudDirectory + pcc.template.PointCloudDirectory);

                                    int last_dot_in_filename = file.LastIndexOf('.');
                                    int last_slash_in_filename = file.LastIndexOf('\\');
                                    int no_length = last_dot_in_filename - last_slash_in_filename - 1;
                                    uint file_no = UInt32.Parse(file.Substring(last_slash_in_filename + 1, no_length));
                            
                                    if (file_no >= clip_start_frame &&
                                        file_no <= clip_end_frame)
                                    {
                                        if (!File.Exists(pointCloudDirectory + '/' + file))
                                            File.Copy(file, pointCloudDirectory + '/' + file);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
