using System;
using System.Linq;
using GOAP.Action;
using GOAP.Condition;
using UnityEngine;
using Idle = GOAP.Precondition.Idle;

public class CaptureAllCitiesGoal
{
    private readonly byte _player;
    private CityModel _myCityModel;
    private CityModel _enemyCityModel;
    private readonly Algorithms _algorithms;
    private Func<CityModel, (CityModel, CityModel)> _func;
    private CityModel _cityModelForAttack;
    private CityModel _cityModelForArmy;
	
    public CaptureAllCitiesGoal(byte player)
    {
        _player = player;
        _algorithms = new Algorithms();
        MapModel.OnUnitAdded += OnUnitAdded;
        
        CreateAttackEnemyCityPlan();
    }
	
    private void OnUnitAdded(UnitModel newUnit)
    {
        if (newUnit.Owner != _player)
        {
            return;
        }

        newUnit.Actions.Add(_cityModelForArmy != null
            ? MoveUnitToCollectArmy(newUnit, _cityModelForArmy)
            : MoveUnitToCollectArmy(newUnit, _myCityModel));
    }
	
    private Move MoveUnitToCollectArmy(UnitModel newUnit, CityModel cityModel)
    {
        UIDebug.SetTarget(cityModel, TargetType.CollectArmy, _player);
	    
        Move move = Move.Create(
            newUnit, 
            cityModel,
            UnitEnterTheCityCollectArmy,  
            UnitEnteredCity.Create(newUnit, cityModel), 
            Idle.Create(newUnit));

        return move;
    }

    private void CreateAttackEnemyCityPlan()
    {
        (_myCityModel, _enemyCityModel) = GetFirstMyAndEnemyCity(_player);
        _func = _algorithms.ShortestEnemyCity(_myCityModel, Opponent.Get(_player));
	    
        (_cityModelForAttack, _cityModelForArmy) = _func.Invoke(_enemyCityModel);

        _cityModelForAttack.OnOwnerChanged -= CityModelForAttackOnOwnerChanged;
        _cityModelForArmy.OnOwnerChanged -= CityModelForArmyOnOwnerChanged;
        
        _cityModelForAttack.OnOwnerChanged += CityModelForAttackOnOwnerChanged;
        _cityModelForArmy.OnOwnerChanged += CityModelForArmyOnOwnerChanged;
        
        foreach (var unitModel in MapModel.Units[_player])
        {
            unitModel.Actions.Add(MoveUnitToCollectArmy(unitModel, _cityModelForArmy));
        }

    }
	
    private (CityModel, CityModel) GetFirstMyAndEnemyCity(byte player)
    {
        CityModel myCityModel = null;
        CityModel enemyCityModel = null;
        
        foreach (var city in MapModel.Cities)
        {
            if (myCityModel != null && enemyCityModel != null)
            {
                break;
            }
            
            if (city.Owner == player)
            {
                myCityModel = city;
            }
            else
            {
                enemyCityModel = city;
            }
        }

        return (myCityModel, enemyCityModel);
    }

    private void CityModelForArmyOnOwnerChanged(byte newOwner)
    {
        foreach (var neighbour in MapModel.Graph.AdjacencyList[_cityModelForAttack])
        {
            var city = MapModel.GetCity(neighbour.Index);
            if (city.Owner == _player)
            {
                return;
            }
        }

        foreach (var unit in MapModel.Units[_player])
        {
            unit.CancelActions();
            unit.Actions.Add(GOAP.Action.Idle.Create(unit, null, null));
        }
	    
        CreateAttackEnemyCityPlan();
    }
    private void CityModelForAttackOnOwnerChanged(byte newOwner)
    {
        if (MapModel.GetOwnCitiesCount(_player) == MapModel.Cities.Count())
        {
            Debug.Log("!!!VICTORY!!!");
            return;
        }
	    
        var noEnemyNearMyCity = NoEnemyNearMyCity.Create(_cityModelForAttack);
	    
        if (noEnemyNearMyCity.IsComplete() == false)
        {
            foreach (var unit in MapModel.Units[_player])
            {
                UIDebug.SetTarget(_cityModelForAttack, TargetType.Protect, _player);
                GOAP.Action.Idle wait = GOAP.Action.Idle.Create(unit, UnitEnterTheCityCollectArmy, null, noEnemyNearMyCity);
                unit.Actions.Add(wait);
            }
            return;
        }

        noEnemyNearMyCity.Release();
        CreateAttackEnemyCityPlan();
    }


    private void CityArmyReadyForAttack(UnitModel unit, bool isCanceled)
    {

        if (_cityModelForAttack.Owner == _player)
        {
            CreateAttackEnemyCityPlan();
            return;
        }
	    
        UIDebug.SetTarget(_cityModelForAttack, TargetType.Attack, _player);
	    
        foreach (var unitModel in _cityModelForArmy.GetUnitsByOwner(_player))
        {
            Move move = Move.Create(
                unitModel, 
                _cityModelForAttack,
                UnitsEnteredInEnemyCity,  
                CityCaptured.Create(_cityModelForAttack, _player), 
                Idle.Create(unitModel));
            
            unitModel.Actions.Add(move);
        }
    }

    private void UnitEnterTheCityCollectArmy(UnitModel unit, bool isCanceled)
    {

        if (unit.CityModel == null)
        {
            return;
        }

        if (_cityModelForAttack.Owner == _player)
        {
            CreateAttackEnemyCityPlan();
            return;
        }
	    
        var enemyWeakInCity = EnemyWeakInCity.Create(_cityModelForArmy, _cityModelForAttack);

        if (enemyWeakInCity.IsComplete())
        {
            foreach (var unitModel in unit.CityModel.GetUnitsByOwner(_player))
            {
                CityArmyReadyForAttack(unitModel, true);
            }
        }
	    
        else
        {
            foreach (var unitModel in unit.CityModel.GetUnitsByOwner(_player))
            {
                GOAP.Action.Idle move = GOAP.Action.Idle.Create(
                    unitModel,
                    CityArmyReadyForAttack,
                    null,
                    enemyWeakInCity);

                unitModel.Actions.Add(move);
            }
        }

    }

    private void UnitsEnteredInEnemyCity(UnitModel unit, bool iscanceled)
    {
        if (unit.CityModel != null && unit.CityModel.CanCapture() == false)
        {
            CreateAttackEnemyCityPlan();
        }
    }
	
	
}