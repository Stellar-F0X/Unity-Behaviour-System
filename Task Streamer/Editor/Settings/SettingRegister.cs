using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    //https://docs.unity3d.com/ScriptReference/SettingsProvider.html
    public class SettingRegister
    {
        public static string SettingsResistryPath
        {
            get { return "Project/Task Streamer Settings"; }
        }
        
        
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            return new SettingsProvider(SettingsResistryPath, SettingsScope.Project)
            {
                label = "Task Streamer",
                activateHandler = ProvideSettingHandler
            };
        }

        private static void ProvideSettingHandler(string searchContext, VisualElement rootElement)
        {
            var settings = EditorUtilities.FindAssetByName<EditorSettings>($"t:{nameof(EditorSettings)}");

            rootElement.Add(new InspectorElement(settings));
            rootElement.Bind(new SerializedObject(settings));
        }
    }
}