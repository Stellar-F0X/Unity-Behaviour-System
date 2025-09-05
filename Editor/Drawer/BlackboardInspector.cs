using UnityEditor;

namespace TaskStreamer.Tool
{
    [CustomEditor(typeof(BlackboardAsset))]
    public class BlackboardInspector : Editor
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