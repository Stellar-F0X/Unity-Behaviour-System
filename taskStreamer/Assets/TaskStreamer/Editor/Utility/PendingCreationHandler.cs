using System;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Tool
{
	/// <summary>
	/// 스크립트 생성 후 도메인 리로드가 완료되면 콜백을 자동으로 호출하는 핸들러입니다.
	/// SessionState를 사용하여 도메인 리로드 전후로 상태를 유지합니다.
	/// </summary>
	[InitializeOnLoad]
	internal static class PendingCreationHandler
	{
		static PendingCreationHandler()
		{
			EditorApplication.delayCall -= PendingCreationHandler.ProcessPendingCreation;
			EditorApplication.delayCall += PendingCreationHandler.ProcessPendingCreation;
		}


		private const string _PENDING_KEY = "TaskStreamer.PendingScriptCreation";


		/// <summary>
		/// 스크립트 생성 요청을 저장합니다.
		/// 도메인 리로드 후 지정된 콜백 타입의 OnScriptCreated 메서드가 호출됩니다.
		/// </summary>
		/// <typeparam name="TCallback">IScriptCreationCompletedCallback을 구현한 콜백 타입</typeparam>
		/// <param name="data">생성 요청 데이터</param>
		public static void RequestScriptCreation<TCallback>(PendingScriptCreationData data) where TCallback : IScriptCreationCompletedCallback, new()
		{
			Assert.IsNotNull(data, "Data must not be null");

			data.callbackTypeName = typeof(TCallback).AssemblyQualifiedName;

			string json = JsonUtility.ToJson(data);
			SessionState.SetString(_PENDING_KEY, json);
		}


		/// <summary> 도메인 리로드 후 대기 중인 스크립트 생성 작업을 처리합니다. </summary>
		private static void ProcessPendingCreation()
		{
			string json = SessionState.GetString(_PENDING_KEY, string.Empty);

			if (string.IsNullOrEmpty(json))
			{
				return;
			}

			// 처리 완료 후 상태 삭제 (먼저 삭제하여 중복 실행 방지)
			SessionState.EraseString(_PENDING_KEY);
			
			PendingScriptCreationData data = JsonUtility.FromJson<PendingScriptCreationData>(json);
			Assert.IsNotNull(data, "Failed to deserialize PendingScriptCreationData");
			
			// TSEditor 인스턴스와 GraphView가 준비될 때까지 대기
			EditorApplication.delayCall += () => PendingCreationHandler.ExecuteCallback(data);
		}


		/// <summary> 콜백을 실행합니다. </summary>
		private static void ExecuteCallback(PendingScriptCreationData data)
		{
			if (TSEditor.Instance == null || TSEditor.Instance.taskGraphView == null)
			{
				Debug.LogWarning($"TSEditor가 준비되지 않아 스크립트 생성 콜백을 건너뜁니다: {data.scriptName}");
				return;
			}

			// 스크립트 로드
			MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(data.scriptAssetPath);
			Assert.IsNotNull(script, $"스크립트를 찾을 수 없습니다: {data.scriptAssetPath}");


			Type scriptClass = script.GetClass();
			Assert.IsNotNull(scriptClass, $"스크립트에서 클래스를 가져올 수 없습니다: {data.scriptAssetPath}");

			// 그래프 전환
			Graph targetGraph = TSEditor.Instance.graphAsset.GetGraph(data.graphGuid);
			Assert.IsNotNull(targetGraph,$"대상 그래프를 찾을 수 없습니다: {data.graphGuid}");


			if (TSEditor.Instance.currentGraph != targetGraph)
			{
				bool isSubGraph = targetGraph != TSEditor.Instance.graphAsset.main;
				TSEditor.Instance.ChangeGraph(targetGraph, isSubGraph);
			}

			// 콜백 타입 인스턴스 생성 및 호출
			Type callbackType = Type.GetType(data.callbackTypeName);
			Assert.IsNotNull(callbackType, $"콜백 타입을 찾을 수 없습니다: {data.callbackTypeName}");

			IScriptCreationCompletedCallback callback = Activator.CreateInstance(callbackType) as IScriptCreationCompletedCallback;
			Assert.IsNotNull(callback, $"콜백 인스턴스를 생성할 수 없습니다: {data.callbackTypeName}");

			callback.OnScriptCreated(scriptClass, data);
			Debug.Log($"'{data.scriptName}' has been created successfully.");
		}
	}
	
	
	/// <summary> 스크립트 생성 요청 시 저장되는 데이터로, 도메인 리로드 후에도 SessionState를 통해 유지됩니다. </summary>
	[Serializable]
	internal class PendingScriptCreationData
	{
		/// <summary> 콜백을 받을 타입의 AssemblyQualifiedName </summary>
		public string callbackTypeName;

		/// <summary> 생성할 스크립트/노드의 이름 </summary>
		public string scriptName;

		/// <summary> 생성된 스크립트의 에셋 경로 </summary>
		public string scriptAssetPath;

		/// <summary> 노드가 생성될 그래프의 GUID </summary>
		public UGUID graphGuid;

		/// <summary> 대상 요소(Node 또는 Transition)의 GUID </summary>
		public UGUID targetGuid;

		/// <summary> 추가 GUID (예: BBCondition의 GUID) </summary>
		public UGUID extraGuid;

		/// <summary> 노드 생성 위치 </summary>
		public Vector2 position;

		/// <summary> 생성 후 선택 여부 </summary>
		public bool focusOnCreated;
	}
}
