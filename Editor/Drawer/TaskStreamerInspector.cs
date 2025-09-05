using UnityEditor;
using UnityEngine;

namespace TaskStreamer.Tool
{
    [CustomEditor(typeof(TaskStreamer))]
    public class TaskStreamerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            SerializedProperty graphAsset = serializedObject.FindProperty("_graphAsset");
            SerializedProperty pauseUpdate = serializedObject.FindProperty("_pauseUpdate");
            SerializedProperty updateMode = serializedObject.FindProperty("_tickMode");
            SerializedProperty runtimBB = serializedObject.FindProperty("_runtimeBlackboard");

            GraphAsset previousGraph = graphAsset.objectReferenceValue as GraphAsset;
            
            graphAsset.objectReferenceValue = EditorGUILayout.ObjectField("Graph Asset", graphAsset.objectReferenceValue, typeof(GraphAsset), false);
            updateMode.enumValueIndex = EditorGUILayout.Popup("Tick Mode", updateMode.enumValueIndex, updateMode.enumDisplayNames);
            pauseUpdate.boolValue = EditorGUILayout.Toggle("Pause Update", pauseUpdate.boolValue);


            if ((graphAsset.objectReferenceValue == null || previousGraph != graphAsset.objectReferenceValue) && EditorWindow.HasOpenInstances<TaskStreamerEditor>())
            {
                TaskStreamerEditor.OpenWindow(graphAsset.objectReferenceValue as GraphAsset);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Stream Editor", GUILayout.ExpandWidth(true)) && graphAsset.objectReferenceValue != null)
                {
                    TaskStreamerEditor.OpenWindow(graphAsset.objectReferenceValue as GraphAsset);
                }

                if (GUILayout.Button("Open Editor Settings", GUILayout.ExpandWidth(true)))
                {
                    SettingsService.OpenProjectSettings(SettingRegister.SettingsResistryPath);
                }
            }

            if (SerializedProperty.DataEquals(runtimBB, null) == false)
            {
                EditorGUILayout.PropertyField(runtimBB);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}