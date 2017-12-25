using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class PointCloudClip : PlayableAsset, ITimelineClipAsset
{
    public PointCloudBehaviour template = new PointCloudBehaviour ();
    public ExposedReference<DensityFieldManager> MarchingCubesManagerRef;

    public ClipCaps clipCaps
    {
        get { return ClipCaps.All; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<PointCloudBehaviour>.Create (graph, template);
        PointCloudBehaviour clone = playable.GetBehaviour ();

        //connect first marching cubes manager that is found in the scene
        DensityFieldManager[] managers = FindObjectsOfType(typeof(DensityFieldManager)) as DensityFieldManager[];

        if ( managers.Length > 0 )
            MarchingCubesManagerRef.defaultValue = managers[0];

        clone.MarchingCubesManagerRef = MarchingCubesManagerRef.Resolve (graph.GetResolver ());

        clone.RegisterPointCloudClip();

        return playable;
    }
}
