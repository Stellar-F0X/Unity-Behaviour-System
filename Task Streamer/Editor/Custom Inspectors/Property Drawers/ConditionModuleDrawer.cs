using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace TaskStreamer.Tool
{
    [CustomPropertyDrawer(typeof(ConditionModule), true)]
    public class ConditionModuleDrawer : PropertyDrawer
    {
        public ConditionModuleDrawer()
        {
            _popupStyle = new GUIStyle(EditorStyles.popup);
            _popupStyle.alignment = TextAnchor.MiddleCenter;
            _popupStyle.fontSize = 16;
        }

        private readonly GUIStyle _popupStyle;


        private Rect _fieldRect = Rect.zero;

        private Rect _variableARect = Rect.zero;

        private Rect _variableBRect = Rect.zero;

        private Rect _comparisonRect = Rect.zero;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (TaskStreamerEditor.Instance is null)
            {
                return;
            }

            this.CreateRects(position);

            SerializedProperty variableAProp = property.FindPropertyRelative("variableA");
            SerializedProperty variableBProp = property.FindPropertyRelative("variableB");
            SerializedProperty comparisonProp = property.FindPropertyRelative("comparison");

            EComparison comparisonValue = EComparison.None;

            if (property.managedReferenceValue is ConditionModule condition)
            {
                comparisonValue = condition.availableOperators;
            }
            else
            {
                Debug.LogError("Failed to get the condition module.");
            }

            if (comparisonValue == EComparison.None)
            {
                return;
            }
            
            EditorGUI.PropertyField(_variableARect, variableAProp, GUIContent.none);
            this.DrawComparisonOption(comparisonProp, comparisonValue);
            EditorGUI.PropertyField(_variableBRect, variableBProp, GUIContent.none);
        }


        private void CreateRects(Rect position)
        {
            float appliedOffsetWidth = position.width - 10;
            float comparisonWidth = 40f;
            float thirdWidth = appliedOffsetWidth / 3;
            float remainingWidth = (thirdWidth - comparisonWidth) / 2;
            float width = thirdWidth + remainingWidth;

            _fieldRect = new Rect(position.x, position.y + 2, appliedOffsetWidth, EditorGUIUtility.singleLineHeight);
            _variableARect = new Rect(_fieldRect.x, _fieldRect.y, width, _fieldRect.height);
            _variableBRect = new Rect(_fieldRect.x + width + comparisonWidth + 10, _fieldRect.y, width, _fieldRect.height);
            _comparisonRect = new Rect(_fieldRect.x + width + 5, _fieldRect.y, comparisonWidth, _fieldRect.height);
        }


        private void DrawComparisonOption(SerializedProperty conditionType, EComparison comparisonType)
        {
            List<int> conditionIndex = ListPool<int>.Get();

            string[] conditionTypes = this.GetComparisonOptionSymbols(comparisonType, conditionIndex);

            if (conditionTypes is null || conditionTypes.Length == 0)
            {
                return;
            }

            int prev = Mathf.Max(conditionIndex.IndexOf(conditionType.enumValueFlag), 0);
            int index = EditorGUI.Popup(_comparisonRect, prev, conditionTypes, _popupStyle);
            conditionType.enumValueFlag = conditionIndex[index];

            ListPool<int>.Release(conditionIndex);
        }


        private string[] GetComparisonOptionSymbols(EComparison comparisonValue, List<int> resultIndices)
        {
            List<string> conditionTypes = ListPool<string>.Get();

            for (int index = (int)EComparison.Equal; index <= (int)comparisonValue; index <<= 1)
            {
                EComparison currentComparisonValue = (EComparison)index;

                if ((currentComparisonValue & comparisonValue) == currentComparisonValue)
                {
                    resultIndices.Add(index);
                    conditionTypes.Add(this.GetSymbol(currentComparisonValue));
                }
            }

            string[] resultSymbols = conditionTypes.ToArray();
            ListPool<string>.Release(conditionTypes);
            return resultSymbols;
        }


        private string GetSymbol(EComparison comparison)
        {
            switch (comparison)
            {
                case EComparison.Equal: return "=";

                case EComparison.NotEqual: return "≠";

                case EComparison.GreaterThan: return ">";

                case EComparison.GreaterThanOrEqual: return "≥";

                case EComparison.LessThan: return "<";

                case EComparison.LessThanOrEqual: return "≤";

                default: throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
            }
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4;
        }
    }
}