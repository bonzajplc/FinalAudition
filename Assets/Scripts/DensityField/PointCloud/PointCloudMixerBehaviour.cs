using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Runtime.InteropServices;

public class PointCloudMixerBehaviour : PlayableBehaviour
{
    [DllImport("RenderingPlugin")]
    private static extern int SetParticleClipFrameName(int clipID, [MarshalAs(UnmanagedType.LPStr)] string frameName, [MarshalAs(UnmanagedType.LPStr)] string nextFrameName, float fraction, float influence, int resolution, 
        float scaleX, float scaleY, float scaleZ, float offsetX, float offsetY, float offsetZ, bool forceReload );
    [DllImport("RenderingPlugin")]
    private static extern int ClearDensityField(int resolution);
    [DllImport("RenderingPlugin")]
    private static extern int GenerateDensityField(int resolution);

    [DllImport("RenderingPlugin")]
    private static extern int RegisterParticleTrack(int trackID, IntPtr particleBuffer, int particleCount, int bufferStride, int resolution );
    [DllImport("RenderingPlugin")]
    private static extern int UnregisterParticleTrack(int trackID);

    struct DensityFieldParticle
    {
        public uint pos;
        public float magnitude;
        public uint color;
    }

    public DensityFieldManager DFMRef_ = null;

    private DensityFieldParticle[] particles_ = null;
    public GCHandle pinnedParticles_;
 #if !UNITY_EDITOR
    //static int lastMixerFrame = -1;
#endif

    public override void OnPlayableCreate(Playable playable)
    {
    }

    public void RegisterTrack()
    {
        particles_ = new DensityFieldParticle[DFMRef_.maxParticles];

        pinnedParticles_ = GCHandle.Alloc(particles_, GCHandleType.Pinned);

        RegisterParticleTrack(GetHashCode(), pinnedParticles_.AddrOfPinnedObject(), DFMRef_.maxParticles, (1 + 1 + 1) * sizeof(float), DFMRef_.Resolution );
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        UnregisterParticleTrack(GetHashCode());
        pinnedParticles_.Free();
    }

    // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        int inputCount = playable.GetInputCount ();

        bool densityFieldCleared = false;

#if !UNITY_EDITOR
        //check the current time of timeline
 /*       PlayableDirector director = DFMRef_.gameObject.GetComponent<PlayableDirector>();
        int frame = -1;

        if (director != null)
        {
#if UNITY_PS4
            int frameRate = 60;
#else
            int frameRate = 90;
#endif
            //frame = (int)(director.time * frameRate);
        }

        if (lastMixerFrame == frame)
            return;

        lastMixerFrame = frame;*/
#endif

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);

            if ( inputWeight > 0.0f )
            {
                ScriptPlayable<PointCloudBehaviour> inputPlayable = (ScriptPlayable<PointCloudBehaviour>)playable.GetInput(i);
                float time = (float)inputPlayable.GetTime();
                PointCloudBehaviour input = inputPlayable.GetBehaviour();

#if UNITY_EDITOR
                if ( input.PointCloudDirectory != input.PointCloudDirectoryInternal )
                {
                    input.SetFrameFileList();
                }
#endif

                if (input.frameFiles.Length == 0)
                    continue;

                if (!densityFieldCleared)
                {
                    ClearDensityField(DFMRef_.Resolution);
                    densityFieldCleared = true;
                }

                int newFrame = (int)(time * 30.0f);
                float fraction = (time * 30.0f) - (int)(time * 30.0f);
                Shader.SetGlobalFloat("_ArtSpaces_frame_fraction", fraction);

                bool forceReload = false;
                if (newFrame - input.frameNo != 0 && newFrame - input.frameNo != 1)
                {
                    forceReload = true;
#if UNITY_EDITOR
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
#endif
                }
                input.frameNo = newFrame;

                if (input.frameNo >= input.frameFiles.Length)
                    input.frameNo = input.frameFiles.Length - 1;

                int nextFrameNo = newFrame + 1;

                if (nextFrameNo >= input.frameFiles.Length)
                    nextFrameNo = input.frameFiles.Length - 1;

                SetParticleClipFrameName(input.GetHashCode(), input.frameFiles[input.frameNo], input.frameFiles[nextFrameNo], fraction, inputWeight * input.influence, DFMRef_.Resolution, 
                    input.scale.x, input.scale.y, input.scale.z, input.translate.x, input.translate.y, input.translate.z, forceReload);

                //Debug.Log("Current frame: " + input.frameNo + "   Next frame: " + nextFrameNo + " fraction: " + fraction);
            }
        }

        DFMRef_.numPointCloudParticles_ = GenerateDensityField(GetHashCode());
        if (DFMRef_.numPointCloudParticles_ >= 0)
        {
            DFMRef_.particleBuffer_.SetData(particles_, 0, 0, DFMRef_.numPointCloudParticles_);
        }
    }
}
