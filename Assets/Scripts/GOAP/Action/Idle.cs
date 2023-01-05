namespace GOAP.Action
{
    public sealed class Idle : BaseAction<Idle>
    {

        public static Idle Create(UnitModel u, OnActionCompleteDelegate action, Precondition.IPrecondition p, Condition.ICondition c = null)
        {
            var a = Allocate();
            a.InitAction(p, c, u);
            a.onActionComplete += action;
            return a;
        }

        protected override float Process(float dt)
        {
            unit?.Idle(true);
            return 0f;
        }

        protected override void OnComplete(bool isCanceled)
        {
            unit?.Idle(false);
            base.OnComplete(isCanceled);
        }

#if DEBUG_MODE
		protected override string AdditionalActionData()
		{
			return "Unit: " + unit;
		}
#endif
    }
}