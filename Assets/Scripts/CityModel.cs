using System;
using System.Collections.Generic;
using UnityEngine;

public class CityModel
{
    public char Index => _index;
    public byte Owner => _owner;
    public Vector3 Position => _cityView ? _cityView.transform.position : Vector3.zero;
    public IEnumerable<UnitModel> Units => _units;
    public int UnitsCount => _units.Count;
    public float Radius => _radius;

    public event Action<byte> OnOwnerChanged;

    private readonly char _index;
    private byte _owner;
    private readonly List<UnitModel> _units;
    private CityView _cityView;
    private float _radius = 7;

    public CityModel(char i, byte o)
    {
        _index = i;
        _owner = o;
        _units = new List<UnitModel>();
    }

    public IEnumerable<UnitModel> GetUnitsByOwner(byte owner)
    {
        List<UnitModel> result = new List<UnitModel>();
        
        foreach (var unit in _units)
        {
            if (unit.Owner == owner)
            {
                result.Add(unit);
            }
        }

        return result;
    }

    public void ConnectView(CityView view)
    {
        _cityView = view;
    }

    public void AddUit(UnitModel unitModel)
    {
        unitModel.SetCity(this);
        
        if (_units.Contains(unitModel) == false)
        {
            _units.Add(unitModel);
        }

        CheckOwner();
    }

    public void CheckOwner()
    {
        if (IsEnemiesOnly() == false)
        {
            return;
        }

        if (CanCapture())
        {
            var enemy = Opponent.Get(_owner);
            _owner = enemy;
            _cityView.SetOwner(enemy);
            MapModel.UpdateGraph();
            OnOwnerChanged?.Invoke(enemy);
        }
    }

    private bool IsEnemiesOnly()
    {
        if (_units.Count == 0)
        {
            return false;
        }
        
        foreach (var unit in _units)
        {
            if (unit.Owner == _owner)
            {
                return false;
            }
        }

        return true;
    }
    
    public void RemoveUnit(UnitModel unitModel)
    {
        unitModel.SetCity(null);
        _units.Remove(unitModel);
        CheckOwner();
    }

    public override int GetHashCode()
    {
        return _index;
    }

    public override bool Equals(object other)
    {
        return _index == ((CityModel)other)!._index;
    }
    
    public int GetUnitsCountByOwner(byte owner)
    {
        int result = 0;

        foreach (var unit in _units)
        {
            if (unit.Owner == owner)
            {
                result++;
            }
        }

        return result;
    }
    
    public float GetUnitsHealthByOwner(byte owner)
    {
        float result = 0;

        foreach (var unit in _units)
        {
            if (unit.Owner == owner)
            {
                result += unit.HealthNormalized;
            }
        }

        return result;
    }

    public bool CanCapture()
    {
        var enemy = Opponent.Get(_owner);

        foreach (CityModel neighbor in MapModel.Graph.AdjacencyList[this])
        {
            var city = MapModel.GetCity(neighbor._index);
            if (city._owner == enemy)
            {
                return true;
            }
        }

        return false;
    }
}