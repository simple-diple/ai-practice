namespace GOAP.Condition
{
    public interface ICondition
    {
        bool IsComplete();
        void Release();
    }
}