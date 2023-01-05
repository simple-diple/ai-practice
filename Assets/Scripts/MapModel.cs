using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapModel
{
    public static IEnumerable<CityModel> Cities => instance._cities;
    public static Road[] Roads => instance._roads;
    public static Dictionary<byte, List<UnitModel>> Units => instance._units;
    public static bool Exists => instance != null;
    public static Graph Graph => instance._graph;

    public static event Action<UnitModel> OnUnitAdded;

    private IEnumerable<CityModel> _cities;
    private Road[] _roads;
    private Dictionary<byte, List<UnitModel>> _units;
    private Dictionary<char, CityModel> _citiesMap;
    private static MapModel instance;
    private Graph _graph;

    public MapModel(IEnumerable<CityModel> cities, Road[] roads)
    {
        _cities = cities;
        _roads = roads;
        _units = new Dictionary<byte, List<UnitModel>>();
        _units[1] = new List<UnitModel>();
        _units[2] = new List<UnitModel>();

        _citiesMap = new Dictionary<char, CityModel>();

        foreach (CityModel city in _cities)
        {
            _citiesMap.Add(city.Index, city);
        }
        
        instance = this;
    }

    public static void UpdateGraph()
    {
        instance._graph = new Graph();
    }

    public static CityModel GetCity(char index)
    {
        return instance._citiesMap[index];
    }

    public static int GetOwnCitiesCount(byte owner)
    {
        int result = 0;
        foreach (var city in Cities)
        {
            if (city.Owner == owner)
            {
                result++;
            }
        }

        return result;
    }

    public static void AddUnit(UnitModel unitModel)
    {
        instance._units[unitModel.Owner].Add(unitModel);
        OnUnitAdded?.Invoke(unitModel);
    }

    public static CityModel GetClosestCity(UnitModel unit)
    {
        CityModel bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = unit.Position;
        foreach (var potentialTarget in MapModel.Cities)
        {
            if (potentialTarget.Owner != unit.Owner)
            {
                continue;
            }

            Vector3 directionToTarget = potentialTarget.Position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }

        return bestTarget;
    }

    public static void PlaceUnitToCity(char cityIndex, UnitModel unitModel)
    {
        var city = instance._citiesMap[cityIndex];
        city.AddUit(unitModel);
    }
    
    public static void RemoveUnitFromCity(UnitModel unitModel)
    {
        if (unitModel.CityModel == null)
        {
            return;
        }
        
        var city = unitModel.CityModel;
        city.RemoveUnit(unitModel);
    }

    public static void RemoveUnit(UnitModel unitModel)
    {
        RemoveUnitFromCity(unitModel);
        instance._units[unitModel.Owner].Remove(unitModel);
    }
}