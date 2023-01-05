namespace GOAP.Action
{
    public sealed class Move : BaseAction<Move>
    {
        private CityModel _cityModel;

        public static Move Create(UnitModel u
            , CityModel cityModel
            , OnActionCompleteDelegate d
            , Condition.ICondition c
            , Precondition.IPrecondition pc
         )
        {
            var a = Allocate();
            a.InitAction(pc, c, u, true);
            a.onActionComplete += d;
            a._cityModel = cityModel;
            u.SetTarget(cityModel);

            for (var index = u.Actions.Count - 1; index >= 0; index--)
            {
                var action = u.Actions[index];
                if (action is Move)
                {
                    u.Actions.Remove(action);
                }
            }

            return a;
        }

        protected override float Process(float dt)
        {
            if (unit == null)
                return dt;
            
            return Equals(unit.CityModel, _cityModel) ? dt : 0;
        }
    }
}