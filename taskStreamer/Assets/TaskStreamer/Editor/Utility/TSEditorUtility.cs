using System.IO;
using NUnit.Framework;
using TaskStreamer.Runtime.Utility;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TaskStreamer.Tool
{
	public static class TSEditorUtility
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

			if (guids == null || guids.Length == 0)
			{
				throw new FileNotFoundException($"Asset not found at filter: {searchFilter}");
			}

			string path = AssetDatabase.GUIDToAssetPath(guids[0]);

			if (File.Exists(path) == false)
			{
				throw new FileNotFoundException($"Asset not found at filter: {searchFilter}");
			}

			return path;
		}


		
		internal static TextAsset LoadTemplateScript(string scriptTemplateFullName)
		{
			string searchFilter = $"{scriptTemplateFullName} t:TextAsset";
			string searchFolder = $"{PathUtility.editorPath}/ScriptTemplate";

			string templatePath = TSEditorUtility.FindAssetPath(searchFilter, searchFolder);
			Assert.IsTrue(templatePath.IsNotNullOrEmpty(), "템플릿 파일 경로를 찾지 못했습니다.");

			TextAsset templateFile = AssetDatabase.LoadAssetAtPath<TextAsset>(templatePath);
			Assert.IsNotNull(templateFile, "스크립트 템플릿을 불러오기 실패했습니다.");

			return templateFile;
		}

		

		/// <summary> 노드 스크립트 생성 (BT/FSM 노드용) </summary>
		internal static void CreateNewNodeScript<TCallback>(ScriptCreationCommandBase command, string scriptTemplateFullName, string nodeName) where TCallback : IScriptCreationCompletedCallback, new()
		{
			string scriptAssetPath = CreateScriptFile(scriptTemplateFullName, nodeName);

			PendingScriptCreationData data = new PendingScriptCreationData
			{
				scriptName = nodeName,
				scriptAssetPath = scriptAssetPath,
				graphGuid = command.view.focusGraph.guid,
				position = command.position,
				focusOnCreated = true
			};

			const ImportAssetOptions refreshOps = ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport;
			PendingCreationHandler.RequestScriptCreation<TCallback>(data);
			AssetDatabase.Refresh(refreshOps);
			CompilationPipeline.RequestScriptCompilation();
			EditorUtility.RequestScriptReload();
		}
		


		/// <summary> 서비스 스크립트 생성 </summary>
		internal static void CreateNewServiceScript<TCallback>(ScriptCreationCommandBase command, NodeViewBase focusedNodeView, string scriptTemplateFullName, string serviceName) where TCallback : IScriptCreationCompletedCallback, new()
		{
			string scriptAssetPath = CreateScriptFile(scriptTemplateFullName, serviceName);

			PendingScriptCreationData data = new PendingScriptCreationData
			{
				scriptName = serviceName,
				scriptAssetPath = scriptAssetPath,
				graphGuid = command.view.focusGraph.guid,
				targetGuid = focusedNodeView.targetNode.guid,
				position = command.position,
				focusOnCreated = true
			};

			const ImportAssetOptions refreshOps = ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport;
			PendingCreationHandler.RequestScriptCreation<TCallback>(data);
			AssetDatabase.Refresh(refreshOps);
			CompilationPipeline.RequestScriptCompilation();
			EditorUtility.RequestScriptReload();
		}

		

		/// <summary> Condition 스크립트 생성 </summary>
		internal static void CreateNewConditionScript<TCallback>(ScriptCreationCommandBase command, UGUID targetGuid, UGUID bbConditionGuid, string scriptTemplateFullName, string conditionName) where TCallback : IScriptCreationCompletedCallback, new()
		{
			string scriptAssetPath = CreateScriptFile(scriptTemplateFullName, conditionName);

			PendingScriptCreationData data = new PendingScriptCreationData
			{
				scriptName = conditionName,
				scriptAssetPath = scriptAssetPath,
				graphGuid = command.view.focusGraph.guid,
				targetGuid = targetGuid,
				extraGuid = bbConditionGuid,
				position = command.position,
				focusOnCreated = true
			};

			const ImportAssetOptions refreshOps = ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport;
			PendingCreationHandler.RequestScriptCreation<TCallback>(data);
			AssetDatabase.Refresh(refreshOps);
			CompilationPipeline.RequestScriptCompilation();
			EditorUtility.RequestScriptReload();
		}

		

		/// <summary> 템플릿에서 스크립트 파일 생성 </summary>
		private static string CreateScriptFile(string scriptTemplateFullName, string scriptName)
		{
			TextAsset templateFile = LoadTemplateScript(scriptTemplateFullName);
			Assert.IsNotNull(templateFile, "스크립트 템플릿을 불러오기 실패했습니다.");

			string content = GetContent(templateFile, scriptName);
			string assetCreationPath = Path.Combine(Application.dataPath, scriptName + ".cs");
			File.WriteAllText(assetCreationPath, content);

			return "Assets/" + scriptName + ".cs";
		}



		private static string GetContent(TextAsset templateFile, string nodeName)
		{
			string content = templateFile.text.Replace("#NODEGUID#", UGUID.Create().ToString());

			if (UnityEditor.EditorSettings.projectGenerationRootNamespace.IsNotNullOrEmpty())
			{
				string namespaceName = UnityEditor.EditorSettings.projectGenerationRootNamespace + " {";
				content = content.Replace("#ROOTNAMESPACEBEGIN#", namespaceName);
				content = content.Replace("#ROOTNAMESPACEEND#", "}");
			}
			else
			{
				content = content.Replace("#ROOTNAMESPACEBEGIN#", string.Empty);
				content = content.Replace("#ROOTNAMESPACEEND#", string.Empty);
			}

			content = content.Replace("#SCRIPTNAME#", nodeName);
			return content;
		}
	}
}
