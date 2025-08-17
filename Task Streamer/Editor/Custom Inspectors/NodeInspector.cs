using TaskStreamer.BT;
using TaskStreamer.FSM;
using UnityEditor;
using UnityEngine;

namespace TaskStreamer.Tool
{
    [CustomEditor(typeof(NodeBase), true)]
    public class NodeInspector : Editor
    {
        private GUIStyle _headerLabelStyle;


        public override void OnInspectorGUI()
        {
            if (_headerLabelStyle is null)
            {
                _headerLabelStyle = EditorUtilities.GetHeaderStyle();
            }
            
            //=========================================================================================
            //=================================[Base Class Fields]=====================================
            
            //TODO: 추후 다른 그래프를 추가하게 된다면 그 그래프에서 다룰 노드에 대한 것을 추가해야 됨.
            switch (serializedObject.targetObject)
            {
                case StateBase: EditorUtilities.DrawHeader("State Inspector", _headerLabelStyle, endSpacing: 2); break;
                
                case BehaviorNodeBase: EditorUtilities.DrawHeader("Behavior Inspector", _headerLabelStyle, endSpacing: 2); break;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }

            SerializedProperty nameProp = serializedObject.FindProperty("m_Name");
            SerializedProperty tagProp = serializedObject.FindProperty("_tag");
            SerializedProperty desProp = serializedObject.FindProperty("_description");

            using (new EditorGUI.DisabledScope(!TaskStreamerEditor.canEditGraph))
            {
                nameProp.stringValue = EditorGUILayout.TextField("Name", nameProp.stringValue);
                tagProp.stringValue = EditorGUILayout.TextField("Tag", tagProp.stringValue);

                EditorGUILayout.LabelField("Description");
                desProp.stringValue = EditorGUILayout.TextArea(desProp.stringValue, GUILayout.Height(EditorGUIUtility.singleLineHeight * 3));
            }

            SerializedProperty startProp = serializedObject.FindProperty("position");
            //=========================================================================================
            //=========================================================================================


            //=========================================================================================
            //=================================[Child Class Fields]====================================
            if (EditorUtilities.HasRemainingPropertiesAfter(startProp))
            {
                EditorUtilities.DrawHeader(this.target.name, _headerLabelStyle, 10f, 2f);
                EditorUtilities.DrawPropertiesRange(startProp, startInclusive: false);
            }
            //=========================================================================================
            //=========================================================================================

            serializedObject.ApplyModifiedProperties();
        }
    }
}