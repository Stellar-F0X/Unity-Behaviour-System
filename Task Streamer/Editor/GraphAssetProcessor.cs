using TaskStreamer.BT;
using TaskStreamer.FSM;
using UnityEditor;

namespace TaskStreamer.Tool
{
    public class GraphAssetProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.EndsWith(".asset") == false)
                {
                    continue;
                }
                
                GraphAsset asset = AssetDatabase.LoadAssetAtPath<GraphAsset>(assetPath);
                    
                if (asset != null && asset.main is null)
                {
                    GraphAssetProcessor.InitializeGraphAsset(asset);
                }
            }
        }

        private static void InitializeGraphAsset(GraphAsset asset)
        {
            switch (asset.mainGraphType)
            {
                case GraphType.FSM: asset.main = StateMachine.CreateGraph("Main", asset);  break;
                
                case GraphType.BT:  asset.main = BehaviorTree.CreateGraph("Main", asset); break;
            }

            if (asset.main != null)
            {
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }
    }
}