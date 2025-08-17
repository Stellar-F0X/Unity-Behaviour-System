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
                _headerLabelStyle = EditorUtilities.GetHeaderStyle();
            }

            if (target is not Transition transition)
            {
                EditorGUILayout.HelpBox("This editor is only for Transition objects.", MessageType.Error);
                Debug.LogError("This editor is only for Transition objects.");
                return;
            }

            EditorUtilities.DrawHeader("Transition Inspector", _headerLabelStyle, endSpacing: 2);

            
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }

            SerializedProperty descriptionProp = serializedObject.FindProperty("_description");
            SerializedProperty isConditionalProp = serializedObject.FindProperty("_conditional");
            SerializedProperty conditionsProp = serializedObject.FindProperty("_conditions");

            
            bool foundStartNode = TaskStreamerEditor.Instance.view.focusGraph.TryGetNodeByGuid(transition.fromNodeGuid, out NodeBase startNode);
            bool foundDestinationNode = TaskStreamerEditor.Instance.view.focusGraph.TryGetNodeByGuid(transition.toNodeGuid, out NodeBase endNode);

            
            if (foundStartNode == false || foundDestinationNode == false)
            {
                EditorGUILayout.HelpBox("Target state not found in the current graph.", MessageType.Warning);
            }
            
            
            EditorGUILayout.LabelField("Transition", $"{startNode.name} → {endNode.name}");
            EditorGUILayout.LabelField("Description");

            GUILayoutOption heightOption = GUILayout.Height(EditorGUIUtility.singleLineHeight * 3);

            using (new EditorGUI.DisabledScope(!TaskStreamerEditor.canEditGraph))
            {
                descriptionProp.stringValue = EditorGUILayout.TextArea(descriptionProp.stringValue, heightOption);
            }

            EditorUtilities.DrawHeader("Condition", _headerLabelStyle, startSpacing: 5, endSpacing: 2);
            isConditionalProp.boolValue = EditorGUILayout.Toggle("Conditional", isConditionalProp.boolValue);

            if (isConditionalProp.boolValue == false)
            {
                serializedObject.ApplyModifiedProperties(); //early return.
                return;
            }

            
            using (new EditorGUI.DisabledScope(!TaskStreamerEditor.canEditGraph))
            {
                EditorGUILayout.PropertyField(conditionsProp, new GUIContent("Conditions"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}