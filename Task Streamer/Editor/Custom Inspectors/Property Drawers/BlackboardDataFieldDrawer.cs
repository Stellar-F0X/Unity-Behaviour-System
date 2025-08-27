using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TaskStreamer.Tool
{
    [CustomPropertyDrawer(typeof(BlackboardData))]
    public class BlackboardDataFieldDrawer : PropertyDrawer
    {
        private const string _KEY_PROPERTY_NAME = "_key";
        private const string _VALUE_PROPERTY_NAME = "_value";
        private const string _VARIABLES_PROPERTY_NAME = "_variables";
        private const float _BACKGROUND_OPACITY_ACTIVE = 0.5f;
        private const float _BACKGROUND_OPACITY_NORMAL = 0.2f;
        private const float _VERTICAL_SPACING_MULTIPLIER = 0.5f;

        private ReorderableList _reorderableList;
        private string _lastPropertyPath;

        
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty variablesProperty = property.FindPropertyRelative(_VARIABLES_PROPERTY_NAME);

            if (variablesProperty == null)
            {
                EditorGUI.LabelField(position, label.text, "Property not found");
                return;
            }

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            variablesProperty.isExpanded = EditorGUI.Foldout(foldoutRect, variablesProperty.isExpanded, label);

            if (variablesProperty.isExpanded == false)
            {
                return;
            }

            this.InitializeReorderableListIfNeeded(property, variablesProperty);
            
            this.DrawExpandedList(position, property);
        }

        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty variablesProperty = property.FindPropertyRelative(_VARIABLES_PROPERTY_NAME);
            float totalHeight = EditorGUIUtility.singleLineHeight;

            if (variablesProperty?.isExpanded == true)
            {
                totalHeight += CalculateExpandedListHeight(variablesProperty);
            }

            return totalHeight;
        }

        
        private void InitializeReorderableListIfNeeded(SerializedProperty property, SerializedProperty variablesProperty)
        {
            string currentPropertyPath = property.propertyPath;

            if (_reorderableList != null && _lastPropertyPath == currentPropertyPath)
            {
                return;
            }

            _reorderableList = new ReorderableList(property.serializedObject, variablesProperty, false, false, false, false);
            
            _reorderableList.drawElementBackgroundCallback = DrawElementBackground;
            _reorderableList.elementHeightCallback = GetElementHeight;
            _reorderableList.drawElementCallback = DrawElement;
            
            _lastPropertyPath = currentPropertyPath;
        }

        
        private void DrawExpandedList(Rect position, SerializedProperty property)
        {
            if (_reorderableList == null)
            {
                return;
            }

            float listStartY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float listHeight = position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
            
            Rect listRect = new Rect(position.x, listStartY, position.width, listHeight);

            _reorderableList.DoList(listRect);
        }

        private float CalculateExpandedListHeight(SerializedProperty variablesProperty)
        {
            float elementHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float listContentHeight = Mathf.Max(variablesProperty.arraySize, 1) * elementHeight;
            return EditorGUIUtility.standardVerticalSpacing + listContentHeight + EditorGUIUtility.singleLineHeight;
        }

        
        private float GetElementHeight(int index)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        
        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty elementProperty = _reorderableList?.serializedProperty?.GetArrayElementAtIndex(index);

            if (elementProperty == null)
            {
                return;
            }

            SerializedProperty keyProperty = elementProperty.FindPropertyRelative(_KEY_PROPERTY_NAME);
            SerializedProperty valueProperty = elementProperty.FindPropertyRelative(_VALUE_PROPERTY_NAME);

            if (keyProperty == null || valueProperty == null)
            {
                return;
            }

            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += EditorGUIUtility.standardVerticalSpacing * _VERTICAL_SPACING_MULTIPLIER;

            string displayName = string.IsNullOrEmpty(keyProperty.stringValue) ? "Empty Key" : keyProperty.stringValue;
            
            EditorGUI.PropertyField(rect, valueProperty, new GUIContent(displayName));
        }

        
        private void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Color backgroundColor = GetBackgroundColor(index, isActive);
            EditorGUI.DrawRect(rect, backgroundColor);
        }

        
        private static Color GetBackgroundColor(int index, bool isActive)
        {
            if (isActive)
            {
                return new Color(0.5f, 0.5f, 0.5f, _BACKGROUND_OPACITY_ACTIVE);
            }
            
            if (index % 2 == 0)
            {
                return new Color(0.4f, 0.4f, 0.4f, _BACKGROUND_OPACITY_NORMAL);
            }
            else
            {
                return new Color(0.2f, 0.2f, 0.2f, _BACKGROUND_OPACITY_NORMAL);
            }
        }
    }
}