namespace GOAP.Action
{
    public interface IAction
    {
        event OnActionCompleteDelegate onActionComplete;
        IAction PreconditionAction { get; }
        float Priority { get; set; }
        bool AlwaysAskPrecondition { get; }
        bool CheckCompleteCondition { get; }

#if DEBUG_MODE
		string DebugString { get; set; }
#endif
#if LOOP_TRACKING
		string CreateStackTrace { get; }
#endif

        float Run(float dt);
        void Cancel();
        void Release();
        void SafeRelease();

    }
}