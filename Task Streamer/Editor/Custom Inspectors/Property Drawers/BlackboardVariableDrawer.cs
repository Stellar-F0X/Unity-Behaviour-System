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
        
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (TaskStreamerEditor.Instance is null || TaskStreamerEditor.Instance.graphAsset == null)
            {
                return;
            }

            property.serializedObject.Update();
            
            this.CreateRects(position, label);

            using (new EditorGUI.DisabledScope(!TaskStreamerEditor.canEditGraph))
            {
                SerializedProperty isGlobalProp = property.FindPropertyRelative("_isGlobal");
                bool previous = isGlobalProp.boolValue;
                isGlobalProp.boolValue = EditorGUI.Toggle(_buttonRect, isGlobalProp.boolValue, "radio");
                bool isChanged = previous != isGlobalProp.boolValue;

                EditorGUI.PrefixLabel(_labelRect, label);

                if (isGlobalProp.boolValue)
                {
                    this.DrawBlackboardVariablePopup(property, TaskStreamerEditor.Instance.graphAsset.blackboard);
                }
                else
                {
                    this.DrawLocalVariableField(property, _fieldRect, isChanged);
                }
            }

            property.serializedObject.ApplyModifiedProperties();
        }
        

        private void DrawBlackboardVariablePopup(SerializedProperty property, BlackboardAsset blackboard)
        {
            //만약 Blackboard가 null이거나, 블랙보드에 Variable이 없다면 에러 메시지를 표시한다.
            if (blackboard == null || blackboard.count == 0)
            {
                EditorGUI.PrefixLabel(_labelRect, new GUIContent(property.displayName));
                EditorUtilities.DrawError(_fieldRect, "No blackboard variables found.");
                return;
            }

            // 현재 프로퍼티 타입과 호환되는 변수들만 필터링
            Type variableType = typeof(Variable<>).MakeGenericType(fieldInfo.FieldType.GenericTypeArguments[0]);
            Variable[] variables = blackboard.GetVariablesByType(variableType);

            if (variables.Length == 0)
            {
                EditorUtilities.DrawError(_fieldRect, "No assignable blackboard variables found.");
                return;
            }

            property.serializedObject.ApplyModifiedProperties();

            string[] options = this.GetPopupOptions(variables);

            SerializedProperty variableProp = property.FindPropertyRelative("_variable");

            // 현재 선택된 인덱스 계산
            int selectedIndex = this.CalculateSelectIndex(variableProp, options);
            int newSelectedIndex = EditorGUI.Popup(_fieldRect, selectedIndex, options);

            if (newSelectedIndex == selectedIndex)
            {
                return;
            }

            if (newSelectedIndex == 0)
            {
                this.AllocateVariable(variableProp);
            }
            else
            {
                variableProp.managedReferenceValue = variables[newSelectedIndex - 1];
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
            
            SerializedProperty valueProperty = variableProp.FindPropertyRelative("_value");

            if (isChanged || SerializedProperty.DataEquals(valueProperty, null))
            {
                this.AllocateVariable(variableProp);
            }

            // 로컬 변수 값 필드 그리기
            EditorGUI.PropertyField(fieldRect, valueProperty, GUIContent.none, true);
        }

        
        private void AllocateVariable(SerializedProperty variableProp)
        {
            Type variableType = typeof(Variable<>).GetImplementedType(fieldInfo.FieldType.GenericTypeArguments[0]);
            
            variableProp.managedReferenceValue = Utility.Utilities.CreateVariable(variableType, true);
        }


        private string[] GetPopupOptions(Variable[] variables)
        {
            // 팝업 옵션 생성 (None + 변수 이름들)
            List<string> optionsList = ListPool<string>.Get();

            optionsList.Add("None");

            foreach (Variable variable in variables)
            {
                optionsList.Add(variable.key);
            }

            string[] options = optionsList.ToArray();
            ListPool<string>.Release(optionsList);
            return options;
        }


        private int CalculateSelectIndex(SerializedProperty property, string[] options)
        {
            int selectedIndex = 0;
            SerializedProperty nameProperty = property.FindPropertyRelative("_key");

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