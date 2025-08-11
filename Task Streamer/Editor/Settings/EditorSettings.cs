using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class EditorSettings : ScriptableObject
    {
        public Color miniMapBackgroundColor = new Color32(30, 30, 30, 255);
        public Color nodeGroupColor = new Color32(65, 65, 65, 255);
        
        [Header("View Color Options")]
        public Color nodeSuccessColor = new Color32(0, 100, 0, 255);
        public Color nodeFailureColor = new Color32(100, 0, 0, 255);
        
        [Space]
        public Gradient nodeStatusLinearColor;
        public Gradient edgeStatusLinearColor;
        
        [Header("Runtime Options")]
        public float highlightDuration = 0.5f;
        public float updateInterval = 0.0625f; // 625ms
        
        [Header("Layout References")]
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
