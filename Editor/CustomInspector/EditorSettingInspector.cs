using UnityEditor;
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
            VisualElement inspectorView = TaskStreamerResourcesLoader.Settings.CloneTree();

            UnsignedIntegerField updateRate = inspectorView.Q<UnsignedIntegerField>("update-rate");
            FloatField highlightDuration = inspectorView.Q<FloatField>("highlight-duration");
            ListView tagListView = inspectorView.Q<ListView>("tag-list");

            Debug.Assert(updateRate != null, "Update Rate field is null");
            updateRate.value = _settings.updatesPerSecond; 
            
            Debug.Assert(highlightDuration != null, "Highlight Duration field is null");
            highlightDuration.value = _settings.highlightDuration;
            
            Debug.Assert(tagListView != null, "Tag List View is null");
            tagListView.itemsSource = _settings.tagList;
            tagListView.makeItem = () => new TextField();
            tagListView.bindItem = this.OnBindTagListElement;
            tagListView.onRemove = this.OnRemovedTagListItem;

            updateRate.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(_settings, "Change Editor Setting");
                _settings.updatesPerSecond = (uint)Mathf.Clamp(evt.newValue, 1, 30);
                _settings.updateInterval = 1f / _settings.updatesPerSecond;
                UnityEditor.EditorUtility.SetDirty(_settings);
            });

            highlightDuration.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(_settings, "Change Editor Setting");
                _settings.highlightDuration = Mathf.Max(evt.newValue, 0f);
                _settings.durationReciprocal = 1f / _settings.highlightDuration;
                UnityEditor.EditorUtility.SetDirty(_settings);
            });

            return inspectorView;
        }


        private void OnBindTagListElement(VisualElement element, int index)
        {
            if (element is not TextField textField)
            {
                return;
            }

            textField.label = $"Tag ({index})";
            textField.value = _settings.tagList[index];
            textField.enabledSelf = index == 0 ? false : true;
            
            textField.AddToClassList("tag-list-element");
            textField.UnregisterValueChangedCallback(this.OnTagValueChanged);
            textField.RegisterValueChangedCallback(this.OnTagValueChanged);
        }

        
        private void OnTagValueChanged(ChangeEvent<string> evt)
        {
            int index = _settings.tagList.IndexOf(evt.previousValue);

            if (index == -1)
            {
                return;
            }
            
            Undo.RecordObject(_settings, "Change Tag List");
            _settings.tagList[index] = evt.newValue;
            UnityEditor.EditorUtility.SetDirty(_settings);
        }


        private void OnRemovedTagListItem(BaseListView listView)
        {
            bool completelyDeleted = false;
            
            foreach (int index in listView.selectedIndices)
            {
                if (index != 0 && listView.itemsSource.Count > index)
                {
                    listView.itemsSource.RemoveAt(index);
                    completelyDeleted = true;
                }
            }

            if (completelyDeleted)
            {
                listView.RefreshItems();
            }
        }
    }
}