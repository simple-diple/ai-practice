namespace GOAP.Action
{
    public sealed class WaitTime : BaseAction<WaitTime>
    {
        
        private float _time;

        public static WaitTime Create(UnitModel u = null, OnActionCompleteDelegate d = null, float seconds = 1)
        {
            var a = Allocate();
            a.InitAction(null, null, u, false);
            a.onActionComplete += d;
            a._time = seconds;
            return a;
        }

        protected override float Process(float dt)
        {
            _time -= dt;
            dt = _time > 0 ? 0f : dt;
            return dt;
        }

    }
}