using System.Collections.Generic;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class EditorSettings : ScriptableObject
    {
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


        private void Awake()
        {
            //프로젝트 뷰에선 숨기고, 빌드될땐, 이미 모든 노드에 값이 할당된 뒤일테니, 빌드에 포함하지 않음.
            this.hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInBuild;
        }
    }
}
