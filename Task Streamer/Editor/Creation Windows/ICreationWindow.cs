using System;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public interface ICreationWindow
    {
        public bool modulesIsEmpty
        {
            get;
        }

        public void OpenWindow(Vector2 mousePosition, float width = 200, float height = 240);

        public void RegisterCreationCallbackOnce(Delegate callback);

        public void UnregisterCreationCallbackOnce();
        
        public ICreationWindow AddFactoryModule(FactoryModule module);

        public ICreationWindow RemoveFactoryModule(FactoryModule module);
    }
}