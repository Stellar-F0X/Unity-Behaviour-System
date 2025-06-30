using UnityEditor;
using UnityEngine;
using Transition = TaskStreamer.FSM.Transition;

namespace TaskStreamer.Tool
{
    [CustomEditor(typeof(Transition), true)]
    public class TransitionInspector : Editor
    {
        private GUIStyle _headerLabelStyle;


        public override void OnInspectorGUI()
        {
            if (_headerLabelStyle is null)
            {
                _headerLabelStyle = EditorHelper.GetHeaderStyle();
            }

            if (target is not Transition transition)
            {
                EditorGUILayout.HelpBox("This editor is only for Transition objects.", MessageType.Error);
                Debug.LogError("This editor is only for Transition objects.");
                return;
            }

            EditorHelper.DrawHeader("Transition Inspector", _headerLabelStyle, endSpacing: 2);

            
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }

            SerializedProperty isConditionalProp = serializedObject.FindProperty("conditional");
            SerializedProperty desProp = serializedObject.FindProperty("description");
            SerializedProperty conditionsProp = serializedObject.FindProperty("conditions");
            SerializedProperty completeTypeProp = serializedObject.FindProperty("completeType");

            
            bool foundStartNode = TaskStreamerEditor.Instance.view.focusGraph.TryGetNodeByGuid(transition.startStateGuid, out NodeBase startNode);
            bool foundDestinationNode = TaskStreamerEditor.Instance.view.focusGraph.TryGetNodeByGuid(transition.targetStateGuid, out NodeBase endNode);

            
            if (foundStartNode == false || foundDestinationNode == false)
            {
                EditorGUILayout.HelpBox("Target state not found in the current graph.", MessageType.Warning);
            }
            
            
            EditorGUILayout.LabelField("Transition", $"{startNode.name} → {endNode.name}");
            EditorGUILayout.LabelField("Description");

            GUILayoutOption heightOption = GUILayout.Height(EditorGUIUtility.singleLineHeight * 3);

            using (new EditorGUI.DisabledScope(!TaskStreamerEditor.canEditGraph))
            {
                desProp.stringValue = EditorGUILayout.TextArea(desProp.stringValue, heightOption);
            }

            EditorHelper.DrawHeader("Condition", _headerLabelStyle, startSpacing: 5, endSpacing: 2);
            isConditionalProp.boolValue = EditorGUILayout.Toggle("Conditional", isConditionalProp.boolValue);

            if (isConditionalProp.boolValue == false)
            {
                serializedObject.ApplyModifiedProperties(); //early return.
                return;
            }
            
            // 어떤 방식으로 성공 시킬지.
            ECompleteType completeType = (ECompleteType)completeTypeProp.enumValueIndex;
            completeType = (ECompleteType)EditorGUILayout.EnumPopup(completeTypeProp.displayName, completeType);
            completeTypeProp.enumValueIndex = (int)completeType;

            
            using (new EditorGUI.DisabledScope(!TaskStreamerEditor.canEditGraph))
            {
                EditorGUILayout.PropertyField(conditionsProp, new GUIContent("Conditions"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}