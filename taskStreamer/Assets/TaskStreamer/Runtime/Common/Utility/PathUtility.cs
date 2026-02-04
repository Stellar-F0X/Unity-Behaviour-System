#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Assertions;
#endif
using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TaskStreamer.Runtime.Utility
{
	public static class PathUtility
	{
		private const string _ASSETS_BASE_PATH = "Assets/TaskStreamer/Editor/";

		private const string _PACKAGES_BASE_PATH = "Packages/com.stellarf0x.taskstreamer/Editor/";



		public static string editorPath
		{
			get
			{
#if USE_ASSETS_PATH
				return _ASSETS_BASE_PATH;
#else
                return PACKAGES_BASE_PATH;
#endif
			}
		}


#if UNITY_EDITOR
		public static T LoadAsset<T>(string fileName) where T : Object
		{
#if USE_ASSETS_PATH
			Span<char> filePathSpan = stackalloc char[_ASSETS_BASE_PATH.Length + fileName.Length];
			_ASSETS_BASE_PATH.AsSpan().CopyTo(filePathSpan);
			fileName.AsSpan().CopyTo(filePathSpan[_ASSETS_BASE_PATH.Length..]);
			string filePath = new string(filePathSpan);
#else
            Span<char> filePathSpan = stackalloc char[PACKAGES_BASE_PATH.Length + fileName.Length];
            PACKAGES_BASE_PATH.AsSpan().CopyTo(filePathSpan);
            fileName.AsSpan().CopyTo(filePathSpan[PACKAGES_BASE_PATH.Length..]);
            string filePath = new string(filePathSpan);
#endif
			T cachedAsset = AssetDatabase.LoadAssetAtPath<T>(filePath);
			Assert.IsTrue(cachedAsset != null, $"Cannot load {typeof(T).Name}.");
			return cachedAsset;
		}

#endif



		public static string CallerFilePathToUnityPath(string callerFilePath)
		{
			if (string.IsNullOrEmpty(callerFilePath))
			{
				return null;
			}

			callerFilePath = Path.GetFullPath(callerFilePath).Replace('\\', '/');

			string dataPath = Application.dataPath.Replace('\\', '/');

			if (callerFilePath.StartsWith(dataPath))
			{
				return "Assets" + callerFilePath.Substring(dataPath.Length);
			}

			string projectRoot = Path.GetDirectoryName(dataPath)?.Replace('\\', '/');

			string packageCachePath = projectRoot + "/Library/PackageCache/";

			if (callerFilePath.StartsWith(packageCachePath) == false)
			{
				return null;
			}

			string relative = callerFilePath.Substring(packageCachePath.Length);

			int at = relative.IndexOf('@');

			if (at > 0)
			{
				int slash = relative.IndexOf('/', at);

				relative = slash > 0 ? relative.Remove(at, slash - at) : relative.Substring(0, at);
			}

			return "Packages/" + relative;
		}
	}
}