using UnityEditor;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class TaskStreamerResourcesLoader
    {
        public static VisualTreeAsset Window
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/TaskStreamerWindow.uxml");
#else
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/TaskStreamerWindow.uxml");
#endif
            }
        }

        public static VisualTreeAsset BehaviorNode
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/BehaviorNode.uxml");
#else
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/BehaviorNode.uxml");
#endif
            }
        }

        public static VisualTreeAsset StateNode
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/StateNode.uxml");
#else
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/StateNode.uxml");
#endif
            }
        }

        public static VisualTreeAsset Settings
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/EditorSettings.uxml");
#else
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/EditorSettings.uxml");
#endif
            }
        }

        public static VisualTreeAsset BasicPanel
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/BasicPanel.uxml");
#else
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/BasicPanel.uxml");
#endif
            }
        }
        
        
        public static VisualTreeAsset FieldPanel
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/FieldPanel.uxml");
#else
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/FieldPanel.uxml");
#endif
            }
        }
        
        
        public static VisualTreeAsset ServiceSectionPanel
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/ServiceSectionPanel.uxml");
#else
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/ServiceSectionPanel.uxml");
#endif
            }
        }


        public static VisualTreeAsset ServiceContainerPanel
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/ServiceContainerPanel.uxml");
#else
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/ServiceContainerPanel.uxml");
#endif
            }
        }
        

        public static VisualTreeAsset BBVariableItem
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/BBVariableItem.uxml");
#else
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/BBVariableItem.uxml");
#endif
            }
        }

        public static VisualTreeAsset BBVariableField
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/BBVariableField.uxml");
#else
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/BBVariableField.uxml");
#endif
            }
        }

        public static VisualTreeAsset BBBConditionField
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/BBBasedConditionField.uxml");
#else
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/BBBasedConditionField.uxml");
#endif
            }
        }

        public static VisualTreeAsset BBBConditionListField
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TaskStreamer/Editor/Resource/UI/Layouts/BBBasedConditionFieldList.uxml");
#else
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Layouts/BBBasedConditionFieldList.uxml");
#endif
            }
        }

        public static StyleSheet EditorSettingsStyle
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/TaskStreamer/Editor/Resource/UI/Styles/EditorSettingsStyle.uss");
#else
                return AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Styles/EditorSettingsStyle.uss");
#endif
            }
        }

        public static StyleSheet WindowStyle
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/TaskStreamer/Editor/Resource/UI/Styles/TaskStreamerWindowStyle.uss");
#else
                return AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Styles/TaskStreamerWindowStyle.uss");
#endif
            }
        }

        public static StyleSheet EdgeStyle
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/TaskStreamer/Editor/Resource/UI/Styles/EdgeStyle.uss");
#else
                return AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Styles/EdgeStyle.uss");
#endif
            }
        }

        public static StyleSheet InspectorStyle
        {
            get
            {
#if USE_ASSETS_PATH
                return AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/TaskStreamer/Editor/Resource/UI/Styles/PanelStyle.uss");
#else
                return AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/Styles/PanelStyle.uss");
#endif
            }
        }
    }
}