using System;
using System.IO;
using UnityEditor;
using Object = UnityEngine.Object;

namespace TaskStreamer.Tool
{
    public static class EditorUtility
    {
        public static MonoScript GetMonoScriptFromPoco(Type pocoType)
        {
            string[] guids = AssetDatabase.FindAssets("t:MonoScript");
    
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }
                
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
        
                if (script != null && script.GetClass() == pocoType)
                {
                    return script;
                }
            }
    
            return null;
        }

        
        public static T FindAssetByName<T>(string searchFilter) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets(searchFilter);

            if (guids is null || guids.Length == 0)
            {
                return null;
            }

            foreach (string guid in guids)
            {
                string parentPath = AssetDatabase.GUIDToAssetPath(guid);

                if (File.Exists(parentPath))
                {
                    return AssetDatabase.LoadAssetAtPath<T>(parentPath);
                }
            }

            throw new FileNotFoundException($"Asset not found at filter: {searchFilter}");
        }


        public static string FindAssetPath(string searchFilter)
        {
            string[] guids = AssetDatabase.FindAssets(searchFilter);

            if (guids != null && guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);

                if (File.Exists(path))
                {
                    return path;
                }
            }

            throw new FileNotFoundException($"Asset not found at filter: {searchFilter}");
        }
        
        
        public static bool IsDuplicated(GraphAsset currentAsset)
        {
            string[] guids = AssetDatabase.FindAssets("t:GraphAsset");

            int count = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                
                GraphAsset asset = AssetDatabase.LoadAssetAtPath<GraphAsset>(assetPath);

                if (asset == null || asset.graphGuid != currentAsset.graphGuid)
                {
                    continue;
                }

                count++;

                if (count > 1) // 자기 자신 포함해서 2개 이상이면 중복 
                {
                    return true;
                }
            }

            return false;
        }
    }
}