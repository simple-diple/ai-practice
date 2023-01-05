using System;
using System.Linq;
using GOAP.Action;
using GOAP.Condition;
using UnityEngine;
using Idle = GOAP.Precondition.Idle;

public class Planner
{
    private readonly byte _player;
    private CityModel _myCityModel;
    private CityModel _enemyCityModel;
    private readonly Algorithms _algorithms;
    private Graph _graph;
    private Func<CityModel, (CityModel, CityModel)> _func;
    private CityModel _cityModelForAttack;
    private CityModel _cityModelForArmy;

    public Planner(byte player)
    {
	    _player = player;
	    _algorithms = new Algorithms();
    }

    private void OnUnitAdded(UnitModel newUnit)
    {
	    if (newUnit.Owner != _player)
	    {
		    return;
	    }

	    newUnit.Actions.Add(_cityModelForAttack != null
		    ? MoveUnitToCollectArmy(newUnit, _cityModelForAttack)
		    : MoveUnitToCollectArmy(newUnit, _myCityModel));
    }

    private Move MoveUnitToCollectArmy(UnitModel newUnit, CityModel cityModel)
    {
	    Move move = Move.Create(
		    newUnit, 
		    cityModel,
		    UnitEnterTheCityCollectArmy,  
		    UnitEnteredCity.Create(newUnit, cityModel), 
		    Idle.Create(newUnit));

	    return move;
    }

    public void CreateAttackEnemyCityPlan()
    {
	    _graph = new Graph();
	    (_myCityModel, _enemyCityModel) = GetFirstMyAndEnemyCity(_player);
	    _func = _algorithms.ShortestEnemyCity(_graph, _myCityModel, Opponent.Get(_player));
	    
        (_cityModelForAttack, _cityModelForArmy) = _func.Invoke(_enemyCityModel);
        
        MapModel.OnUnitAdded -= OnUnitAdded;
        MapModel.OnUnitAdded += OnUnitAdded;
        
        _cityModelForAttack.OnOwnerChanged -= CityModelForAttackOnOwnerChanged;
        _cityModelForArmy.OnOwnerChanged -= CityModelForArmyOnOwnerChanged;
        
        _cityModelForAttack.OnOwnerChanged += CityModelForAttackOnOwnerChanged;
        _cityModelForArmy.OnOwnerChanged += CityModelForArmyOnOwnerChanged;
        
        foreach (var unitModel in MapModel.Units[_player])
        {
	        unitModel.Actions.Add(MoveUnitToCollectArmy(unitModel, _cityModelForArmy));
        }

    }

    private void CityModelForArmyOnOwnerChanged(byte newOwner)
    {
	    _graph = new Graph();
	    foreach (var neighbour in _graph.AdjacencyList[_cityModelForAttack])
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
	    
	    CreateAttackEnemyCityPlan();
    }

    private void CityArmyReadyForAttack(UnitModel unit, bool complete)
    {
	    if (complete == false)
	    {
		    return;
	    }
	    
	    foreach (var unitModel in unit.CityModel.GetUnitsByOwner(_player))
	    {
		    Move move = Move.Create(
			    unitModel, 
			    _cityModelForAttack,
			    UnitsEnteredInEnemyCity,  
			    UnitEnteredCity.Create(unitModel, _cityModelForAttack), 
			    Idle.Create(unitModel));
            
		    unitModel.Actions.Add(move);
	    }
    }

    private void UnitEnterTheCityCollectArmy(UnitModel unit, bool complete)
    {
	    if (complete == false)
	    {
		    return;
	    }

	    if (unit.CityModel == null)
	    {
		    return;
	    }

	    var condition = EnemyWeakInCity.Create(_cityModelForArmy, _enemyCityModel);

	    if (condition.IsComplete())
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
				    condition);

			    unitModel.Actions.Add(move);
		    }
	    }

    }

    private void UnitsEnteredInEnemyCity(UnitModel unit, bool iscanceled)
    {
	    Debug.Log("!!!We are in enemy cityModel!!!");
    }

    public void ManualUpdate(float dt)
    {
	    var units = MapModel.Units[_player];
	    var unitsCount = units.Count;
	    
	    for (var unitIndex = 0; unitIndex < unitsCount; ++unitIndex)
	    {
		    var unit = units[unitIndex];

		    var actions = unit.Actions;
		    var leftTime = dt;

		    for (var i = 0; i < actions.Count; i++)
		    {
			    if (actions[i].CheckCompleteCondition)
			    {
				    //Debug.Log(unit + " <color=white> STOP ACTION </color> " + actions[i]);
				    for (var c = actions.Count - 1; c >= i; c--)
				    {
					    actions[c].Cancel();
				    }

				    actions.RemoveRange(i, actions.Count - i);
				    if (i == 0)
				    {
					    unit.Idle(true);
				    }

				    break;
			    }
		    }

		    if (actions.Count > 0)
		    {
			    var preconditionAction = actions[actions.Count - 1].PreconditionAction;
			    while (preconditionAction != null)
			    {
				    actions.Add(preconditionAction);
#if DEBUG_MODE
						if (PlayerPreferences.instance.enableLoopedActionsFinder)
						{
							actionsFinder.AddActionByObject(unit, preconditionAction.GetType(), Time.realtimeSinceStartup);
						}
#endif
				    preconditionAction = preconditionAction.PreconditionAction;
			    }
		    }

//#if LOOP_TRACKING
		    var justAdded = false;
//#endif
		    var loopsCount = 0;
		    while (leftTime > 0 && loopsCount++ < 20)
		    {
			    if (actions.Count == 0)
			    {
				    unit.Idle(true);
			    }

//#if LOOP_TRACKING
			    var startLoopTime = leftTime;
//#endif
			    var action = actions[actions.Count - 1];
			    leftTime = action.Run(leftTime);
			    if (justAdded)
			    {
				    justAdded = false;
				    if (leftTime >= startLoopTime)
				    {
#if LOOP_TRACKING
							Debug.LogWarning("Action " + actions[actions.Count - 1] + " for unit " + unit.name + " was added and removed in 1 frame\n" + actions[actions.Count - 1].CreateStackTrace);
#endif
					    leftTime = 0;
					    actions.RemoveAt(actions.Count - 1);
				    }
			    }

			    if (leftTime > 0)
			    {
				    actions.RemoveAt(actions.Count - 1);
				    if (actions.Count > 0)
				    {
					    var lastAction = actions[actions.Count - 1];
					    if (lastAction.AlwaysAskPrecondition && !lastAction.CheckCompleteCondition)
					    {
						    var preconditionAction = lastAction.PreconditionAction;
						    while (preconditionAction != null)
						    {
							    actions.Add(preconditionAction);
//#if LOOP_TRACKING
							    justAdded = true;
//#endif
#if DEBUG_MODE
									if (PlayerPreferences.instance.enableLoopedActionsFinder)
									{
										actionsFinder.AddActionByObject(unit, preconditionAction.GetType(), Time.realtimeSinceStartup);
									}
#endif
							    preconditionAction = preconditionAction.PreconditionAction;
						    }
					    }
				    }
			    }
		    }

		    if (leftTime > 0)
		    {
			    if (!unit.lastFrameLoopedAction)
			    {
				    var dbgMsg = "Looped action: leftTime = " + leftTime + " / loopsCount = " + loopsCount;
				    for (var ai = 0; ai < actions.Count; ai++)
				    {
					    dbgMsg += "\n" + ai + "Action " + actions[ai];
				    }

				    Debug.LogWarning(dbgMsg);
			    }

			    unit.lastFrameLoopedAction = true;
		    }
		    else
		    {
			    unit.lastFrameLoopedAction = false;
		    }
	    }

#if DEBUG_MODE
			if (PlayerPreferences.instance.enableLoopedActionsFinder)
			{
				actionsFinder.Process(Time.realtimeSinceStartup, Time.frameCount);
			}
#endif
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
}