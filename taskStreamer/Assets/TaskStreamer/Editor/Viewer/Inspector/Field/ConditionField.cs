using System;
using System.Collections.Generic;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
	/// <summary> BBBasedConditionField는 시각적 요소로, 조건 필드에 관련된 UI를 정의합니다. </summary>
	internal class ConditionField : VisualElement
	{
		public ConditionField()
		{
			TSEditor.bbbConditionField.CloneTree(this);

			_headerFoldout = this.Q<Foldout>("main-header");
			_fieldListView = this.Q<ListView>("field-list");
			_enableToggle = this.Q<Toggle>("enable-toggle");
			_contentMask = this.Q<VisualElement>("content-mask");
			_conditionDeleteButton = this.Q<Button>("delete-button");
			_conditionDeleteButton.iconImage = TSEditor.deleteButton;

			_fieldListView.makeItem += () => new VisualElement();
			_fieldListView.bindItem += this.BindItem;

			_enableToggle.RegisterValueChangedCallback(this.UpdateServiceEnableState);
				
			_conditionDeleteButton.UnregisterCallback<ClickEvent>(this.OnDeleteButtonClicked);
			_conditionDeleteButton.RegisterCallback<ClickEvent>(this.OnDeleteButtonClicked);
		}


		/// <summary> 삭제 요청 이벤트를 발생시킵니다. </summary>
		public event Action<ConditionField> OnDeleteRequested;
		

		private readonly VisualElement _contentMask;


		/// _headerFoldout 변수는 UI의 Foldout 요소를 참조하며, 서비스 섹션의 확장/축소 상태를 관리합니다.
		private readonly Foldout _headerFoldout;


		/// <summary> 사용자가 정의한 VisualElement 리스트를 표시하는 ListView 객체로, 서비스 패널의 필드 목록을 관리합니다. </summary>
		private readonly ListView _fieldListView;


		/// 서비스 활성화를 제어하기 위한 토글 변수입니다.
		/// 값 변경 시 ServiceBase의 enable 속성에 반영됩니다.
		private readonly Toggle _enableToggle;


		/// <summary> 버튼을 클릭하여 조건을 삭제하는 기능을 제공하는 버튼입니다. </summary>
		private readonly Button _conditionDeleteButton;


		/// <summary>조건 처리에 사용되는 값을 저장합니다.</summary>
		private Condition _conditionValue;



		/// <summary> 현재 조건의 값을 반환합니다. </summary>
		public Condition conditionValueValue
		{
			get { return _conditionValue; }
		}



		private void BindItem(VisualElement element, int index)
		{
			VariableHandle handle = _conditionValue.variableHandles[index];

			if (element.childCount > 0)
			{
				element.Clear();
			}

			if (element.childCount == 0)
			{
				element.Add(TSVisualUtility.GetFieldByValueType(handle));
			}
		}



		/// <summary> Condition 데이터를 기반으로 UI와 값을 초기화합니다. </summary>
		/// <param name="condition"> 초기화에 사용될 Condition 객체입니다. </param>
		public void Setup(Condition condition)
		{
			this._conditionValue = condition;
			this.tooltip = condition.description;
			this._conditionDeleteButton.enabledSelf = TSEditor.canEditGraph;

			this._enableToggle.value = condition.enable;
			this._headerFoldout.value = condition.isExpanded;
			this._fieldListView.itemsSource = condition.variableHandles;
			this._headerFoldout.text = StringUtility.ToNicifyName(condition.name);
		}



		/// <summary> 삭제 버튼이 클릭되었을 때 호출됩니다. </summary>
		/// <param name="evt"> 클릭 이벤트를 나타내는 ClickEvent 객체입니다. </param>
		private void OnDeleteButtonClicked(ClickEvent evt)
		{
			if (TSEditor.canEditGraph == false || this._conditionValue is null)
			{
				return;
			}

			this.OnDeleteRequested?.Invoke(this);
		}
		
		
		
		private void UpdateServiceEnableState(ChangeEvent<bool> evt)
		{
			if (evt.newValue)
			{
				_contentMask.style.display = DisplayStyle.None;
			}
			else
			{
				_contentMask.style.display = DisplayStyle.Flex;
			}

			this._conditionValue.enable = evt.newValue;
			this._contentMask.enabledSelf = !evt.newValue;
		}
	}
}