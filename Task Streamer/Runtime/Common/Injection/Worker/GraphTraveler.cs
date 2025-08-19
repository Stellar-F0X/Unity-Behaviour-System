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
    public class GraphTraveler : DefaultVisitWorker
    {
        public GraphTraveler(BlackboardAsset blackboard, GraphAsset graphAsset, TaskStreamer taskStreamer)
        {
            this._blackboard = blackboard;
            this._graphAsset = graphAsset;
            this._taskStreamer = taskStreamer;
        }
        
        private readonly TaskStreamer _taskStreamer;
        private readonly BlackboardAsset _blackboard;
        private readonly GraphAsset _graphAsset;


        public TaskStreamer taskStreamer
        {
            get { return _taskStreamer; }
        }

        public BlackboardAsset blackboard
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
    }
}