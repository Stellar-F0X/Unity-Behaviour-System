using TaskStreamer.Utility;
using UnityEditor;
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

        private static VisualTreeAsset _floatingInspectorView;

        private static VisualTreeAsset _behaviorNode;
        private static VisualTreeAsset _stateNode;
        private static VisualTreeAsset _serviceView;

        private static StyleSheet _edgeStyle;

        private static VisualTreeAsset _basicSectionPanel;
        private static VisualTreeAsset _fieldSectionPanel;
        private static VisualTreeAsset _serviceContainerPanel;
        private static VisualTreeAsset _serviceSectionPanel;

        private static VisualTreeAsset _bbbConditionField;
        private static VisualTreeAsset _bbbConditionListField;
        private static VisualTreeAsset _bbVariableField;
        private static StyleSheet _blackboardStyle;



        public static VisualTreeAsset Window
        {
            get { return _window ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/TaskStreamerWindow.uxml"); }
        }


        public static VisualTreeAsset BehaviorNode
        {
            get { return _behaviorNode ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/BehaviorNode.uxml"); }
        }


        public static VisualTreeAsset StateNode
        {
            get { return _stateNode ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/StateNode.uxml"); }
        }


        public static VisualTreeAsset Settings
        {
            get { return _settings ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/TaskStreamerSettings.uxml"); }
        }


        public static VisualTreeAsset BasicSectionPanel
        {
            get { return _basicSectionPanel ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/BasicSectionPanel.uxml"); }
        }


        public static VisualTreeAsset FieldSectionPanel
        {
            get { return _fieldSectionPanel ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/FieldSectionPanel.uxml"); }
        }


        public static VisualTreeAsset ServiceSectionPanel
        {
            get { return _serviceSectionPanel ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/ServiceSectionPanel.uxml"); }
        }


        public static VisualTreeAsset ServiceContainerPanel
        {
            get { return _serviceContainerPanel ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/ServiceContainerPanel.uxml"); }
        }


        public static VisualTreeAsset FloatingInspectorView
        {
            get { return _floatingInspectorView ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/FloatingInspectorView.uxml"); }
        }


        public static VisualTreeAsset BBVariableField
        {
            get { return _bbVariableField ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/BBVariableField.uxml"); }
        }


        public static VisualTreeAsset BBBConditionField
        {
            get { return _bbbConditionField ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/BBBasedConditionField.uxml"); }
        }


        public static VisualTreeAsset BBBConditionListField
        {
            get { return _bbbConditionListField ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/BBBasedConditionFieldList.uxml"); }
        }


        public static VisualTreeAsset ServiceView
        {
            get { return _serviceView ??= PathUtility.LoadAsset<VisualTreeAsset>("Layouts/ServiceView.uxml"); }
        }


        public static StyleSheet SettingsStyle
        {
            get { return _settingsStyle ??= PathUtility.LoadAsset<StyleSheet>("Styles/TaskStreamerSettingsStyle.uss"); }
        }


        public static StyleSheet WindowStyle
        {
            get { return _windowStyle ??= PathUtility.LoadAsset<StyleSheet>("Styles/TaskStreamerWindowStyle.uss"); }
        }


        public static StyleSheet EdgeStyle
        {
            get { return _edgeStyle ??= PathUtility.LoadAsset<StyleSheet>("Styles/EdgeStyle.uss"); }
        }


        public static StyleSheet BlackboardStyle
        {
            get { return _blackboardStyle ??= PathUtility.LoadAsset<StyleSheet>("Styles/TaskStreamerBlackboardStyle.uss"); }
        }
    }
}