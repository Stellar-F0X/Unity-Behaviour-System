using System;
using System.Collections.Generic;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    /// <summary> Blackboard 교체될 때, 이미 등록되어 있는 BlackboardVariable을 해제하는 용도로 사용되는 객체. </summary>
    public class VariablesInitAdapter : IVisitPropertyAdapter<GraphDictionary>,
                                        IVisitPropertyAdapter<Graph>,
                                        IVisitPropertyAdapter<NodeDictionary>,
                                        IVisitPropertyAdapter<NodeBase>,
                                        IVisitContravariantPropertyAdapter<BlackboardVariable>,
                                        IVisitPropertyAdapter<List<Transition>>, 
                                        IVisitPropertyAdapter<Transition>
    {
        public VariablesInitAdapter(GraphVisitor dataContainer)
        {
            _dataContainer = dataContainer;

            _behaviorTreeBag = PropertyBag.GetPropertyBag<BehaviorTree>();
            _stateMachineBag = PropertyBag.GetPropertyBag<StateMachine>();
        }

        private readonly GraphVisitor _dataContainer;

        private readonly IPropertyBag<BehaviorTree> _behaviorTreeBag;
        private readonly IPropertyBag<StateMachine> _stateMachineBag;



        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            if (_dataContainer.debugMode)
            {
                Debug.Log($"nodeDictionary element count : {value.Count}");
            }

            IPropertyBag<Dictionary<UGUID, NodeBase>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>();
            Dictionary<UGUID, NodeBase> dictionaryValue = (Dictionary<UGUID, NodeBase>)value;
            propertyBag.Accept(_dataContainer, ref dictionaryValue);
        }
        


        public void Visit<TContainer>(in VisitContext<TContainer, NodeBase> context, ref TContainer container, ref NodeBase value)
        {
            context.ContinueVisitation(ref container, ref value);
        }
        
        

        public void Visit<TContainer>(in VisitContext<TContainer, Graph> context, ref TContainer container, ref Graph value)
        {
            _dataContainer.currentGraph = value;

            if (_dataContainer.debugMode)
            {
                Debug.Log($"visit {value.name}({value.graphType}) graph.");
            }

            switch (value)
            {
                case BehaviorTree behaviorTree: _behaviorTreeBag.Accept(_dataContainer, ref behaviorTree); break;

                case StateMachine stateMachine: _stateMachineBag.Accept(_dataContainer, ref stateMachine); break;

                //TODO: GOAP

                default: Debug.LogError("Invalid graph type"); break;
            }

            _dataContainer.currentGraph = null;
        }



        public void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
        {
            IPropertyBag<Dictionary<UGUID, Graph>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, Graph>>();
            Dictionary<UGUID, Graph> dictionaryValue = value as Dictionary<UGUID, Graph>;
            propertyBag.Accept(_dataContainer, ref dictionaryValue);
        }



        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
        {
            if (value is null || value.isGlobal == false)
            {
                return;
            }
            
            if (_dataContainer.debugMode)
            {
                Debug.Log(context.Property.Name);
            }

            //Activator.CreateInstance로 생성자를 호출하여 BlackboardVariable<T>와 Variable<T>를 생성한다.
            BlackboardVariable newValue = Activator.CreateInstance(value.GetType()) as BlackboardVariable;
            Debug.Assert(newValue is not null, "Failed to create a new BlackboardVariable instance.");
            context.Property.SetValue(ref container, newValue);
        }
        
        
        
        public void Visit<TContainer>(in VisitContext<TContainer, Transition> context, ref TContainer container, ref Transition value)
        {
            if (_dataContainer.debugMode)
            {
                Debug.Log($"{context.Property.Name}  Name: {value.name}  Des: {value.description}");
            }

            if (value.conditions.modules.Count == 0)
            {
                return;
            }

            IPropertyBag<List<ConditionModule>> bag = PropertyBag.GetPropertyBag<List<ConditionModule>>();
            List<ConditionModule> conditions = value.conditions.modules;
            bag.Accept(_dataContainer, ref conditions);
        }



        public void Visit<TContainer>(in VisitContext<TContainer, List<Transition>> context, ref TContainer container, ref List<Transition> value)
        {
            context.ContinueVisitation(ref container, ref value);
        }
    }
}