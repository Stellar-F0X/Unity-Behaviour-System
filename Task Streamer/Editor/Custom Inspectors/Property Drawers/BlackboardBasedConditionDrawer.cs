using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TaskStreamer.Tool
{
    [CustomPropertyDrawer(typeof(BlackboardBasedCondition))]
    public class BlackboardBasedConditionDrawer : PropertyDrawer
    {
        private const int _PROPERTY_HEIGHT = 5;

        private SerializedProperty _serializedProperty;
        private SerializedProperty _listProperty;
        private SerializedObject _serializedObject;
        private ReorderableList _conditionList;

        private Rect _position;



        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _listProperty = property.FindPropertyRelative("modules");

            _serializedProperty = property;

            _position = position;

            if (SerializedProperty.DataEquals(_listProperty, null))
            {
                Debug.LogError("Condition list is null");
                return;
            }
            
            
            
            SerializedProperty completeTypeProp = property.FindPropertyRelative("evaluationPolicy");
            
            if (SerializedProperty.DataEquals(completeTypeProp, null))
            {
                Debug.LogError("Evaluation policy property is null");
                return;
            }
            
            Rect evaluationPolicyRect = new Rect(_position.x, _position.y, _position.width, EditorGUIUtility.singleLineHeight);
            
            EvaluationPolicy evaluationPolicy = (EvaluationPolicy)completeTypeProp.enumValueIndex;
            evaluationPolicy = (EvaluationPolicy)EditorGUI.EnumPopup(evaluationPolicyRect, completeTypeProp.displayName, evaluationPolicy);
            completeTypeProp.enumValueIndex = (int)evaluationPolicy;
            
            _position.y += EditorGUIUtility.singleLineHeight;
            
            
            
            this.DrawList();
            
            this.DrawAddConditionButton(new Rect(_position.width - 30, _position.y + 5, 30, EditorGUIUtility.singleLineHeight - 2));
            
            this.DrawRemoveConditionButton(new Rect(_position.width - 60, _position.y + 5, 30, EditorGUIUtility.singleLineHeight - 2));
        }


        private void DrawList()
        {
            this.CreateList();

            _conditionList.DoList(new Rect(_position.x, _position.y + 3, _position.width, _conditionList.GetHeight()));
        }

        
        private void CreateList()
        {
            if (_conditionList is null)
            {
                _conditionList = new ReorderableList(_listProperty.serializedObject, _listProperty, true, true, false, false);

                _conditionList.drawElementBackgroundCallback = this.DrawElementBackground;
                _conditionList.elementHeightCallback = this.GetElementHeight;
                _conditionList.drawElementCallback = this.DrawElement;
                _conditionList.drawHeaderCallback = this.DrawTransitionHeader;
                _conditionList.multiSelect = true;
                _conditionList.footerHeight = 10;
            }

            _conditionList.serializedProperty = _listProperty;
        }


        private void DrawAddConditionButton(Rect addButtonRect)
        {
            Texture addButtonImg = EditorGUIUtility.IconContent("CreateAddNew@2x").image;
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);

            if (GUI.Button(addButtonRect, addButtonImg, buttonStyle))
            {
                ICreationWindow window = CreationWindow.GetCreationWindow("Conditions", false);
                    
                if (window.modulesIsEmpty)
                {
                    window.AddFactoryModule(new ConditionFactoryModule(typeof(ConditionModule), "Conditions", 0));
                }

                window.RegisterCreationCallbackOnce((Action<ConditionModule>)this.AddElementToList);
                window.OpenWindow(addButtonRect.position);
            }
        }

        private void DrawRemoveConditionButton(Rect removeButtonRect)
        {
            Texture removeButtonImg = EditorGUIUtility.IconContent("d_Toolbar Minus@2x").image;
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);

            if (GUI.Button(removeButtonRect, removeButtonImg, buttonStyle))
            {
                if (_conditionList.selectedIndices == null || _conditionList.selectedIndices.Count == 0)
                {
                    return;
                }

                foreach (int index in _conditionList.selectedIndices.OrderByDescending(idx => idx))
                {
                    _listProperty.DeleteArrayElementAtIndex(index);
                }

                _conditionList.ClearSelection();
                _listProperty.serializedObject.ApplyModifiedProperties();
            }
        }


        private void AddElementToList(ConditionModule conditionModule)
        {
            int insertIndex = _listProperty.arraySize;
            _listProperty.InsertArrayElementAtIndex(insertIndex);

            SerializedProperty newProp = _listProperty.GetArrayElementAtIndex(insertIndex);
            newProp.managedReferenceValue = conditionModule;

            _listProperty.serializedObject.ApplyModifiedProperties();
            _listProperty = _serializedProperty.FindPropertyRelative("modules");

            this.CreateList();
            this.DrawList();
        }


        private void DrawTransitionHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Conditions");
        }


        private void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Color backgroundColor;

            // 선택된 상태일 때 더 명확한 색상 사용
            if (isActive && isFocused)
            {
                backgroundColor = new Color(0.3f, 0.5f, 0.85f, 0.8f); // 파란색 계열
            }
            else if (isActive)
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


        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = _listProperty.GetArrayElementAtIndex(index);

            if (SerializedProperty.DataEquals(element, null))
            {
                Debug.LogError("Failed to get the condition element at the specified index.");
                return;
            }

            rect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUI.GetPropertyHeight(element));

            EditorGUI.PropertyField(rect, element, new GUIContent($"Condition {index}"), true);
        }


        private float GetElementHeight(int index)
        {
            SerializedProperty element = _listProperty.GetArrayElementAtIndex(index);

            if (SerializedProperty.DataEquals(element, null))
            {
                Debug.LogError("Failed to get the condition element height at the specified index.");
                return 0;
            }

            return EditorGUI.GetPropertyHeight(element) + _PROPERTY_HEIGHT;
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            _listProperty = property.FindPropertyRelative("modules");

            this.CreateList();

            return _conditionList.GetHeight();
        }
    }
}