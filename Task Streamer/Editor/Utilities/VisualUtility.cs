using System;
using TaskStreamer.Injection;
using TaskStreamer.Utility;
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
            Debug.Log(context.fieldType);
            Type type = context.fieldType.GenericTypeArguments[0];

            if (type == typeof(float))
            {
                return new BBVariableField<float, FloatField>(context);
            }

            if (type == typeof(double))
            {
                return new BBVariableField<double, DoubleField>(context);
            }

            if (type == typeof(int))
            {
                return new BBVariableField<int, IntegerField>(context);
            }

            if (type == typeof(bool))
            {
                return new BBVariableField<bool, Toggle>(context);
            }

            if (type == typeof(string))
            {
                return new BBVariableField<string, TextField>(context);
            }

            if (type.IsEnum)
            {
                var enumField = new BBVariableField<Enum, EnumField>(context);
                enumField.variableField.Init(Activator.CreateInstance(type) as Enum);
                return enumField;
            }

            if (type == typeof(Vector2))
            {
                return new BBVariableField<Vector2, Vector2Field>(context);
            }

            if (type == typeof(Vector3))
            {
                return new BBVariableField<Vector3, Vector3Field>(context);
            }

            if (type == typeof(Quaternion))
            {
                return new BBVariableField<Quaternion, QuaternionField>(context);
            }
            
            if (type == typeof(Vector4))
            {
                return new BBVariableField<Vector4, Vector4Field>(context);
            }

            if (type == typeof(Vector2Int))
            {
                return new BBVariableField<Vector2Int, Vector2IntField>(context);
            }

            if (type == typeof(Vector3Int))
            {
                return new BBVariableField<Vector3Int, Vector3IntField>(context);
            }

            if (type == typeof(Color))
            {
                return new BBVariableField<Color, ColorField>(context);
            }

            if (typeof(GameObject).IsAssignableFrom(type))
            {
                var objectField = new BBVariableField<Object, ObjectField>(context);
                objectField.variableField.allowSceneObjects = true;
                objectField.variableField.objectType = type;
                objectField.variableField.label = "";
                return objectField;
            }

            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                var objectField = new BBVariableField<Object, ObjectField>(context);
                objectField.variableField.allowSceneObjects = false;
                objectField.variableField.objectType = type;
                objectField.variableField.label = "";
                return objectField;
            }

            if (typeof(Object).IsAssignableFrom(type))
            {
                var objectField = new BBVariableField<Object, ObjectField>(context);
                objectField.variableField.allowSceneObjects = false;
                objectField.variableField.objectType = type;
                objectField.variableField.label = "";
                return objectField;
            }

            return new UnsupportedTypeField(context.context);
        }
    }
}