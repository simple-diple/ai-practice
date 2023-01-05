namespace GOAP.Condition
{
    public class CityCaptured : ObjectsPool<CityCaptured>, ICondition
    {
        private CityModel _cityModel;
        private byte _player;
        
        public static CityCaptured Create(CityModel c, byte player)
        {
            var condition = Allocate();
            condition._player = player;
            condition._cityModel = c;
            return condition;
        }
        
        public bool IsComplete()
        {
            return _cityModel.Owner == _player;
        }
    }
}