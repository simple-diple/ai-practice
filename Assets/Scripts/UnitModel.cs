using System;
using System.Collections.Generic;
using GOAP.Action;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitModel
{
    public byte Owner => _owner;
    public CityModel CityModel => _cityModel;
    public CityModel TargetCityModel => _targetCityModel;
    public Vector3 Position => _position;
    public List<IAction> Actions => _actions;
    public float HealthNormalized => _health / _MAX_HEALTH;
    
    private readonly byte _owner;
    private CityModel _cityModel;
    private CityModel _targetCityModel;
    private UnitModel _targetUnit;
    private Vector3 _target;
    private Vector3 _position;
    private float _health = _MAX_HEALTH;
    private readonly float _attackRange = 5;
    private readonly float _reloadTime = 1;
    private float _currentReloadValue = 1;
    private bool _isDied;
    private List<IAction> _actions;
    private const float _MAX_HEALTH = 2.5f;

    public bool IsIdle => _isIdle;
    public bool IsTargetReached
    {
        get => _isTargetReached;
        set
        {
            _isTargetReached = value;
            if (value)
            {
                _isIdle = true;
            }
        }
    }
    private bool _isIdle;
    private bool _isTargetReached;
    private readonly UnitView _view;
    public bool lastFrameLoopedAction;

    public event Action<Vector3> OnSetTarget;

    public UnitModel(byte owner, UnitView view)
    {
        this._owner = owner;
        _view = view;
        _actions = new List<IAction>(100) { GOAP.Action.Idle.Create(this, null, null) };
    }
    public object MostVisibleEnemy
    {
        get
        {
            //UpdateSectorsNearestTargets();
            //var mostVisibleEnemy = sectors[0].nearestEnemy;
#if DEBUG_MODE
				debugString += dmgDebugString;
				foreach (var p in dmgDebugStrings)
				{
					p.Key.debugString += p.Value;
				}
#endif
            //return mostVisibleEnemy;
            return null;
        }
    }

    public void Attack(UnitModel u)
    {
        _targetUnit = u;
        _isIdle = false;
        var distancePoint = (u._position - _position.normalized * _attackRange);
        OnSetTarget?.Invoke(distancePoint);
    }

    public void SetTarget(CityModel c)
    {
        _targetCityModel = c;
        _isIdle = false;
        OnSetTarget?.Invoke(c.Position);
    }

    private bool FindEnemy()
    {
        if (_targetUnit != null && 
            _targetUnit._isDied == false && 
            Vector3.Distance(_targetUnit._position, _position) <= _attackRange)
        {
            return true;
        }
        
        foreach (var enemy in MapModel.Units[Opponent.Get(_owner)])
        {
            if (Vector3.Distance(enemy._position, _position) <= _attackRange)
            {
                _targetUnit = enemy;
                return true;
            }
        }

        return false;
    }

    private bool Reload(float dt)
    {
        if (_currentReloadValue <= 0)
        {
            _currentReloadValue = _reloadTime;
            return true;
        }

        _currentReloadValue -= dt;
        return false;
    }

    private void Attack(float dt)
    {
        if (Reload(dt) && _targetUnit != null)
        {
            float damage = Random.Range(0.1f, 0.2f) * _health;
            _targetUnit.GetDamage(damage);
        }
    }

    private void GetDamage(float damage)
    {
        Debug.Log("Get damage " + damage);
        _health -= damage;
        if (_health <= 0)
        {
            _health = 0;
            Die();
        }
    }

    private void CheckCity()
    {
        foreach (CityModel city in MapModel.Cities)
        {
            if (Vector3.Distance(city.Position, _position) <= city.Radius)
            {
                MapModel.PlaceUnitToCity(city.Index, this);
                return;
            }
        }
        
        MapModel.RemoveUnitFromCity(this);
    }

    private void Die()
    {
        if (_isDied)
        {
            return;
        }
        
        _isDied = true;
        ReleaseActions();
        Debug.Log("Unit die ");
        MapModel.RemoveUnit(this);
        _view.Dispose();
    }

    public void Tick(float deltaTime)
    {
        if (_isDied)
        {
            return;
        }
        
        CheckCity();
        
        if (FindEnemy())
        {
            Attack(deltaTime);
        }
    }

    public void SetPosition(Vector3 position)
    {
        _position = position;
    }

    public void SetCity(CityModel cityModel)
    {
        _cityModel = cityModel;
    }
    
    public void Idle(bool value)
    {
        if (value)
        {
            SetPosition(_position);
        }
        
        _isIdle = value;
    }

    private void ReleaseActions()
    {
        for (var i = _actions.Count - 1; i >= 0; --i)
        {
            _actions[i].SafeRelease();
        }
        _actions.Clear();
    }
    
    public void CancelActions()
    {
        for (var i = _actions.Count - 1; i >= 0; --i)
        {
            _actions[i].Cancel();
        }
        _actions.Clear();
    }
}