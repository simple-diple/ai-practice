namespace GOAP.Condition
{
    public class CityCanCaptured : ObjectsPool<CityCanCaptured>, ICondition
    {
        private CityModel _cityModel;

        public static CityCanCaptured Create(CityModel c)
        {
            var condition = Allocate();
            condition._cityModel = c;
            return condition;
        }
        
        public bool IsComplete()
        {
            return _cityModel.CanCapture();
        }
    }
}