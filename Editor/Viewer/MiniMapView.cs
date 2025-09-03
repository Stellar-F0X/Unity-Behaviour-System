using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    internal class MiniMapView : MiniMap
    {
        public MiniMapView(ToolbarToggle toggleMinimap, TaskGraphView graphView)
        {
            this.style.backgroundColor = TaskStreamerEditor.settings.minimapColor;
            this._activated = false;
            this.visible = false;
            this.anchored = true;
            this.graphView = graphView;
            
            toggleMinimap.UnregisterValueChangedCallback(this.ActiveMinimap);
            toggleMinimap.RegisterValueChangedCallback(this.ActiveMinimap);
            
            graphView.UnregisterCallback<GeometryChangedEvent>(this.UpdatePosition);
            graphView.RegisterCallback<GeometryChangedEvent>(this.UpdatePosition);
        }
        
        
        private bool _activated;


        private void ActiveMinimap(ChangeEvent<bool> activeEvent)
        {
            this._activated = activeEvent.newValue;
            this.visible = activeEvent.newValue;
            this.enabledSelf = activeEvent.newValue;
        }


        private void UpdatePosition(GeometryChangedEvent evt)
        {
            if (evt.newRect.width >= 240 && evt.newRect.height >= 240)
            {
                //Toggle로 비활성화된게 아니라, 남은 공간이 없어서 비활성화된 것이라면 다시 활성화한다.
                if (_activated)
                {
                    this.visible = true;
                    this.enabledSelf = true;
                }

                float x = evt.newRect.width - 220;
                float y = evt.newRect.height - 220;

                this.SetPosition(new Rect(x, y, 200, 200));
            }
            else
            {
                this.visible = false;
                this.enabledSelf = false;
            }
        }
    }
}