using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(PointCloudBehaviour))]
public class PointCloudDrawer : PropertyDrawer
{
    public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
        int fieldCount = 5;
        return fieldCount * EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty PointCloudDirectoryProp = property.FindPropertyRelative("PointCloudDirectory");
        SerializedProperty translateProp = property.FindPropertyRelative("translate");
        SerializedProperty scaleProp = property.FindPropertyRelative("scale");
        SerializedProperty influenceProp = property.FindPropertyRelative("influence");

        Rect singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        singleFieldRect.width = position.width * 4 / 5;
        EditorGUI.PropertyField(singleFieldRect, PointCloudDirectoryProp);

        singleFieldRect.x += singleFieldRect.width;
        singleFieldRect.width = position.width / 5;
        if (EditorGUI.DropdownButton(singleFieldRect, new GUIContent("ChooseDirectory"), FocusType.Keyboard))
        {
            string dataPath = Application.dataPath;
            dataPath = dataPath.Replace("/Assets", "");

            string path = EditorUtility.OpenFolderPanel("Choose Directory With Point Cloud Clips", dataPath + PointCloudDirectoryProp.stringValue, "");

            if( path != "" )
            {
                if (path.StartsWith(dataPath))
                {
                    path = path.Substring(dataPath.Length);
                }

                PointCloudClip pcc = property.serializedObject.targetObject as PointCloudClip;
                pcc.template.PointCloudDirectory = path;

                PointCloudDirectoryProp.stringValue = path;
                PointCloudDirectoryProp.serializedObject.Update();
                PointCloudDirectoryProp.serializedObject.ApplyModifiedProperties();
            }
        }

        singleFieldRect.x = position.x;
        singleFieldRect.width = position.width;
        singleFieldRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(singleFieldRect, translateProp);

        singleFieldRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(singleFieldRect, scaleProp);

        singleFieldRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(singleFieldRect, influenceProp);
    }
}
