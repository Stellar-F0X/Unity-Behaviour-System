using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    //TODO: 주석 달아야 됨.
    public class EditorSettings : ScriptableObject
    {
        public float highlightDuration = 0.5f;
        public uint updatesPerSecond = 10;
        
        public float updateInterval = 0.1f;
        public float durationReciprocal = 2f;

        public List<string> tagList = new List<string>() { "None" };
        
        public Color minimapColor = new Color32(30, 30, 30, 255);
        public Color nodeGroupColor = new Color32(65, 65, 65, 255);
        public Color successNodeColor = new Color32(0, 100, 0, 255);
        public Color failureNodeColor = new Color32(100, 0, 0, 255);
        
        public Gradient nodeStatusGradient;
        public Gradient edgeStatusGradient;
        


#region UI Toolkit Templates
        // UI Toolkit assets for the editor
        public VisualTreeAsset editorXml;
        public StyleSheet editorStyle;
        
        public VisualTreeAsset behaviorNodeXml;
        public StyleSheet behaviorNodeStyle;
        
        public VisualTreeAsset stateNodeXml;
        public StyleSheet stateNodeStyle;
        
        public VisualTreeAsset bbVariableXml;
        public StyleSheet bbVariableStyle;
        
        public VisualTreeAsset editorSettingsXml;
        public StyleSheet editorSettingsStyle;
        
        public StyleSheet EdgeStyle;

        public VisualTreeAsset nodeInspectorXml;
        public StyleSheet inspectorStyle;

        public VisualTreeAsset bbVariableFieldXml;

        public VisualTreeAsset bbBasedConditionFieldXml;
        public StyleSheet bbBasedConditionFieldStyle;

        public VisualTreeAsset bbBasedConditionListFieldXml;
        public StyleSheet bbBasedConditionListStyle;
#endregion 
    }
}
