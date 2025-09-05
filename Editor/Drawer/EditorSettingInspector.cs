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
            
            
            
            VisualElement inspectorView = TaskStreamerResourceLoader.Settings.CloneTree();

            
            
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

        
        


        /// <summary>
        /// ListView 엘리먼트를 특정 index에 따라 바인딩합니다.
        /// </summary>
        /// <param name="element">바인딩할 ListView의 개별 요소입니다.</param>
        /// <param name="index">데이터 소스의 요소 인덱스입니다.</param>
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



        

        /// 태그 값이 변경될 때 호출되며, 해당 변경 사항을 EditorSettings에 기록합니다.
        /// <param name="evt">변경된 태그 값을 포함한 이벤트 데이터입니다.</param>
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


        


        /// <summary>
        /// 태그 리스트 아이템이 삭제되었을 때 호출되어 리스트를 업데이트합니다.
        /// </summary>
        /// <param name="listView">삭제 동작이 수행된 리스트 뷰</param>
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