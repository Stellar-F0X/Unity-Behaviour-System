using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// TaskStreamer의 UI 및 스타일 리소스를 로드하는 정적 클래스입니다.
    /// VisualTreeAsset과 StyleSheet 파일을 읽어 사용할 수 있도록 제공합니다.
    /// </summary>
    public class TaskStreamerResourcesLoader
    {
        // 캐시 변수들
        private static VisualTreeAsset _window;

        private static VisualTreeAsset _behaviorNode;

        private static VisualTreeAsset _stateNode;

        private static VisualTreeAsset _settings;

        private static VisualTreeAsset _basicSectionPanel;

        private static VisualTreeAsset _fieldSectionPanel;

        private static VisualTreeAsset _serviceSectionPanel;

        private static VisualTreeAsset _serviceContainerPanel;

        private static VisualTreeAsset _bbVariableItem;

        private static VisualTreeAsset _bbVariableField;

        private static VisualTreeAsset _bbbConditionField;

        private static VisualTreeAsset _bbbConditionListField;

        private static StyleSheet _editorSettingsStyle;

        private static StyleSheet _windowStyle;

        private static StyleSheet _edgeStyle;

        private static StyleSheet _inspectorStyle;


        /// <summary>
        /// TaskStreamer의 에디터 UI 레이아웃을 정의하는 VisualTreeAsset을 반환합니다.
        /// </summary>
        public static VisualTreeAsset Window
        {
            get
            {
                if (_window == null)
                {
#if USE_ASSETS_PATH
                    _window = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/TaskStreamerWindow.uxml");
#else
                    _window =
 AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/TaskStreamerWindow.uxml");
#endif
                    if (_window == null)
                    {
                        Debug.LogError("TaskStreamerWindow.uxml 파일을 찾을 수 없습니다.");
                    }
                }

                return _window;
            }
        }

        /// <summary>
        /// UIElement의 BehaviorNode 레이아웃을 로드하기 위한 정적 프로퍼티입니다.
        /// 지정된 경로에서 BehaviorNode.uxml 파일을 불러옵니다.
        /// </summary>
        public static VisualTreeAsset BehaviorNode
        {
            get
            {
                if (_behaviorNode == null)
                {
#if USE_ASSETS_PATH
                    _behaviorNode = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/BehaviorNode.uxml");
#else
                    _behaviorNode =
 AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/BehaviorNode.uxml");
#endif
                    if (_behaviorNode == null)
                    {
                        Debug.LogError("BehaviorNode.uxml 파일을 찾을 수 없습니다.");
                    }
                }

                return _behaviorNode;
            }
        }

        /// <summary>
        /// StateNode는 StateNode.uxml 파일에서 VisualTreeAsset을 로드하여 UI 요소의 레이아웃을 정의하는 속성입니다.
        /// </summary>
        public static VisualTreeAsset StateNode
        {
            get
            {
                if (_stateNode == null)
                {
#if USE_ASSETS_PATH
                    _stateNode = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/StateNode.uxml");
#else
                    _stateNode =
 AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/StateNode.uxml");
#endif
                    if (_stateNode == null)
                    {
                        Debug.LogError("StateNode.uxml 파일을 찾을 수 없습니다.");
                    }
                }

                return _stateNode;
            }
        }

        /// <summary>
        /// Settings는 TaskStreamer의 설정 관련 UI를 정의하는 VisualTreeAsset을 반환합니다.
        /// 컴포넌트나 인스펙터에서 설정 UI를 로드 및 수정하는 데 사용됩니다.
        /// </summary>
        public static VisualTreeAsset Settings
        {
            get
            {
                if (_settings == null)
                {
#if USE_ASSETS_PATH
                    _settings = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/TaskStreamerSettings.uxml");
#else
                    _settings = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/TaskStreamerSettings.uxml");
#endif
                    if (_settings == null)
                    {
                        Debug.LogError("TaskStreamerSettings.uxml 파일을 찾을 수 없습니다.");
                    }
                }

                return _settings;
            }
        }

        /// <summary>
        /// BasicSectionPanel은 UI 상에서 태스크(Task)의 기본 정보를 표시 및 편집할 수 있도록 설계된 VisualElement입니다.
        /// </summary>
        public static VisualTreeAsset BasicSectionPanel
        {
            get
            {
                if (_basicSectionPanel == null)
                {
#if USE_ASSETS_PATH
                    _basicSectionPanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/BasicSectionPanel.uxml");
#else
                    _basicSectionPanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/BasicSectionPanel.uxml");
#endif
                    if (_basicSectionPanel == null)
                    {
                        Debug.LogError("BasicSectionPanel.uxml 파일을 찾을 수 없습니다.");
                    }
                }

                return _basicSectionPanel;
            }
        }

        /// <summary>
        /// FieldSectionPanel은 UI의 필드 섹션 패널을 생성하고 관리하는 클래스입니다.
        /// 주어진 VariableHandle 리스트를 바탕으로 ListView 항목을 설정합니다.
        /// </summary>
        public static VisualTreeAsset FieldSectionPanel
        {
            get
            {
                if (_fieldSectionPanel == null)
                {
#if USE_ASSETS_PATH
                    _fieldSectionPanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/FieldSectionPanel.uxml");
#else
                    _fieldSectionPanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/FieldSectionPanel.uxml");
#endif
                    if (_fieldSectionPanel == null)
                    {
                        Debug.LogError("FieldSectionPanel.uxml 파일을 찾을 수 없습니다.");
                    }
                }

                return _fieldSectionPanel;
            }
        }

        /// <summary>
        /// ServiceSectionPanel은 서비스 설정 및 관리를 위한 UI 구성 요소입니다.
        /// 서비스 활성화, 삭제, 필드 리스트 관리 등을 제공합니다.
        /// </summary>
        public static VisualTreeAsset ServiceSectionPanel
        {
            get
            {
                if (_serviceSectionPanel == null)
                {
#if USE_ASSETS_PATH
                    _serviceSectionPanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/ServiceSectionPanel.uxml");
#else
                    _serviceSectionPanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/ServiceSectionPanel.uxml");
#endif
                    if (_serviceSectionPanel == null)
                    {
                        Debug.LogError("ServiceSectionPanel.uxml 파일을 찾을 수 없습니다.");
                    }
                }

                return _serviceSectionPanel;
            }
        }

        /// <summary>
        /// ServiceContainerPanel은 UI 요소로, 서비스 리스트를 관리 및 표시하기 위한 패널입니다.
        /// </summary>
        public static VisualTreeAsset ServiceContainerPanel
        {
            get
            {
                if (_serviceContainerPanel == null)
                {
#if USE_ASSETS_PATH
                    _serviceContainerPanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/ServiceContainerPanel.uxml");
#else
                    _serviceContainerPanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/ServiceContainerPanel.uxml");
#endif
                    if (_serviceContainerPanel == null)
                    {
                        Debug.LogError("ServiceContainerPanel.uxml 파일을 찾을 수 없습니다.");
                    }
                }

                return _serviceContainerPanel;
            }
        }

        /// <summary>
        /// BBVariableItem은 UI 엘리먼트의 레이아웃을 정의하는 UXML 리소스를 나타냅니다.
        /// TaskStreamer 툴에서 변수 항목 레이아웃을 로드하는 데 사용됩니다.
        /// </summary>
        public static VisualTreeAsset BBVariableItem
        {
            get
            {
                if (_bbVariableItem == null)
                {
#if USE_ASSETS_PATH
                    _bbVariableItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/BBVariableItem.uxml");
#else
                    _bbVariableItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/BBVariableItem.uxml");
#endif
                    if (_bbVariableItem == null)
                    {
                        Debug.LogError("BBVariableItem.uxml 파일을 찾을 수 없습니다.");
                    }
                }

                return _bbVariableItem;
            }
        }

        /// <summary>
        /// BBVariableField는 BlackboardVariable을 시각적으로 편집 및 관리하기 위해 설계된 필드 레이아웃 리소스를 로드하는 속성입니다.
        /// </summary>
        public static VisualTreeAsset BBVariableField
        {
            get
            {
                if (_bbVariableField == null)
                {
#if USE_ASSETS_PATH
                    _bbVariableField = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/BBVariableField.uxml");
#else
                    _bbVariableField = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/BBVariableField.uxml");
#endif
                    if (_bbVariableField == null)
                    {
                        Debug.LogError("BBVariableField.uxml 파일을 찾을 수 없습니다.");
                    }
                }

                return _bbVariableField;
            }
        }

        /// <summary>
        /// BBBConditionField는 BB 기반 조건 필드 UI를 정의하는 VisualTreeAsset을 로드합니다.
        /// </summary>
        public static VisualTreeAsset BBBConditionField
        {
            get
            {
                if (_bbbConditionField == null)
                {
#if USE_ASSETS_PATH
                    _bbbConditionField = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/BBBasedConditionField.uxml");
#else
                    _bbbConditionField = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/BBBasedConditionField.uxml");
#endif
                    if (_bbbConditionField == null)
                    {
                        Debug.LogError("BBBasedConditionField.uxml 파일을 찾을 수 없습니다.");
                    }
                }

                return _bbbConditionField;
            }
        }

        /// <summary>
        /// BBBConditionListField는 BlackboardBasedCondition의 조건 목록 UI를 표시하는 VisualTreeAsset을 정의합니다.
        /// </summary>
        public static VisualTreeAsset BBBConditionListField
        {
            get
            {
                if (_bbbConditionListField == null)
                {
#if USE_ASSETS_PATH
                    _bbbConditionListField = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/BBBasedConditionFieldList.uxml");
#else
                    _bbbConditionListField = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/BBBasedConditionFieldList.uxml");
#endif
                    if (_bbbConditionListField == null)
                    {
                        Debug.LogError("BBBasedConditionFieldList.uxml 파일을 찾을 수 없습니다.");
                    }
                }

                return _bbbConditionListField;
            }
        }

        /// <summary>
        /// EditorSettingsStyle 속성은 TaskStreamer 에디터의 설정 인터페이스에 적용되는 스타일시트를 반환합니다.
        /// </summary>
        public static StyleSheet EditorSettingsStyle
        {
            get
            {
                if (_editorSettingsStyle == null)
                {
#if USE_ASSETS_PATH
                    _editorSettingsStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/TaskStreamer/Editor/Resource/UI/Styles/EditorSettingsStyle.uss");
#else
                    _editorSettingsStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Styles/EditorSettingsStyle.uss");
#endif
                    if (_editorSettingsStyle == null)
                    {
                        Debug.LogError("EditorSettingsStyle.uss 파일을 찾을 수 없습니다.");
                    }
                }

                return _editorSettingsStyle;
            }
        }

        /// <summary>
        /// TaskStreamer에서 UI 스타일을 지정하기 위한 스타일시트를 제공하는 프로퍼티입니다.
        /// </summary>
        public static StyleSheet WindowStyle
        {
            get
            {
                if (_windowStyle == null)
                {
#if USE_ASSETS_PATH
                    _windowStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/TaskStreamer/Editor/Resource/UI/Styles/TaskStreamerWindowStyle.uss");
#else
                    _windowStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Styles/TaskStreamerWindowStyle.uss");
#endif
                    if (_windowStyle == null)
                    {
                        Debug.LogError("TaskStreamerWindowStyle.uss 파일을 찾을 수 없습니다.");
                    }
                }

                return _windowStyle;
            }
        }

        /// <summary>
        /// `EdgeStyle`는 그래프 기반 도구의 엣지 스타일을 정의하는 `StyleSheet` 자산입니다.
        /// 이를 사용하여 엣지의 시각적 스타일을 커스터마이징할 수 있습니다.
        /// </summary>
        public static StyleSheet EdgeStyle
        {
            get
            {
                if (_edgeStyle == null)
                {
#if USE_ASSETS_PATH
                    _edgeStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/TaskStreamer/Editor/Resource/UI/Styles/EdgeStyle.uss");
#else
                    _edgeStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Styles/EdgeStyle.uss");
#endif
                    if (_edgeStyle == null)
                    {
                        Debug.LogError("EdgeStyle.uss 파일을 찾을 수 없습니다.");
                    }
                }

                return _edgeStyle;
            }
        }

        /// <summary>
        /// InspectorStyle 속성은 유니티 에디터 내 검사기 패널의 스타일을 정의하는 StyleSheet 파일을 로드하는 데 사용됩니다.
        /// 경로 설정에 따라 적절한 스타일시트를 불러옵니다.
        /// </summary>
        public static StyleSheet InspectorStyle
        {
            get
            {
                if (_inspectorStyle == null)
                {
#if USE_ASSETS_PATH
                    _inspectorStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/TaskStreamer/Editor/Resource/UI/Styles/PanelStyle.uss");
#else
                    _inspectorStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Styles/PanelStyle.uss");
#endif
                    if (_inspectorStyle == null)
                    {
                        Debug.LogError("PanelStyle.uss 파일을 찾을 수 없습니다.");
                    }
                }

                return _inspectorStyle;
            }
        }
    }
}