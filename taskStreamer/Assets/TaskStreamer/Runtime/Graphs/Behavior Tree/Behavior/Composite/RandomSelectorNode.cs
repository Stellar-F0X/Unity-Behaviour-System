using System;
using System.Collections.Generic;
using Unity.Properties;
using Random = UnityEngine.Random;

namespace TaskStreamer.Runtime.BT
{
    [Serializable, GeneratePropertyBag, TaskDescription]
    public class RandomSelectorNode : CompositeNode
    {
        private readonly List<int> _randomIndices = new List<int>();
        
        private int _currentRandomIndex;


        
        public override void OnAwake()
        {
            if (children is null || children.Count == 0)
            {
                return;
            }
            
            for (int i = 0; i < children.Count; i++)
            {
                this._randomIndices.Add(i);
            }
        }


        protected override void OnEnter()
        {
            this.ShuffleIndices(this._randomIndices);
            
            this._currentChildrenIndex = this._randomIndices[_currentRandomIndex];
        }


        protected override Status OnUpdate()
        {
            if (children is null || children.Count == 0)
            {
                return Status.Failure;
            }
            
            switch (children[_currentChildrenIndex].UpdateNode())
            {
                case Status.Success: return Status.Success;

                case Status.Running: return Status.Running;

                case Status.Failure: _currentChildrenIndex = _randomIndices[++_currentRandomIndex]; break;
            }

            if (_currentRandomIndex == children.Count)
            {
                return Status.Failure;
            }
            else
            {
                return Status.Running;
            }
        }


        private void ShuffleIndices(List<int> indices)
        {
            int temp = 0;
            int tempIndex = 0;

            for (int i = indices.Count - 1; i > 0; i--)
            {
                tempIndex = Random.Range(0, i + 1);

                temp = indices[i];
                indices[i] = indices[tempIndex];
                indices[tempIndex] = temp;
            }
        }
    }
}