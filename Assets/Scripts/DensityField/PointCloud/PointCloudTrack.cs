using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.4484754f, 0.605952f, 0.7720588f)]
[TrackClipType(typeof(PointCloudClip))]
public class PointCloudTrack : TrackAsset
{
    public ExposedReference<DensityFieldManager> MarchingCubesManagerRef;

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var playable = ScriptPlayable<PointCloudMixerBehaviour>.Create(graph, inputCount);
        PointCloudMixerBehaviour clone = playable.GetBehaviour();

        //connect first marching cubes manager that is found in the scene
        DensityFieldManager[] managers = FindObjectsOfType(typeof(DensityFieldManager)) as DensityFieldManager[];

        if (managers.Length > 0)
            MarchingCubesManagerRef.defaultValue = managers[0];

        clone.DFMRef_ = MarchingCubesManagerRef.Resolve(graph.GetResolver());

        clone.RegisterTrack();

        return playable;
    }
}
