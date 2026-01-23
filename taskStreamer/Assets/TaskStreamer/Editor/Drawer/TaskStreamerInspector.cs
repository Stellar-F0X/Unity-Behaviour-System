using TaskStreamer.Runtime;
using UnityEditor;
using UnityEngine;
using UEditorUtility = UnityEditor.EditorUtility;

namespace TaskStreamer.Tool
{
    [CustomEditor(typeof(TaskStreamer.Runtime.TaskStreamer))]
    public class TaskStreamerInspector : Editor
    {
        private void OnEnable()
        {
            Undo.undoRedoPerformed -= this.OnUndoRedoPerformed;
            Undo.undoRedoPerformed += this.OnUndoRedoPerformed;
        }

        
        
        private void OnDisable()
        {
            // Undo 콜백 해제
            Undo.undoRedoPerformed -= this.OnUndoRedoPerformed;
        }


        
        /// <summary> Undo/Redo가 수행될 때 호출되는 콜백 메서드입니다. </summary>
        private void OnUndoRedoPerformed()
        {
            serializedObject.Update();
            base.Repaint();
            
            SerializedProperty graphAsset = serializedObject.FindProperty("_graphAsset");
            TaskStreamerEditor.OpenWindow(graphAsset.objectReferenceValue as GraphAsset); 
        }

        
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty updateMode = serializedObject.FindProperty("_tickMode");
            SerializedProperty graphAsset = serializedObject.FindProperty("_graphAsset");
            SerializedProperty pauseUpdate = serializedObject.FindProperty("_pauseUpdate");
            SerializedProperty runtimeBlackboard = serializedObject.FindProperty("_runtimeBlackboard");

            GraphAsset previousGraph = graphAsset.objectReferenceValue as GraphAsset;

            
            
            //Graph Asset
            {
                EditorGUI.BeginChangeCheck();

                graphAsset.objectReferenceValue = EditorGUILayout.ObjectField("Graph Asset", graphAsset.objectReferenceValue, typeof(GraphAsset), false);

                if (EditorGUI.EndChangeCheck())
                {
                    // GraphAsset 변경 시 Undo 기록
                    Undo.RecordObject(target, "Change Graph Asset");
                    this.HandleGraphAssetChange(previousGraph, graphAsset);
                }
            }



            //Tick Mode
            {
                EditorGUI.BeginChangeCheck();

                updateMode.enumValueIndex = EditorGUILayout.Popup("Tick Mode", updateMode.enumValueIndex, updateMode.enumDisplayNames);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Change Tick Mode");
                }
            }



            //Pasue Update
            {
                EditorGUI.BeginChangeCheck();

                pauseUpdate.boolValue = EditorGUILayout.Toggle("Pause Update", pauseUpdate.boolValue);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Change Pause Update");
                }
            }

            
            
            this.DrawEditorButtons(graphAsset);
            
            this.DrawRuntimeBlackboardField(runtimeBlackboard);


            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                UEditorUtility.SetDirty(target);
            }
        }

        

        /// <summary> 런타임 블랙보드 필드를 Inspector 창에 표시합니다. </summary>
        /// <param name="runtimeBlackboard">Inspector에 표시할 SerializedProperty 타입의 런타임 블랙보드 데이터입니다.</param>
        private void DrawRuntimeBlackboardField(SerializedProperty runtimeBlackboard)
        {
            if (SerializedProperty.DataEquals(runtimeBlackboard, null))
            {
                return;
            }

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(runtimeBlackboard);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Change Runtime Blackboard");
                UEditorUtility.SetDirty(target);
            }
        }


        
        /// <summary> 그래프 자산(GraphAsset)의 변경 사항을 처리하고 에디터 창을 업데이트합니다. </summary>
        /// <param name="previousGraph">이전의 그래프 자산입니다.</param>
        /// <param name="graphAsset">현재 선택된 그래프 자산을 나타내는 SerializedProperty입니다.</param>
        private void HandleGraphAssetChange(GraphAsset previousGraph, SerializedProperty graphAsset)
        {
            if (EditorWindow.HasOpenInstances<TaskStreamerEditor>() == false)
            {
                return;
            }

            if (previousGraph != graphAsset.objectReferenceValue)
            {
                TaskStreamerEditor.OpenWindow(graphAsset.objectReferenceValue as GraphAsset);
            }
            else if (graphAsset.objectReferenceValue == null)
            {
                TaskStreamerEditor.ClearWindow();
            }
        }

        

        /// TaskStreamer의 GraphAsset에 관련된 에디터 버튼을 생성하고 동작을 정의합니다.
        /// <param name="graphAsset">연결된 GraphAsset을 나타내는 SerializedProperty입니다.</param>
        private void DrawEditorButtons(SerializedProperty graphAsset)
        {
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
        }
    }
}