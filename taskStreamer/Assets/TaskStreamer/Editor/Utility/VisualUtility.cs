using System;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TaskStreamer.Tool
{
    public static class VisualUtility
    {
        public static void SetBorderColor(this IStyle elementStyle, Color color)
        {
            elementStyle.borderTopColor = color;
            elementStyle.borderBottomColor = color;
            elementStyle.borderLeftColor = color;
            elementStyle.borderRightColor = color;
        }


        public static void SetEdgeColor(this Edge edge, Color color)
        {
            edge.edgeControl.inputColor = color;
            edge.edgeControl.outputColor = color;
        }
        
        
        
        public static void DrawError(in Rect rect, in string message, in float iconSize = 12f)
        {
            Rect iconRect = new Rect(rect.x, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
            Rect textRect = new Rect(rect.x + iconSize + 2f, rect.y, rect.width - iconSize - 2f, rect.height);

            Texture warningImg = EditorGUIUtility.IconContent("console.warnicon").image;
            GUI.DrawTexture(iconRect, warningImg, ScaleMode.ScaleToFit);

            EditorGUI.LabelField(textRect, message);
        }


        public static TNode GetNodeByView<TNode>(this Node node) where TNode : NodeBase
        {
            if (node is NodeViewBase nodeView && nodeView.targetNode is TNode result)
            {
                return result;
            }
            else
            {
                Debug.LogError("Failed to convert the node view to the specified node type");
                return null;
            }
        }


        public static VisualElement GetFieldByValueType(VariableHandle context)
        {
            Type type = context.GetValue<BlackboardVariable>()?.valueType;

            if (type == null) 
            {
                return new UnsupportedTypeField(context.context);
            }

            if (type == typeof(float))
            {
                return new BlackboardVariableField<float, FloatField>(context);
            }

            if (type == typeof(double))
            {
                return new BlackboardVariableField<double, DoubleField>(context);
            }

            if (type == typeof(int))
            {
                return new BlackboardVariableField<int, IntegerField>(context);
            }

            if (type == typeof(bool))
            {
                return new BlackboardVariableField<bool, Toggle>(context);
            }

            if (type == typeof(string))
            {
                return new BlackboardVariableField<string, TextField>(context);
            }

            if (type.IsEnum)
            {
                if (type.GetAttribute<FlagsAttribute>() is null)
                {
                    return new BlackboardVariableEnumField<EnumField>(context);
                }
                else
                {
                    return new BlackboardVariableEnumField<EnumFlagsField>(context);
                }
            }

            if (type == typeof(Vector2))
            {
                return new BlackboardVariableField<Vector2, Vector2Field>(context);
            }

            if (type == typeof(Vector3))
            {
                return new BlackboardVariableField<Vector3, Vector3Field>(context);
            }

            if (type == typeof(Quaternion))
            {
                return new BlackboardVariableField<Quaternion, QuaternionField>(context);
            }
            
            if (type == typeof(Vector4))
            {
                return new BlackboardVariableField<Vector4, Vector4Field>(context);
            }

            if (type == typeof(Vector2Int))
            {
                return new BlackboardVariableField<Vector2Int, Vector2IntField>(context);
            }

            if (type == typeof(Vector3Int))
            {
                return new BlackboardVariableField<Vector3Int, Vector3IntField>(context);
            }

            if (type == typeof(Color))
            {
                return new BlackboardVariableField<Color, ColorField>(context);
            }

            bool isComponentOrObject = typeof(GameObject).IsAssignableFrom(type);
            isComponentOrObject |= typeof(ScriptableObject).IsAssignableFrom(type);
            isComponentOrObject |= typeof(MonoBehaviour).IsAssignableFrom(type);
            isComponentOrObject |= typeof(Object).IsAssignableFrom(type);

            if (isComponentOrObject)
            {
                var objectField = new BlackboardVariableField<Object, ObjectField>(context);
                ObjectField localObjectField = objectField.localVariableInputField;
                localObjectField.allowSceneObjects = false;
                localObjectField.objectType = type;
                localObjectField.label = "";
                return objectField;
            }

            throw new ArgumentException($"Unsupported value type: {type.Name}");
        }
    }
}