using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AsyncLoader))]
public class AsyncLoaderEditor : Editor
{
    SerializedProperty  nextSceneProp;
    SerializedProperty  timeTypeProp;
    SerializedProperty  sceneLoadTimeProp;
    SerializedProperty  loadSceneInEditorProp;
    SerializedProperty  sceneNameProp;

    SerializedProperty fadeInProp;
    SerializedProperty fadeInTimeProp;
    SerializedProperty fadeInOnlyInXRModeProp;

    SerializedProperty fadeOutProp;
    SerializedProperty fadeOutTimeProp;
    SerializedProperty fadeOutOnlyInXRModeProp;

    private void OnEnable()
    {
        nextSceneProp           =   serializedObject.FindProperty("nextScene");
        timeTypeProp            =   serializedObject.FindProperty("timeType");
        sceneLoadTimeProp       =   serializedObject.FindProperty("sceneLoadTime");
        loadSceneInEditorProp   =   serializedObject.FindProperty("loadSceneInEditor");
        sceneNameProp           =   serializedObject.FindProperty("sceneName");

        fadeInProp              =   serializedObject.FindProperty("fadeIn");
        fadeInTimeProp          =   serializedObject.FindProperty("fadeInTime");
        fadeInOnlyInXRModeProp  =   serializedObject.FindProperty("fadeInOnlyInXRMode");

        fadeOutProp             =   serializedObject.FindProperty("fadeOut");
        fadeOutTimeProp         =   serializedObject.FindProperty("fadeOutTime");
        fadeOutOnlyInXRModeProp =   serializedObject.FindProperty("fadeOutOnlyInXRMode");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((AsyncLoader)target), typeof(AsyncLoader), false);
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.ObjectField(nextSceneProp);
        
        GUI.enabled = false;
        EditorGUILayout.TextField(sceneNameProp.stringValue);
        GUI.enabled = true;

        AsyncLoader.TimeType type = (AsyncLoader.TimeType)timeTypeProp.enumValueIndex;
        type     =   (AsyncLoader.TimeType)EditorGUILayout.EnumPopup("Time Type", type);
        timeTypeProp.enumValueIndex = (int)type;
        sceneLoadTimeProp.floatValue    =   EditorGUILayout.FloatField("Scene Load Time", sceneLoadTimeProp.floatValue);
        loadSceneInEditorProp.boolValue =   EditorGUILayout.Toggle("Load scene in editor", loadSceneInEditorProp.boolValue);

        fadeInProp.boolValue = EditorGUILayout.Toggle("Fade In", fadeInProp.boolValue);
        EditorGUI.BeginDisabledGroup(!fadeInProp.boolValue);
        fadeInTimeProp.floatValue = EditorGUILayout.FloatField("Fade In Time", fadeInTimeProp.floatValue);
        fadeInOnlyInXRModeProp.boolValue = EditorGUILayout.Toggle("Fade In Only In XR Mode", fadeInOnlyInXRModeProp.boolValue);
        EditorGUI.EndDisabledGroup();

        fadeOutProp.boolValue = EditorGUILayout.Toggle("Fade Out", fadeOutProp.boolValue);
        EditorGUI.BeginDisabledGroup(!fadeOutProp.boolValue);
        fadeOutTimeProp.floatValue = EditorGUILayout.FloatField("Fade Out Time", fadeOutTimeProp.floatValue);
        fadeOutOnlyInXRModeProp.boolValue = EditorGUILayout.Toggle("Fade Out Only In XR Mode", fadeOutOnlyInXRModeProp.boolValue);
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
        sceneNameProp.stringValue   =   ((AsyncLoader) target).nextScene.name;
        serializedObject.ApplyModifiedProperties();
    }
}
