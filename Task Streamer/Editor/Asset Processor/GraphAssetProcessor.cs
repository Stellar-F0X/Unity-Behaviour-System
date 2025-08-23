using TaskStreamer.Utility;
using UnityEditor;

namespace TaskStreamer.Tool
{
    public class GraphAssetProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.EndsWith(".asset") == false)
                {
                    continue;
                }
                
                GraphAsset asset = AssetDatabase.LoadAssetAtPath<GraphAsset>(assetPath);

                if (asset == null)
                {
                    continue;
                }
                
                if (asset.main is null)
                {
                    Graph graphRef = asset.main;
                    
                    TSObjectFactory.CreateGraph(asset, asset.mainGraphType, ref graphRef, "Main");
                    
                    asset.main = graphRef;
                    
                    asset.graphGuid = UGUID.Create();
                }
                
                if (EditorUtility.IsDuplicated(asset))
                {
                    asset.ReassignAllGraphElementGuids();
                }
            }
        }
    }
}