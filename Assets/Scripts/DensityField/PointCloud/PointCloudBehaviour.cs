using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Runtime.InteropServices;
using System.IO;

[Serializable]
public class PointCloudBehaviour : PlayableBehaviour
{
    [DllImport("RenderingPlugin")]
    private static extern int RegisterParticleClip(int clipID, int particleCount);

    [DllImport("RenderingPlugin")]
    private static extern int UnregisterParticleClip(int clipID);

    [NonSerialized]
    public DensityFieldManager MarchingCubesManagerRef;

    public string PointCloudDirectory;
    public Vector3 translate = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public float influence = 1.0f;

    [NonSerialized]
    public string PointCloudDirectoryInternal = "";
    [NonSerialized]
    public int frameNo = 0;
    [NonSerialized]
    public string[] frameFiles = null;

    public override void OnPlayableCreate(Playable playable)
    {
    }

    public void RegisterPointCloudClip()
    {
        SetFrameFileList();

        RegisterParticleClip(GetHashCode(), MarchingCubesManagerRef.maxParticles);
    }

    public void SetFrameFileList()
    {
        string dataPath = Application.dataPath;
        dataPath = dataPath.Replace("/Assets", "");

        dataPath += PointCloudDirectory;
        frameFiles = Directory.GetFiles(dataPath, "*.bnzs");
        PointCloudDirectoryInternal = PointCloudDirectory;
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        UnregisterParticleClip(GetHashCode());
    }

    public override void OnGraphStart (Playable playable)
    {
    }
}
