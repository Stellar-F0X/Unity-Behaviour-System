using System;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using UnityEditor;
using UnityEngine;

namespace TaskStreamer.Tool
{
    [CustomEditor(typeof(TaskStreamer))]
    public class TaskStreamerInspector : Editor
    {
        private readonly string[] _graphTypeNames = Enum.GetNames(typeof(EGraphType));

        public override void OnInspectorGUI()
        {
            SerializedProperty graphAsset = serializedObject.FindProperty("_graphAsset");
            SerializedProperty pauseUpdate = serializedObject.FindProperty("_pauseUpdate");
            SerializedProperty updateMode = serializedObject.FindProperty("_tickMode");

            graphAsset.objectReferenceValue = EditorGUILayout.ObjectField("Graph Asset", graphAsset.objectReferenceValue, typeof(GraphAsset), false);
            updateMode.enumValueIndex = EditorGUILayout.Popup("Tick Mode", updateMode.enumValueIndex, updateMode.enumDisplayNames);
            pauseUpdate.boolValue = EditorGUILayout.Toggle("Pause Update", pauseUpdate.boolValue);

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

            serializedObject.ApplyModifiedProperties();
        }
    }
}