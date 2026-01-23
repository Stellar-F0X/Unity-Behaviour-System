using TaskStreamer.Runtime;
using UnityEditor;

namespace TaskStreamer.Tool
{
    [CustomEditor(typeof(GraphAsset))]
    public class GraphAssetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                base.OnInspectorGUI();
            }
        }
    }
}