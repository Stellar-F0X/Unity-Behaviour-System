using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TaskStreamer.Tool
{
    [CustomPropertyDrawer(typeof(RuntimeBlackboard))]
    public class RuntimeBlackboardInspector : PropertyDrawer
    {
        private ReorderableList _list;
        private SerializedProperty _lastProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty variablesProp = property.FindPropertyRelative("_clonedVariables");

            // Foldout 그리기
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            variablesProp.isExpanded = EditorGUI.Foldout(foldoutRect, variablesProp.isExpanded, label);

            if (variablesProp.isExpanded)
            {
                // ReorderableList 초기화 (필요한 경우에만)
                if (_list == null || _lastProperty != property)
                {
                    _list = new ReorderableList(property.serializedObject, variablesProp, false, false, false, false);
                    _list.drawElementBackgroundCallback = this.DrawElementBackground;
                    _list.elementHeightCallback = this.DrawHeightCallback;
                    _list.drawElementCallback = this.DrawElementCallback;
                    _lastProperty = property;
                }

                // 리스트 영역 계산
                float listY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                
                float listHeight = position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
                
                Rect listRect = new Rect(position.x, listY, position.width, listHeight);

                property.serializedObject.Update();
                _list.DoList(listRect);
            }

            property.serializedObject.ApplyModifiedProperties();
        }


        private float DrawHeightCallback(int index)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }


        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty elementProp = _list?.serializedProperty?.GetArrayElementAtIndex(index);

            if (elementProp == null)
            {
                return;
            }

            SerializedProperty keyProp = elementProp.FindPropertyRelative("_key");
            SerializedProperty valueProp = elementProp.FindPropertyRelative("_value");

            if (keyProp != null && valueProp != null)
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                rect.y += EditorGUIUtility.standardVerticalSpacing * 0.5f;
                EditorGUI.PropertyField(rect, valueProp, new GUIContent(keyProp.stringValue));
            }
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty variablesProp = property.FindPropertyRelative("_clonedVariables");

            float height = EditorGUIUtility.singleLineHeight; // Foldout 높이

            if (variablesProp?.isExpanded == true)
            {
                float defaultHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                
                height += EditorGUIUtility.standardVerticalSpacing;
                height += Mathf.Max(variablesProp.arraySize, 1) * defaultHeight;
                height += EditorGUIUtility.singleLineHeight; // ReorderableList 여백
            }

            return height;
        }
        
        
        private void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Color backgroundColor;

            // 선택된 상태일 때 더 명확한 색상 사용

            if (isActive)
            {
                backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 회색 계열
            }
            else if (index % 2 == 0)
            {
                backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.2f);
            }
            else
            {
                backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.2f);
            }

            EditorGUI.DrawRect(rect, backgroundColor);
        }
    }
}