using System;

namespace TaskStreamer.Tool
{
	/// <summary>
	/// 스크립트 생성 완료 후 콜백을 받기 위한 인터페이스입니다.
	/// 이 인터페이스를 구현하는 클래스는 반드시 파라미터 없는 기본 생성자를 가져야 합니다.
	/// </summary>
	internal interface IScriptCreationCompletedCallback
	{
		/// <summary> 스크립트 생성 완료 후 호출되는 콜백 </summary>
		public void OnScriptCreated(Type createdScriptType, PendingScriptCreationData data);
	}
}
