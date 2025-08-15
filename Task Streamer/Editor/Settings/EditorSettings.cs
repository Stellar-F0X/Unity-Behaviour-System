using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    //TODO: 주석 달아야 됨.
    public class EditorSettings : ScriptableObject
    {
        public Color minimapColor = new Color32(30, 30, 30, 255);
        public Color nodeGroupColor = new Color32(65, 65, 65, 255);
        public Color successNodeColor = new Color32(0, 100, 0, 255);
        public Color failureNodeColor = new Color32(100, 0, 0, 255);
        
        public Gradient nodeStatusGradient;
        public Gradient edgeStatusGradient;
        
        public float highlightDuration = 0.5f;
        public uint updatesPerSecond = 10;
        
        public float updateInterval = 0.1f;
        public float durationReciprocal = 2f;
        
        
        // UI Toolkit assets for the editor
        public VisualTreeAsset editorXml;
        public StyleSheet editorStyle;
        public VisualTreeAsset behaviorNodeViewXml;
        public StyleSheet behaviorNodeViewStyle;
        public VisualTreeAsset stateNodeViewXml;
        public StyleSheet stateNodeViewStyle;
        public VisualTreeAsset blackboardVariableViewXml;
        public StyleSheet blackboardVariableViewStyle;
        public VisualTreeAsset editorSettingsXml;
        public StyleSheet editorSettingsStyle;
        public StyleSheet transitionEdgeViewStyle;
    }
}
