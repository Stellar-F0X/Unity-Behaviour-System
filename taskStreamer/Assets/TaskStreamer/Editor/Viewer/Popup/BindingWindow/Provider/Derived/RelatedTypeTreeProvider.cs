using System;
using TaskStreamer.Runtime.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class RelatedTypeTreeProvider : ICategoryTreeProvider
    {
        public RelatedTypeTreeProvider(bool subClassTypeOnly)
        {
            this._subClassTypeOnly = subClassTypeOnly;
        }

        
        private readonly bool _subClassTypeOnly;


        public SearchTreeEntry[] ProvideCategories(FactoryModule module)
        {
            Type[] typeList = this.GetTypesFromTypeCollection(module);
            SearchTreeEntry[] entries = new SearchTreeEntry[typeList.Length + 1];
            entries[0] = new SearchTreeGroupEntry(new GUIContent(module.title));
            entries[0].level = module.layer;

            for (int i = 1; i < entries.Length; ++i)
            {
                string typeName = typeList[i - 1].HasAttribute(out SearchTreeEntryNameAttribute attribute)
                                ? attribute.displayName
                                : StringUtility.ToNicifyName(typeList[i - 1].Name);
                
                entries[i] = new SearchTreeEntry(new GUIContent(typeName));
                entries[i].userData = (typeList[i - 1], module);
                entries[i].level = module.layer + 1;
            }

            return entries;
        }


        private Type[] GetTypesFromTypeCollection(FactoryModule module)
        {
            if (this._subClassTypeOnly == false)
            {
                return new Type[1] { module.targetType };
            }

            TypeCache.TypeCollection s = TypeCache.GetTypesDerivedFrom(module.targetType);

            if (s.Count > 0)
            {
                return s.OrderByNameAndFilterAbstracts();
            }
            else
            {
                return null;
            }
        }
    }
}