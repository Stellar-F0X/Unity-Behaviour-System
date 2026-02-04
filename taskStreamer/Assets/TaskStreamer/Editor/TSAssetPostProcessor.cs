using System.Linq;
using TaskStreamer.Runtime;
using UnityEditor;

namespace TaskStreamer.Tool
{
	public class TSAssetPostProcessor : AssetPostprocessor
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

				TSAssetPostProcessor.CheckForInitialization(asset);
				TSAssetPostProcessor.CheckForDuplicate(asset);
			}
		}




		private static void CheckForInitialization(GraphAsset asset)
		{
			if (asset.isGraphInitialized)
			{
				return;
			}

			asset.isGraphInitialized = true;

			if (asset.blackboard == null)
			{
				return;
			}

			AssetDatabase.AddObjectToAsset(asset.blackboard, asset);
			AssetDatabase.SaveAssets();
		}




		private static void CheckForDuplicate(GraphAsset asset)
		{
			bool isDuplicated = AssetDatabase.FindAssets("t:GraphAsset")
			                                 .Select(static guid => AssetDatabase.GUIDToAssetPath(guid))
			                                 .Select(static path => AssetDatabase.LoadAssetAtPath<GraphAsset>(path))
			                                 .Where(a => a != null && a.graphGuid == asset.graphGuid)
			                                 .Skip(1) //자기 자신 하나 건너뜀.
			                                 .Any();  //두 번째가 존재하면 중복.

			if (isDuplicated)
			{
				asset.ReassignAllGraphElementGuids();
				EditorUtility.SetDirty(asset);
				AssetDatabase.SaveAssets();
			}
		}
	}
}