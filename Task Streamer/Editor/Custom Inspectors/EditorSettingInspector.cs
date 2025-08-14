using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    //TODO: Settings에 코드 제네레이션할 타입들을 저장할건데, 그걸 여기서 바인딩하는 로직을 추가할거라, 굳이 UI Toolkit을 사용했음.  
    
    //TODO: Undo 동작 안되는 버그 수정해야 됨.
    [CustomEditor(typeof(EditorSettings))]
    public class EditorSettingInspector : Editor
    {
        private EditorSettings _settings;
        
        public override VisualElement CreateInspectorGUI()
        {
            this._settings = (EditorSettings)target;
            VisualElement inspectorView = _settings.editorSettingsXml.CloneTree();

            UnsignedIntegerField updateRate = inspectorView.Q<UnsignedIntegerField>("update-rate");
            FloatField highlightDuration = inspectorView.Q<FloatField>("highlight-duration");
            
            ColorField miniMapColor = inspectorView.Q<ColorField>("minimap-color");
            ColorField nodeGroupColor = inspectorView.Q<ColorField>("node-group-color");
            ColorField nodeSuccessColor = inspectorView.Q<ColorField>("node-success-color");
            ColorField nodeFailureColor = inspectorView.Q<ColorField>("node-failure-color");
            GradientField nodeGradientField = inspectorView.Q<GradientField>("node-highlight-color");
            GradientField edgeGradientField = inspectorView.Q<GradientField>("edge-highlight-color");

            Debug.Assert(updateRate != null, "Update Rate field is null");
            updateRate.value = _settings.updatesPerSecond; 
            
            Debug.Assert(highlightDuration != null, "Highlight Duration field is null");
            highlightDuration.value = _settings.highlightDuration;
            
            Debug.Assert(miniMapColor != null, "Mini Map Color field is null");
            nodeSuccessColor.value = _settings.successNodeColor;
            
            Debug.Assert(nodeSuccessColor != null, "Node Success Color field is null");
            nodeFailureColor.value = _settings.failureNodeColor;
            
            Debug.Assert(nodeFailureColor != null, "Node Failure Color field is null");
            nodeGradientField.value = _settings.nodeStatusGradient;
            
            Debug.Assert(nodeGradientField != null, "Node Gradient field is null");
            edgeGradientField.value = _settings.edgeStatusGradient;
            
            Debug.Assert(edgeGradientField != null, "Edge Gradient field is null");
            nodeGroupColor.value = _settings.nodeGroupColor;
            
            Debug.Assert(nodeGroupColor != null, "Node Group Color field is null");
            miniMapColor.value = _settings.minimapColor;


            updateRate.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(_settings, "Change Editor Setting");
                _settings.updatesPerSecond = (uint)Mathf.Clamp(evt.newValue, 1, 30);
                _settings.updateInterval = 1f / _settings.updatesPerSecond;
                EditorUtility.SetDirty(_settings);
            });

            highlightDuration.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(_settings, "Change Editor Setting");
                _settings.highlightDuration = Mathf.Max(evt.newValue, 0f);
                _settings.durationReciprocal = 1f / _settings.highlightDuration;
                EditorUtility.SetDirty(_settings);
            });

            miniMapColor.RegisterValueChangedCallback(evt => this.Apply(evt, ref _settings.minimapColor));

            nodeGroupColor.RegisterValueChangedCallback(evt => this.Apply(evt, ref _settings.nodeGroupColor));

            nodeSuccessColor.RegisterValueChangedCallback(evt => this.Apply(evt, ref _settings.successNodeColor));

            nodeFailureColor.RegisterValueChangedCallback(evt => this.Apply(evt, ref _settings.failureNodeColor));

            nodeGradientField.RegisterValueChangedCallback(e => this.Apply(e, ref _settings.nodeStatusGradient));

            edgeGradientField.RegisterValueChangedCallback(e => this.Apply(e, ref _settings.edgeStatusGradient));

            return inspectorView;
        }
        
        
        private void Apply<T>(ChangeEvent<T> newValue, ref T value)
        {
            Undo.RecordObject(_settings, "Change Editor Setting");
            
            value = newValue.newValue;
            
            EditorUtility.SetDirty(_settings);
        }
    }
}