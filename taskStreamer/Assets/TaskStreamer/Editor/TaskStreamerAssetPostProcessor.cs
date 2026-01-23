using TaskStreamer.Runtime;
using TaskStreamer.Runtime.Utility;
using UnityEditor;
using UnityEngine;
using ObjectFactory = TaskStreamer.Runtime.Utility.ObjectFactory;

namespace TaskStreamer.Tool
{
    public class TaskStreamerAssetPostProcessor : AssetPostprocessor
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
                    InitializeAssetIfNeeded(asset); 
                    continue;
                }

                if (IsDuplicated(asset))
                {
                    HandleDuplicatedAsset(asset);
                }
            }
        }



        private static void InitializeAssetIfNeeded(GraphAsset asset)
        {
            asset.main = ObjectFactory.CreateGraph(asset, asset.mainGraphType, "Main");
            asset.blackboard = ObjectFactory.CreateBlackboardAsset("Blackboard");
            asset.graphGuid = UGUID.Create();
            
            AssetDatabase.AddObjectToAsset(asset.blackboard, asset);
            
            UnityEditor.EditorUtility.SetDirty(asset);
            
            AssetDatabase.SaveAssets();
        }



        private static void HandleDuplicatedAsset(GraphAsset asset)
        {
            asset.ReassignAllGraphElementGuids();
            
            UnityEditor.EditorUtility.SetDirty(asset);
            
            AssetDatabase.SaveAssets();
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