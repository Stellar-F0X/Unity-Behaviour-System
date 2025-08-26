using UnityEditor.Experimental.GraphView;

namespace TaskStreamer.Tool
{
    public interface ICategoryTreeProvider
    {
        public SearchTreeEntry[] ProvideCategories(FactoryModule module);
    }
}