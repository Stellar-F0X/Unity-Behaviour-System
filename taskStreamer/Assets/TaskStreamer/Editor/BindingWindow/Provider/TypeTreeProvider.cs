using System;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class TypeTreeProvider : ICategoryTreeProvider
    {
        public TypeTreeProvider(bool bindSubClassTypes)
        {
            this._bindSubClassTypes = bindSubClassTypes;
        }

        
        private readonly bool _bindSubClassTypes;


        public SearchTreeEntry[] ProvideCategories(FactoryModule module)
        {
            Type[] typeList = this.GetTypes(module);
            
            SearchTreeEntry[] entries = new SearchTreeEntry[typeList.Length + 1];
            entries[0] = new SearchTreeGroupEntry(new GUIContent(module.title));
            entries[0].level = module.layer;

            for (int i = 1; i < entries.Length; ++i)
            {
                string typeName = StringUtility.ToNicifyName(typeList[i - 1].Name);
                entries[i] = new SearchTreeEntry(new GUIContent(typeName));
                entries[i].userData = (typeList[i - 1], module);
                entries[i].level = module.layer + 1;
            }

            return entries;
        }


        private Type[] GetTypes(FactoryModule module)
        {
            if (this._bindSubClassTypes)
            {
                return TypeCache.GetTypesDerivedFrom(module.targetType).OrderByNameAndFilterAbstracts();
            }
            else
            {
                return new Type[1] { module.targetType };
            }
        }
    }
}