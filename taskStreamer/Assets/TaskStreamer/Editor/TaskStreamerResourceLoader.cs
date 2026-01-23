using TaskStreamer.Runtime.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// TaskStreamer의 UI 및 스타일 리소스를 로드하는 정적 클래스입니다.
    /// VisualTreeAsset과 StyleSheet 파일을 읽어 사용할 수 있도록 제공합니다.
    /// </summary>
    internal static class TaskStreamerResourceLoader
    {
        private static VisualTreeAsset _window;
        private static StyleSheet _windowStyle;

        private static VisualTreeAsset _settings;
        private static StyleSheet _settingsStyle;

        private static VisualTreeAsset _floatingInspector;

        private static VisualTreeAsset _behaviorNode;
        private static VisualTreeAsset _stateNode;
        private static VisualTreeAsset _serviceBlock;

        private static StyleSheet _edgeStyle;

        private static VisualTreeAsset _basicPropertiesSectionPanel;
        private static VisualTreeAsset _fieldPropertiesSection;
        private static VisualTreeAsset _serviceContainerPanel;
        private static VisualTreeAsset _serviceSectionPanel;

        private static VisualTreeAsset _bbbConditionField;
        private static VisualTreeAsset _bbbConditionListField;
        private static VisualTreeAsset _bbVariableField;
        private static StyleSheet _blackboardStyle;

        private static Texture2D  _resizeHandleImage;
        private static Texture2D  _deleteButtonImage;
        private static Texture2D  _bindingButtonImage;
        private static Texture2D _addButtonImage;



#region UXML & USS
        public static VisualTreeAsset window
        {
            get { return _window ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/TaskStreamerWindow.uxml"); }
        }

        
        public static VisualTreeAsset settings
        {
            get { return _settings ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/TaskStreamerSettings.uxml"); }
        }
        

        public static VisualTreeAsset behaviorNode
        {
            get { return _behaviorNode ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/Node/BehaviorNode.uxml"); }
        }


        public static VisualTreeAsset stateNode
        {
            get { return _stateNode ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/Node/StateNode.uxml"); }
        }

        
        public static VisualTreeAsset serviceBlock
        {
            get { return _serviceBlock ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/Node/ServiceBlock.uxml"); }
        }
        

        public static VisualTreeAsset basicPropertiesSectionPanel
        {
            get { return _basicPropertiesSectionPanel ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/Inspector/BasicPropertiesSection.uxml"); }
        }


        public static VisualTreeAsset fieldPropertiesSection
        {
            get { return _fieldPropertiesSection ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/Inspector/FieldPropertiesSection.uxml"); }
        }


        public static VisualTreeAsset serviceSectionPanel
        {
            get { return _serviceSectionPanel ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/Inspector/ServiceSection.uxml"); }
        }


        public static VisualTreeAsset serviceContainerPanel
        {
            get { return _serviceContainerPanel ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/Inspector/ServiceSectionContainer.uxml"); }
        }


        public static VisualTreeAsset floatingInspector
        {
            get { return _floatingInspector ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/Inspector/FloatingInspector.uxml"); }
        }


        public static VisualTreeAsset blackboardVariableField
        {
            get { return _bbVariableField ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/Inspector/BlackboardVariableField.uxml"); }
        }


        public static VisualTreeAsset conditionField
        {
            get { return _bbbConditionField ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/Inspector/ConditionField.uxml"); }
        }


        public static VisualTreeAsset conditionListField
        {
            get { return _bbbConditionListField ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/Inspector/ConditionFieldList.uxml"); }
        }





        public static StyleSheet settingsStyle
        {
            get { return _settingsStyle ??= PathUtility.LoadAsset<StyleSheet>("Styles/TaskStreamerSettingsStyle.uss"); }
        }


        public static StyleSheet windowStyle
        {
            get { return _windowStyle ??= PathUtility.LoadAsset<StyleSheet>("Styles/TaskStreamerWindowStyle.uss"); }
        }


        public static StyleSheet edgeStyle
        {
            get { return _edgeStyle ??= PathUtility.LoadAsset<StyleSheet>("Styles/EdgeStyle.uss"); }
        }


        public static StyleSheet blackboardStyle
        {
            get { return _blackboardStyle ??= PathUtility.LoadAsset<StyleSheet>("Styles/TaskStreamerBlackboardStyle.uss"); }
        }
#endregion



#region Images
        public static Texture2D resizeHandle
        {
            get { return _resizeHandleImage ??= PathUtility.LoadAsset<Texture2D>("Images/resizeHandle.png"); }
        }
        
        
        public static Texture2D  deleteButton
        {
            get { return _deleteButtonImage ??= PathUtility.LoadAsset<Texture2D>("Images/deleteButton.png"); }
        }
        
        
        public static Texture2D  bindingButton
        {
            get { return _bindingButtonImage ??= PathUtility.LoadAsset<Texture2D>("Images/bindingButton.png"); }
        }
        

        public static Texture2D addButton
        {
            get { return _addButtonImage ??= PathUtility.LoadAsset<Texture2D>("Images/addButton.png"); }
        }
#endregion
    }
}