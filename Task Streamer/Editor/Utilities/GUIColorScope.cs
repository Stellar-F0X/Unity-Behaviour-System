using System;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public struct GUIColorScope : IDisposable
    {
        public enum GUIColorScopeType
        {
            Background,
            Content
        };
        
        
        public GUIColorScope(Color color, GUIColorScopeType scopeType) : this()
        {
            this.colorScopeType = scopeType;
            this.originalColor = this.GetColor(scopeType);
            this.SetColor(scopeType, color);
        }
        
        
        public Color originalColor;
        
        public GUIColorScopeType colorScopeType;



        private Color GetColor(GUIColorScopeType scopeType)
        {
            switch (scopeType)
            {
                case GUIColorScopeType.Background: return GUI.backgroundColor;
                
                case GUIColorScopeType.Content: return GUI.contentColor;
            }
            
            Debug.LogError("Could not find scope type: " + scopeType);
            return Color.magenta;
        }

        
        private void SetColor(GUIColorScopeType scopeType, Color color)
        {
            switch (scopeType)
            {
                case GUIColorScopeType.Background: GUI.backgroundColor = color; break;

                case GUIColorScopeType.Content: GUI.contentColor = color; break;
            }
        }
        

        public void Dispose()
        {
            this.SetColor(this.colorScopeType, this.originalColor);
        }
    }
}