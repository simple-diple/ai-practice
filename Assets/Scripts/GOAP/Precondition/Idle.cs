using GOAP.Action;

namespace GOAP.Precondition
{
    public class Idle : ObjectsPool<Idle>, IPrecondition
    {
        private UnitModel _unit;

        public static Idle Create(UnitModel u)
        {
            var p = Allocate();
            p._unit = u;
            return p;
        }

        public IAction NextAction => null;
    }

}