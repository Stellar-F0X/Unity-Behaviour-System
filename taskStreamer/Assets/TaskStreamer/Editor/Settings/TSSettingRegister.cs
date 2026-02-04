using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    //https://docs.unity3d.com/ScriptReference/SettingsProvider.html
    public class TSSettingRegister
    {
        public readonly static string SettingsRegistryPath = "Project/Task Streamer Settings"; 
        
        private readonly static List<string> _Keywords = new List<string>()
        {
            "Task Streamer",
            "Task Streamer Settings",
            "TS",
            "TS Settings",
            "Graph",
            "FSM",
            "BT"
        };
        
        
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(SettingsRegistryPath, SettingsScope.Project);
            provider.label = "Task Streamer";
            provider.activateHandler = TSSettingRegister.ProvideSettingHandler;
            provider.keywords = _Keywords;
            return provider;
        }

        
        private static void ProvideSettingHandler(string searchContext, VisualElement rootElement)
        {
            TSEditorSettings settings = TSEditorUtility.FindAssetByName<TSEditorSettings>($"t:{nameof(TSEditorSettings)}");

            rootElement.Add(new InspectorElement(settings));
            rootElement.Bind(new SerializedObject(settings));
        }
    }
}