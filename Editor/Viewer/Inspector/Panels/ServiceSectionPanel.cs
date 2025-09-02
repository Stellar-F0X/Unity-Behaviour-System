using System;
using System.Collections.Generic;
using System.Linq;
using TaskStreamer.BT;
using TaskStreamer.Utility;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary> ServiceSectionPanel 클래스는 Unity UIElements의 VisualElement를 확장하여 서비스 항목을 표시, 설정, 삭제 등의 UI 기능을 제공하는 패널입니다. </summary>
    public class ServiceSectionPanel : VisualElement, IRefreshablePanel
    {
        public ServiceSectionPanel()
        {
            TaskStreamerResourcesLoader.ServiceSectionPanel.CloneTree(this);

            _headerFoldout = this.Q<Foldout>("main-header"); 
            _fieldListView = this.Q<ListView>("field-list"); 
            _enableToggle = this.Q<Toggle>("enable-toggle"); 
            _deleteButton = this.Q<Button>("delete-button"); 

            _fieldListView.makeItem += () => new VisualElement();
            _fieldListView.bindItem += this.BindItem;
            _deleteButton.clicked += this.OnDeleteButtonClicked;
            _deleteButton.enabledSelf = TaskStreamerEditor.canEditGraph;

            _enableToggle.RegisterValueChangedCallback(evt => service.enable = evt.newValue); 
            _headerFoldout.RegisterValueChangedCallback(evt => service.isExpanded = evt.newValue); 
        }

        
        /// 삭제 요청 이벤트를 처리하기 위한 변수입니다.
        /// ServiceSectionPanel에서 삭제 버튼 클릭 시 호출됩니다.
        public event Action<ServiceSectionPanel> OnDeleteRequested;

        
        /// _headerFoldout 변수는 UI의 Foldout 요소를 참조하며, 서비스 섹션의 확장/축소 상태를 관리합니다.
        private readonly Foldout _headerFoldout;

        
        /// <summary> 사용자가 정의한 VisualElement 리스트를 표시하는 ListView 객체로, 서비스 패널의 필드 목록을 관리합니다. </summary>
        private readonly ListView _fieldListView;


        /// 서비스 활성화를 제어하기 위한 토글 변수입니다.
        /// 값 변경 시 ServiceBase의 enable 속성에 반영됩니다.
        private readonly Toggle _enableToggle;

        
        /// 사용자 인터페이스의 삭제 버튼을 나타내는 변수로, 버튼 클릭 시 삭제 이벤트를 처리합니다.
        private readonly Button _deleteButton;


        /// <summary>
        /// ServiceSectionPanel에서 사용되는 변수 핸들(VariableHandle)의 목록을 저장하는 필드입니다.
        /// 패널의 데이터 바인딩 및 UI 갱신에 사용됩니다.
        /// </summary>
        private List<VariableHandle> _variableHandles;

        
        /// _service는 ServiceSectionPanel 내부에서 사용되는 ServiceBase 타입의 비공개 멤버 변수로,
        /// 패널과 연결된 서비스 데이터를 참조하며, 서비스의 상태와 설정을 반영합니다.
        private ServiceBase _service;
        

        /// ServiceSectionPanel의 서비스 속성으로, 할당된 ServiceBase 객체를 반환합니다.
        /// 서비스 데이터와 UI 간 상호작용을 지원합니다.
        public ServiceBase service
        {
            get { return _service; }
        }
        

        
        /// <summary>
        /// 삭제 버튼 클릭 시 호출되어 삭제 요청 이벤트를 발생시킵니다.
        /// _service가 null이거나 그래프 편집이 불가능할 경우 동작하지 않습니다.
        /// </summary>
        private void OnDeleteButtonClicked()
        {
            if (TaskStreamerEditor.canEditGraph == false || _service is null)
            {
                return;
            }

            OnDeleteRequested?.Invoke(this);
        }
        
        

        /// <summary> RefreshPanel 메서드는 패널의 ListView와 같은 UI 요소를 동적으로 갱신하는 역할을 수행합니다. </summary>
        public void RefreshPanel()
        {
            _fieldListView.RefreshItems();
        }

        
        
        /// <summary> 서비스 섹션 패널의 UI 요소들을 주어진 서비스와 연결하고 초기화합니다. </summary>
        /// <param name="newService">UI에 연결할 새로운 서비스 객체입니다.</param>
        /// <param name="variableHandles">서비스의 변수들에 대한 참조를 담은 핸들 목록입니다.</param>
        public void Initialize(ServiceBase newService, List<VariableHandle> variableHandles)
        {
            _service = newService;
            _headerFoldout.text = StringUtility.ToNicifyName(newService.name);
            _variableHandles = variableHandles;
            _enableToggle.value = newService.enable;
            _headerFoldout.value = newService.isExpanded;
            _fieldListView.itemsSource = _variableHandles;
        }



        /// <summary> ListView 아이템을 주어진 데이터와 연결하고 필요 시 새 VisualElement를 생성 및 갱신합니다. </summary>
        /// <param name="element">연결 또는 갱신할 VisualElement입니다.</param>
        /// <param name="index">아이템의 데이터 소스에서의 인덱스입니다.</param>
        private void BindItem(VisualElement element, int index)
        {
            VariableHandle handle = _variableHandles[index];

            if (element.childCount == 0)
            {
                element.Add(this.CreateFieldElement(handle));
            }

            if (element.childCount > 0 && element[0] is IRefreshableField fieldRefreshable)
            {
                fieldRefreshable.RefreshVariableFieldPanel(handle);
            }
        }



        /// <summary> VariableHandle 객체에 따라 적절한 시각적 필드를 생성합니다. </summary>
        /// <param name="handle">필드 생성을 위한 VariableHandle 객체</param>
        /// <returns>생성된 VisualElement 필드</returns>
        private VisualElement CreateFieldElement(VariableHandle handle)
        {
            switch (handle.initialValue)
            {
                case BlackboardVariable: return VisualUtility.GetFieldByValueType(handle);

                case BlackboardBasedCondition: return new BlackboardBasedConditionListField(handle);
                
                default: return new UnsupportedTypeField(handle.context);
            }
        }
    }
}