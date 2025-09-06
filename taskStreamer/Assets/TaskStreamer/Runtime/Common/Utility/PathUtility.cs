#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TaskStreamer.Utility
{
    public static class PathUtility
    {
        private const string ASSETS_BASE_PATH = "Assets/TaskStreamer/Editor/Resource/UI/";

        private const string PACKAGES_BASE_PATH = "Packages/com.stellarf0x.taskstreamer/Editor/Resource/UI/";



        public static T LoadAsset<T>(string fileName) where T : Object
        {
#if USE_ASSETS_PATH
            Span<char> filePathSpan = stackalloc char[ASSETS_BASE_PATH.Length + fileName.Length];
            
            ASSETS_BASE_PATH.AsSpan().CopyTo(filePathSpan);
            fileName.AsSpan().CopyTo(filePathSpan[ASSETS_BASE_PATH.Length..]);
            
            string filePath = new string(filePathSpan);
            
            
#else
            Span<char> filePathSpan = stackalloc char[PACKAGES_BASE_PATH.Length + fileName.Length];

            PACKAGES_BASE_PATH.AsSpan().CopyTo(filePathSpan);
            fileName.AsSpan().CopyTo(filePathSpan[PACKAGES_BASE_PATH.Length..]);

            string filePath = new string(filePathSpan);
#endif
            
            T cachedAsset = AssetDatabase.LoadAssetAtPath<T>(filePath);
            Debug.Assert(cachedAsset != null, $"Cannot load {typeof(T).Name}.");
            return cachedAsset;
        }
        
        
        
        public static string ToAssetPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                return null;
            }

            // 슬래시 통일
            fullPath = fullPath.Replace('\\', '/');

            // 이미 상대 경로인 경우 그대로 반환
            if (fullPath.StartsWith("Assets/") || fullPath.StartsWith("Packages/"))
            {
                return fullPath;
            }
            
            string dataPath = Application.dataPath.Replace('\\', '/');
            
            string projectPath = Path.GetDirectoryName(dataPath)?.Replace('\\', '/');

            // Assets 폴더 내부 경로인지 확인
            if (fullPath.StartsWith(dataPath))
            {
                return "Assets" + fullPath.Substring(dataPath.Length);
            }

            // Packages 폴더 내부 경로인지 확인
            string packagesPath = projectPath + "/Packages";

            if (fullPath.StartsWith(packagesPath))
            {
                return "Packages" + fullPath.Substring(packagesPath.Length);
            }

            // 경로에서 Assets/ 찾기
            int assetsIndex = fullPath.LastIndexOf("/Assets/", StringComparison.Ordinal);

            if (assetsIndex >= 0)
            {
                return fullPath.Substring(assetsIndex + 1); // "/Assets/" -> "Assets/"
            }

            // 경로에서 Packages/ 찾기
            int packagesIndex = fullPath.LastIndexOf("/Packages/", StringComparison.Ordinal);

            if (packagesIndex >= 0)
            {
                return fullPath.Substring(packagesIndex + 1); // "/Packages/" -> "Packages/"
            }

            return null; // 변환 불가능한 경로
        }
    }
}