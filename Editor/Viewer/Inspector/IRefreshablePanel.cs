namespace TaskStreamer.Tool
{
    /// <summary>
    /// IRefreshablePanel 인터페이스는 UI 패널의 새로고침 동작을 정의합니다.
    /// </summary>
    internal interface IRefreshablePanel
    {
        /// <summary> 패널의 UI 요소나 데이터 상태를 새로고침하여 최신 정보를 표시합니다. </summary>
        public void RefreshPanel();


        /// <summary> 새 Task 객체를 기반으로 패널을 갱신합니다. </summary>
        /// <param name="newValue">패널에 적용할 새로운 Task 객체입니다.</param>
        public void RefreshPanelWithNewValue(object newValue);
    }
}