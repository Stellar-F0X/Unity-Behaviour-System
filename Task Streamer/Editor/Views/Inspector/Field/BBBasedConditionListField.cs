using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class BBBasedConditionListField : VisualElement
    {
        public BBBasedConditionListField(string context, BlackboardBasedCondition bbCondition)
        {
            TaskStreamerEditor.settings.bbBasedConditionListFieldXml.CloneTree(this);

            _bbCondition = bbCondition;

            _conditionListView = this.Q<ListView>("condition-list-view");
            _conditionDeleteBtn = this.Q<Button>("condition-delete-btn");

            _conditionListView.headerTitle = context;
            _conditionListView.itemsSource = bbCondition.modules;
            _conditionListView.bindItem = this.BindConditionItem;
            _conditionListView.makeItem = () => new BBBasedConditionField();

            _conditionDeleteBtn.clickable.clickedWithEventInfo -= this.OnAddButtonClicked;
            _conditionDeleteBtn.clickable.clickedWithEventInfo += this.OnAddButtonClicked;
        }


        private readonly BlackboardBasedCondition _bbCondition;

        private readonly ListView _conditionListView;
        private readonly Button _conditionDeleteBtn;


        private void OnAddButtonClicked(EventBase evt)
        {
            ICreationWindow window = CreationWindow.GetCreationWindow("Conditions", false);

            if (window.modulesIsEmpty)
            {
                window.AddFactoryModule(new ConditionFactoryModule(typeof(Condition), "Conditions", 0));
            }

            window.RegisterCreationCallbackOnce((Action<Condition>)this.AddItemToList);
            window.OpenWindow(evt.originalMousePosition);
        }


        private void BindConditionItem(VisualElement element, int index)
        {
            BBBasedConditionField conditionField = element as BBBasedConditionField;

            Debug.Assert(conditionField is not null, "conditionField is null");
            
            conditionField.OnDeleteRequested -= this.OnVariableDeleteRequested;
            conditionField.OnDeleteRequested += this.OnVariableDeleteRequested;

            conditionField.Setup(this._bbCondition.modules[index]);
        }
        
        
        private void AddItemToList(Condition condition)
        {
            Undo.RecordObject(TaskStreamerEditor.Instance.graphAsset, "TaskStreamer (AddBBBasedCondition)");
            
            _conditionListView.itemsSource.Add(condition);
            _conditionListView.RefreshItems();

            UnityEditor.EditorUtility.SetDirty(TaskStreamerEditor.Instance.graphAsset);
        }


        private void OnVariableDeleteRequested(BBBasedConditionField variableView)
        {
            int index = _conditionListView.itemsSource.IndexOf(variableView.conditionValueValue);

            if (index < 0 || index >= _conditionListView.itemsSource.Count)
            {
                return;
            }
            
            Undo.RecordObject(TaskStreamerEditor.Instance.graphAsset, "TaskStreamer (RemoveBBBasedCondition)");
            
            _conditionListView.itemsSource.RemoveAt(index);
            _conditionListView.RefreshItems();
            
            UnityEditor.EditorUtility.SetDirty(TaskStreamerEditor.Instance.graphAsset);
        }
    }
}