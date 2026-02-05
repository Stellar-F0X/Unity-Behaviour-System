using System.Collections.Generic;
using UnityEngine;

namespace TaskStreamer.Tool
{
#if USE_ASSETS_PATH
    [CreateAssetMenu(fileName = "New EditorSettings", menuName = "Task Streamer/Settings/EditorSettings")]
#endif
    public class TSEditorSettings : ScriptableObject
    {
        private static TSEditorSettings _instance;
        
        public static TSEditorSettings Instance
        {
	        get { return _instance = _instance == null ? TSEditorUtility.FindAssetByName<TSEditorSettings>($"t:{nameof(TSEditorSettings)}") : _instance; }
        }
        
        public float highlightDuration = 0.5f;
        public uint updatesPerSecond = 10;
        
        public float updateInterval = 0.1f;
        public float durationReciprocal = 2f;

        public List<string> tagList = new List<string>() { "None" };
        
        public Color minimapColor = new Color32(30, 30, 30, 255);
        public Color nodeGroupColor = new Color32(65, 65, 65, 255);
        public Color successNodeColor = new Color32(0, 100, 0, 255);
        public Color failureNodeColor = new Color32(100, 0, 0, 255);
        
        public Gradient nodeStatusGradient;
        public Gradient edgeStatusGradient;
    }
}
