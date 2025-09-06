using System;
using System.IO;
using UnityEditor;
using Object = UnityEngine.Object;

namespace TaskStreamer.Tool
{
    public static class EditorUtility
    {
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


        public static string FindAssetPath(string searchFilter, params string[] searchFolders)
        {
            string[] guids = null;

            if (searchFolders.Length == 0)
            {
                guids = AssetDatabase.FindAssets(searchFilter);
            }
            else
            {
                guids = AssetDatabase.FindAssets(searchFilter, searchFolders);
            }

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
    }
}