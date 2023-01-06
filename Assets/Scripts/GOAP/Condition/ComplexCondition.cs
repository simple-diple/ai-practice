using System;
using System.Collections.Generic;

namespace GOAP.Condition
{

    public enum ConditionResult
    {
        AllComplete,
        OneComplete
    }
    
    public class ComplexCondition : ObjectsPool<ComplexCondition>, ICondition
    {
        private List<ICondition> _conditions;
        private ConditionResult _result;

        public static ComplexCondition Create(List<ICondition> conditions, ConditionResult result)
        {
            var condition = Allocate();
            condition._conditions = conditions;
            condition._result = result;
            return condition;
        }
        
        public bool IsComplete()
        {
            switch (_result)
            {
                case ConditionResult.AllComplete:
                    foreach (ICondition condition in _conditions)
                    {
                        if (condition.IsComplete() == false)
                        {
                            return false;
                        }
                    }

                    return true;
                   
                case ConditionResult.OneComplete:
                    foreach (ICondition condition in _conditions)
                    {
                        if (condition.IsComplete())
                        {
                            return true;
                        }
                    }

                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        public override void Release()
        {
            foreach (var condition in _conditions)
            {
                condition.Release();
            }
            
            base.Release();
        }
    }
}