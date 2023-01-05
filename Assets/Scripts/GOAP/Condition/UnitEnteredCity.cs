namespace GOAP.Condition
{
    public class UnitEnteredCity : ObjectsPool<UnitEnteredCity>, ICondition
    {
        private UnitModel _unit;
        private CityModel _cityModel;
        
        public static UnitEnteredCity Create(UnitModel u, CityModel c)
        {
            var condition = Allocate();
            condition._unit = u;
            condition._cityModel = c;
            return condition;
        }
        
        public bool IsComplete()
        {
            return Equals(_unit.CityModel, _cityModel);
        }
    }
}