using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

namespace FinalAudition
{
    [ScriptedImporter(1, "animfa")]
    class AnimFAImporter : ScriptedImporter
    {
        class Set<T> : SortedDictionary<T, bool>
        {
            public void Add(T item)
            {
                if( !this.ContainsKey(item) )
                    this.Add(item, true);
            }
        }

        public override void OnImportAsset(AssetImportContext context)
        {
            string animationClipName = Path.GetFileNameWithoutExtension(context.assetPath);
            string propertyName = "";

            string[] supportedProperties = { "_translateX","_translateY","_translateZ","_rotateX","_rotateY","_rotateZ","_scaleX","_scaleY","_scaleZ" };
            string[] propertyNames = { "localPosition.x", "localPosition.y", "localPosition.z", "localEulerAngles.x", "localEulerAngles.y", "localEulerAngles.z", "localScale.x", "localScale.y", "localScale.z" };

            //check if animation clip is supported

            int i = 0;
            foreach (var property in supportedProperties)
            {
                if (animationClipName.Contains(property))
                {
                    animationClipName = animationClipName.Replace(property, ""); // to replace the specific text with blank
                    propertyName = propertyNames[i];
                    break;
                }
                i++;
            }

            bool isRotateCurve = false;

            if (i > 2 && i < 6)
            {
                isRotateCurve = true;
            }

            if( propertyName == "" )
            {
                //skip since it's not supported
                //context.SetMainAsset("AnimatorController", new GameObject());
                return;
            }

            string clipName = Path.GetDirectoryName(context.assetPath) + "/" + animationClipName + ".anim";

            AnimationClip animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>( clipName );
            if( animationClip == null )
            {
                animationClip = new AnimationClip();
                animationClip.name = animationClipName;
            }

            //fill the animation curve
            if( isRotateCurve ) //completely different path
            {
                AnimationCurve animRotateX = new AnimationCurve();
                AnimationCurve animRotateY = new AnimationCurve();
                AnimationCurve animRotateZ = new AnimationCurve();

                LoadAnimationCurve(Path.GetDirectoryName(context.assetPath) + "/" + animationClipName + "_rotateX.animfa", animRotateX, false);
                LoadAnimationCurve(Path.GetDirectoryName(context.assetPath) + "/" + animationClipName + "_rotateY.animfa", animRotateY, false);
                LoadAnimationCurve(Path.GetDirectoryName(context.assetPath) + "/" + animationClipName + "_rotateZ.animfa", animRotateZ, false);

                AnimationCurve correctedAnimRotateX = new AnimationCurve();
                AnimationCurve correctedAnimRotateY = new AnimationCurve();
                AnimationCurve correctedAnimRotateZ = new AnimationCurve();
                AnimationCurve correctedAnimRotateW = new AnimationCurve();

                float radiansToAngles = 180.0f / Mathf.PI;

                Set<float> set = new Set<float>();

                for (int k = 0; k < animRotateX.length; k++)
                {
                    set.Add(animRotateX.keys[k].time);
                }
                for (int k = 0; k < animRotateY.length; k++)
                {
                    set.Add(animRotateY.keys[k].time);
                }
                for (int k = 0; k < animRotateZ.length; k++)
                {
                    set.Add(animRotateZ.keys[k].time);
                }

                foreach ( var key in set )
                {
                    float time = key.Key;
                    float mayaRotX = animRotateX.Evaluate(time) * radiansToAngles;
                    float mayaRotY = animRotateY.Evaluate(time) * radiansToAngles;
                    float mayaRotZ = animRotateZ.Evaluate(time) * radiansToAngles;

                    var flippedRotation = new Vector3(mayaRotX, -mayaRotY, -mayaRotZ);
                    var qx = Quaternion.AngleAxis(flippedRotation.x, Vector3.right);
                    var qy = Quaternion.AngleAxis(flippedRotation.y, Vector3.up);
                    var qz = Quaternion.AngleAxis(flippedRotation.z, Vector3.forward);
                    var unityRotationQuaternion = qx * qy * qz;

                    correctedAnimRotateX.AddKey(time, unityRotationQuaternion.x);
                    correctedAnimRotateY.AddKey(time, unityRotationQuaternion.y);
                    correctedAnimRotateZ.AddKey(time, unityRotationQuaternion.z);
                    correctedAnimRotateW.AddKey(time, unityRotationQuaternion.w);
                }

                animationClip.SetCurve("", typeof(Transform), "localRotation.x", correctedAnimRotateX);
                animationClip.SetCurve("", typeof(Transform), "localRotation.y", correctedAnimRotateY);
                animationClip.SetCurve("", typeof(Transform), "localRotation.z", correctedAnimRotateZ);
                animationClip.SetCurve("", typeof(Transform), "localRotation.w", correctedAnimRotateW);

                //This ensures a smooth interpolation
                animationClip.EnsureQuaternionContinuity();
            }
            else
            {
                AnimationCurve anim = new AnimationCurve();

                bool reverseValues = false;
                if (propertyName == "_rotateX")
                    reverseValues = true;

                LoadAnimationCurve(context.assetPath, anim, reverseValues);

                animationClip.SetCurve("", typeof(Transform), propertyName, anim);
            }

            AssetDatabase.CreateAsset(animationClip, clipName);

            //string controllerName = Path.GetDirectoryName(context.assetPath) + "/" + animationClipName + ".controller";

            //var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPathWithClip(controllerName, animationClip);

            //context.SetMainAsset("AnimatorController", new GameObject());
        }

        void LoadAnimationCurve( string curvePath, AnimationCurve anim, bool reverseValues )
        {
            var stream = File.Open(curvePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (stream == null)
                return;

            var reader = new StreamReader(stream);

            //skip 4 lines of curve endings
            reader.ReadLine();
            reader.ReadLine();
            reader.ReadLine();
            reader.ReadLine();

            int numKeys = int.Parse(reader.ReadLine());

            for (int i = 0; i < numKeys; i++)
            {
                string text = reader.ReadLine();
                string[] bits = text.Split(' ');
                float time = float.Parse(bits[0]);
                float value = float.Parse(bits[1]);

                if (reverseValues)
                    value = -value;

                anim.AddKey(time, value);
            }

            stream.Close();
        }
    }
}