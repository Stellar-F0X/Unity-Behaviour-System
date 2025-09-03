using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// TaskStreamer의 UI 및 스타일 리소스를 로드하는 정적 클래스입니다.
    /// VisualTreeAsset과 StyleSheet 파일을 읽어 사용할 수 있도록 제공합니다.
    /// </summary>
    internal class TaskStreamerResourcesLoader
    {
        private const string ASSETS_BASE_PATH = "Assets/TaskStreamer/Editor/Resource/UI/";

        private const string PACKAGES_BASE_PATH = "Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/";


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




        private static T LoadAsset<T>(string fileName) where T : ScriptableObject
        {
#if USE_ASSETS_PATH
            string filePath = ASSETS_BASE_PATH + fileName;
#else
            string filePath = PACKAGES_BASE_PATH + fileName;
#endif
            T cachedAsset = AssetDatabase.LoadAssetAtPath<T>(filePath);
            Debug.Assert(cachedAsset != null, $"Cannot load {typeof(T).Name}.");
            return cachedAsset;
        }


        
        public static VisualTreeAsset Window
        {
            get { return _window ??= LoadAsset<VisualTreeAsset>("Layouts/TaskStreamerWindow.uxml"); }
        }


        public static VisualTreeAsset BehaviorNode
        {
            get { return _behaviorNode ??= LoadAsset<VisualTreeAsset>("Layouts/BehaviorNode.uxml"); }
        }


        public static VisualTreeAsset StateNode
        {
            get { return _stateNode ??= LoadAsset<VisualTreeAsset>("Layouts/StateNode.uxml"); }
        }

        
        public static VisualTreeAsset Settings
        {
            get { return _settings ??= LoadAsset<VisualTreeAsset>("Layouts/TaskStreamerSettings.uxml"); }
        }


        public static VisualTreeAsset BasicSectionPanel
        {
            get { return _basicSectionPanel ??= LoadAsset<VisualTreeAsset>("Layouts/BasicSectionPanel.uxml"); }
        }


        public static VisualTreeAsset FieldSectionPanel
        {
            get { return _fieldSectionPanel ??= LoadAsset<VisualTreeAsset>("Layouts/FieldSectionPanel.uxml"); }
        }


        public static VisualTreeAsset ServiceSectionPanel
        {
            get { return _serviceSectionPanel ??= LoadAsset<VisualTreeAsset>("Layouts/ServiceSectionPanel.uxml"); }
        }


        public static VisualTreeAsset ServiceContainerPanel
        {
            get { return _serviceContainerPanel ??= LoadAsset<VisualTreeAsset>("Layouts/ServiceContainerPanel.uxml"); }
        }

        
        public static VisualTreeAsset FloatingInspectorView
        {
            get { return _floatingInspectorView ??= LoadAsset<VisualTreeAsset>("Layouts/FloatingInspectorView.uxml"); }
        }


        public static VisualTreeAsset BBVariableField
        {
            get { return _bbVariableField ??= LoadAsset<VisualTreeAsset>("Layouts/BBVariableField.uxml"); }
        }


        public static VisualTreeAsset BBBConditionField
        {
            get { return _bbbConditionField ??= LoadAsset<VisualTreeAsset>("Layouts/BBBasedConditionField.uxml"); }
        }


        public static VisualTreeAsset BBBConditionListField
        {
            get { return _bbbConditionListField ??= LoadAsset<VisualTreeAsset>("Layouts/BBBasedConditionFieldList.uxml"); }
        }

        
        public static VisualTreeAsset ServiceView
        {
            get { return _serviceView ??= LoadAsset<VisualTreeAsset>("Layouts/ServiceView.uxml"); }
        }


        public static StyleSheet SettingsStyle
        {
            get { return _settingsStyle ??= LoadAsset<StyleSheet>("Styles/TaskStreamerSettingsStyle.uss"); }
        }


        public static StyleSheet WindowStyle
        {
            get { return _windowStyle ??= LoadAsset<StyleSheet>("Styles/TaskStreamerWindowStyle.uss"); }
        }


        public static StyleSheet EdgeStyle
        {
            get { return _edgeStyle ??= LoadAsset<StyleSheet>("Styles/EdgeStyle.uss"); }
        }

        
        public static StyleSheet BlackboardStyle
        {
            get { return _blackboardStyle ??= LoadAsset<StyleSheet>("Styles/TaskStreamerBlackboardStyle.uss"); }
        }
    }
}