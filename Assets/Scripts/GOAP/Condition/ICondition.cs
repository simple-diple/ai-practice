using System.Collections.Generic;

namespace GOAP.Condition
{
    public interface ICondition
    {
        bool IsComplete();
        void Release();
    }
}