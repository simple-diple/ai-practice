using GOAP.Action;

namespace GOAP.Precondition
{
    public interface IPrecondition
    {
        IAction NextAction { get; }
        void Release();
    }
}