using System;
using System.Collections.Generic;
using System.Linq;
using GOAP.Action;
using GOAP.Condition;
using UnityEngine;

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

        newUnit.Actions.Add(MoveUnitToCollectArmy(newUnit, _cityModelForArmy));
    }
	
    private Move MoveUnitToCollectArmy(UnitModel newUnit, CityModel cityModel)
    {
        UIDebug.SetTarget(cityModel, TargetType.CollectArmy, _player);
        
        var condition = ComplexCondition.Create(new List<ICondition>()
        {
            UnitEnteredCity.Create(newUnit, cityModel),
            CityCaptured.Create(cityModel, Opponent.Get(_player))
        }, 
            ConditionResult.OneComplete);
	    
        Move move = Move.Create(
            newUnit, 
            cityModel,
            UnitEnterTheCityCollectArmy,  
            condition, 
            null);

        return move;
    }

    private void CreateAttackEnemyCityPlan()
    {
        if (MapModel.GetOwnCitiesCount(_player) == 0)
        {
            Debug.Log("!!!DEFEAT!!!");
            return;
        }
        
        if (MapModel.GetOwnCitiesCount(_player) == MapModel.Cities.Count())
        {
            Debug.Log("!!!VICTORY!!!");
            return;
        }

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
            unit.ReleaseActions();
            unit.Actions.Add(Idle.Create(unit, null, null));
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
                Idle wait = Idle.Create(unit, UnitEnterTheCityCollectArmy, null, noEnemyNearMyCity);
                unit.Actions.Add(wait);
            }
            return;
        }

        noEnemyNearMyCity.Release();
        CreateAttackEnemyCityPlan();
    }


    private void CityArmyReadyForAttack(UnitModel unit, bool isCanceled)
    {

        if (_cityModelForAttack.Owner == _player || _cityModelForAttack.CanCapture() == false)
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
                null);
            
            unitModel.Actions.Add(move);
        }
    }

    private void UnitEnterTheCityCollectArmy(UnitModel unit, bool isCanceled)
    {
        if (unit.TargetCityModel.Owner != _player)
        {
            unit.ReleaseActions();
            CreateAttackEnemyCityPlan();
            return;
        }

        if (unit.CityModel == null)
        {
            return;
        }

        if (_cityModelForAttack.Owner == _player || _cityModelForAttack.CanCapture() == false)
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
                Idle move = Idle.Create(
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