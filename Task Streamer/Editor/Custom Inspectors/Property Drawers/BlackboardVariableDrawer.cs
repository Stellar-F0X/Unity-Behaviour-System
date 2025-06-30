using System;
using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace TaskStreamer.Tool
{
    [CustomPropertyDrawer(typeof(BlackboardVariable), true)]
    public class BlackboardVariableDrawer : PropertyDrawer
    {
        private Rect _labelRect = Rect.zero;

        private Rect _fieldRect = Rect.zero;

        private Rect _buttonRect = Rect.zero;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (TaskStreamerEditor.Instance is null)
            {
                return;
            }

            Blackboard blackboard = TaskStreamerEditor.Instance.graphAsset.blackboard;

            this.CreateRects(position, label);
            
            if (blackboard is null)
            {
                EditorGUI.PrefixLabel(_labelRect, label);
                EditorHelper.DrawError(_fieldRect, "Blackboard component is NullReference");
                return;
            }

            using (new EditorGUI.DisabledScope(!TaskStreamerEditor.canEditGraph))
            {
                SerializedProperty isGlobalProp = property.FindPropertyRelative("_isGlobal");
                bool previous = isGlobalProp.boolValue; 
                isGlobalProp.boolValue = EditorGUI.Toggle(_buttonRect, isGlobalProp.boolValue, "radio");
                bool isChanged = previous != isGlobalProp.boolValue;

                EditorGUI.PrefixLabel(_labelRect, label);

                if (isGlobalProp.boolValue)
                {
                    this.DrawBlackboardVariablePopup(property, blackboard);
                }
                else
                {
                    this.DrawLocalVariableField(property, _fieldRect, isChanged);
                }
            }

            property.serializedObject.ApplyModifiedProperties();
        }


        private void CreateRects(Rect position, GUIContent label)
        {
            float width = EditorGUIUtility.labelWidth;
            float height = EditorGUIUtility.singleLineHeight;

            if (label == GUIContent.none)
            {
                _labelRect = Rect.zero;
                _fieldRect = new Rect(position.x, position.y, position.width - 20, height);
            }
            else
            {
                _labelRect = new Rect(position.x, position.y, width, height);
                _fieldRect = new Rect(position.x + width + 2, position.y, position.width - width - 20, height);
            }

            _buttonRect = new Rect(position.x + position.width - 16, position.y, 16, height);
        }


        private void DrawBlackboardVariablePopup(SerializedProperty property, Blackboard blackboard)
        {
            if (blackboard == null || blackboard.variables.Count == 0)
            {
                EditorHelper.DrawError(_fieldRect, "No blackboard variables found.");
                return;
            }

            // 현재 프로퍼티 타입과 호환되는 변수들만 필터링
            Type variableType = typeof(Variable<>).MakeGenericType(fieldInfo.FieldType.GenericTypeArguments[0]);
            Variable[] variables = blackboard.GetVariablesByType(variableType);

            if (variables.Length == 0)
            {
                EditorHelper.DrawError(_fieldRect, "No assignable blackboard variables found.");
                return;
            }

            property.serializedObject.ApplyModifiedProperties();
            
            string[] options = this.GetPopupOptions(variables);

            SerializedProperty variableProp = property.FindPropertyRelative("_variable");
            
            // 현재 선택된 인덱스 계산
            int selectedIndex = this.CalculateSelectIndex(variableProp, options);
            int newSelectedIndex = EditorGUI.Popup(_fieldRect, selectedIndex, options);

            if (newSelectedIndex != selectedIndex)
            {
                if (newSelectedIndex == 0)
                {
                    var typeCollection = TypeCache.GetTypesDerivedFrom(variableType);
                    variableProp.managedReferenceValue = Variable.Create(typeCollection[0], true);
                }
                else
                {
                    variableProp.managedReferenceValue = variables[newSelectedIndex - 1];
                }
            }
        }


        private void DrawLocalVariableField(SerializedProperty property, Rect fieldRect, bool isChanged)
        {
            SerializedProperty variableProp = property.FindPropertyRelative("_variable");

            if (SerializedProperty.DataEquals(variableProp, null))
            {
                Debug.LogError("Variable serialized property is null.");
                return;
            }

            if (isChanged)
            {
                Type variableType = Utility.Helper.GetImplementedType(typeof(Variable<>), fieldInfo.FieldType.GenericTypeArguments[0]);
                variableProp.managedReferenceValue = Variable.Create(variableType, true);
            }

            SerializedProperty valueProperty = variableProp.FindPropertyRelative("_value");

            if (SerializedProperty.DataEquals(valueProperty, null))
            {
                Debug.LogError("Value serialized property is null.");
                return;
            }

            // 로컬 변수 값 필드 그리기
            EditorGUI.PropertyField(fieldRect, valueProperty, GUIContent.none, true);
        }


        private string[] GetPopupOptions(Variable[] variables)
        {
            // 팝업 옵션 생성 (None + 변수 이름들)
            List<string> optionsList = ListPool<string>.Get();

            optionsList.Add("None");

            foreach (Variable variable in variables)
            {
                optionsList.Add(variable.name);
            }

            string[] options = optionsList.ToArray();
            ListPool<string>.Release(optionsList);
            return options;
        }


        private int CalculateSelectIndex(SerializedProperty property, string[] options)
        {
            int selectedIndex = 0;
            SerializedProperty nameProperty = property.FindPropertyRelative("_name");

            if (SerializedProperty.DataEquals(nameProperty, null))
            {
                return 0;
            }

            if (string.IsNullOrEmpty(nameProperty.stringValue) == false)
            {
                int valueIndex = Array.IndexOf(options, nameProperty.stringValue) - 1;
                selectedIndex = valueIndex >= 0 ? valueIndex + 1 : 0;
            }

            return Mathf.Clamp(selectedIndex, 0, options.Length - 1);
        }
    }
}