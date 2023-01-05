namespace GOAP.Condition
{
    public class EnemyWeakInCity : ObjectsPool<EnemyWeakInCity>, ICondition
    {
        private CityModel _playerCityModel;
        private CityModel _enemyCityModel;
        
        public static EnemyWeakInCity Create(CityModel playerCityModel, CityModel enemyCityModel)
        {
            var condition = Allocate();
            condition._playerCityModel = playerCityModel;
            condition._enemyCityModel = enemyCityModel;
            return condition;
        }
        
        public bool IsComplete()
        {
            return _enemyCityModel.GetUnitsCountByOwner(_enemyCityModel.Owner) <
                   _playerCityModel.GetUnitsCountByOwner(_playerCityModel.Owner);
        }
    }
}