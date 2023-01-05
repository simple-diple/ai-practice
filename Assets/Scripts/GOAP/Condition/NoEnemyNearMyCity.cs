using UnityEngine;

namespace GOAP.Condition
{
    public class NoEnemyNearMyCity : ObjectsPool<NoEnemyNearMyCity>, ICondition
    {
        private CityModel _city;
        private const float _RADIUS = 15f;
        
        public static NoEnemyNearMyCity Create(CityModel city)
        {
            var condition = Allocate();
            condition._city = city;
            return condition;
        }

        public bool IsComplete()
        {
            var enemy = Opponent.Get(_city.Owner);

            foreach (var unit in MapModel.Units[enemy])
            {
                if (Vector3.Distance(unit.Position, _city.Position) <= _RADIUS)
                {
                    return false;
                }
            }

            return true;
        }
    }
}