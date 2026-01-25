using System.Linq;
using TaskStreamer.Runtime.Utility;
using UnityEngine.Assertions;

namespace TaskStreamer.Runtime
{
    internal static class BlackboardVariableHelper
    {
        /// <summary> 블랙보드에서 해당 Variable이 유효한지 검사합니다. </summary>
        public static bool IsValidInBlackboard(this BlackboardAsset blackboard, BlackboardVariable variable)
        {
            Assert.IsNotNull(variable);

            if (blackboard == null)
            {
                return false;
            }

            return blackboard.FindVariable(variable.guid) is not null;
        }

        
        
        /// <summary> 블랙보드에서 유효하지 않은 Shared Variable을 새 Local Variable로 교체합니다. </summary>
        public static BlackboardVariable SyncWithBlackboard(this BlackboardAsset blackboard, BlackboardVariable variable)
        {
            Assert.IsNotNull(variable);

            if (variable.isShared == false || blackboard.IsValidInBlackboard(variable))
            {
                return variable;
            }
            else
            {
                return TSObjectFactory.CreateBlackboardVariable(variable.genericVariableType);
            }
        }
        
        

        /// <summary> 런타임에서 BlackboardVariable을 바인딩합니다. Local이면 복제, Shared면 블랙보드에서 찾아 연결합니다. </summary>
        public static BlackboardVariable BindForRuntime(BlackboardVariable source, GraphContext context)
        {
            Assert.IsNotNull(source);

            if (source.isShared == false)
            {
                return source.Duplicate();
            }
            
            Assert.IsFalse(context.blackboard == null || context.blackboard.count == 0, "Shared variable requires a valid blackboard.");
            
            BlackboardVariable shared = TSObjectFactory.CreateSharedBlackboardVariable(source.genericVariableType, context.blackboard, source.guid);
            Assert.IsNotNull(shared, "Variable not found in blackboard.");
            
            shared.usage = source.usage;
            return shared;
        }
    }
}
