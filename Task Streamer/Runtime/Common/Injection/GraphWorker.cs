using System;
using System.Collections.Generic;
using System.Reflection;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    //TODO: 나중에 JSON(Or BSON)을 사용한 DataBinding으로 교체하기 전까지 임시 사용.
    public class GraphWorker : PropertyVisitor
    {
        public GraphWorker(Blackboard blackboard, GraphAsset graphAsset, TaskStreamer taskStreamer)
        {
            this._blackboard = blackboard;
            this._graphAsset = graphAsset;
            this._taskStreamer = taskStreamer;
            
            _Providers.Add(typeof(List<NodeGroup>));
            _Providers.Add(typeof(List<Transition>));
            _Providers.Add(typeof(List<ConditionModule>));
            _Providers.Add(typeof(KeyValuePair<UGUID, Graph>));
            _Providers.Add(typeof(KeyValuePair<UGUID, NodeBase>));
        }

        private readonly static HashSet<ICustomAttributeProvider> _Providers = new HashSet<ICustomAttributeProvider>();

        private readonly TaskStreamer _taskStreamer;
        private readonly Blackboard _blackboard;
        private readonly GraphAsset _graphAsset;


        public TaskStreamer taskStreamer
        {
            get { return _taskStreamer; }
        }

        public Blackboard blackboard
        {
            get { return _blackboard; }
        }

        public GraphAsset graphAsset
        {
            get { return _graphAsset; }
        }

        public Graph currentGraph
        {
            get;
            set;
        }


        protected override bool IsExcluded<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            Type type = property.DeclaredValueType();

            if (_Providers.Contains(type))
            {
                return false; //Ignore filtering
            }
            
            if (type.HasAttribute<ReadableAttribute>(true))
            {
                _Providers.Add(type);
                return false; //Ignore filtering
            }
            else
            {
                return true; //Filtering this TValue type
            }
        }
    }
}