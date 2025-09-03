using TaskStreamer.Utility;
using UnityEditor;
using UnityEngine;
using ObjectFactory = TaskStreamer.Utility.ObjectFactory;

namespace TaskStreamer.Tool
{
    public class TaskStreamerAssetProcessor : AssetPostprocessor
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
                    ObjectFactory.CreateGraph(asset, asset.mainGraphType, ref graphRef, "Main");
                    asset.main = graphRef;
                    
                    asset.graphGuid = UGUID.Create();
                    asset.blackboard = ScriptableObject.CreateInstance<BlackboardAsset>();
                    asset.blackboard.name = "Blackboard";
                    
                    AssetDatabase.AddObjectToAsset(asset.blackboard, asset);
                    UnityEditor.EditorUtility.SetDirty(asset.blackboard);
                    UnityEditor.EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssets();
                }

                if (TaskStreamerAssetProcessor.IsDuplicated(asset))
                {
                    asset.ReassignAllGraphElementGuids();
                    UnityEditor.EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssets();
                }
            }
        }


        private static bool IsDuplicated(GraphAsset currentAsset)
        {
            int count = 0;
            string[] guids = AssetDatabase.FindAssets("t:GraphAsset");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GraphAsset asset = AssetDatabase.LoadAssetAtPath<GraphAsset>(assetPath);

                if (asset == null || asset.graphGuid != currentAsset.graphGuid)
                {
                    continue;
                }

                count++;

                if (count == 2) // 자기 자신 포함해서 2개 이상이면 중복 
                {
                    return true;
                }
            }

            return false;
        }
    }
}