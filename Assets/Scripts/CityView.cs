using System;
using System.Collections.Generic;
using UnityEngine;

public class CityView : MonoBehaviour
{
    [SerializeField] private string nodeName;
    [SerializeField] private UnitSpawner unitSpawner;

    public char Node => nodeName[0];
    public byte Owner => _cityModel.Owner;
    public CityModel CityModel => _cityModel;

    private Renderer _renderer;
    private List<LineRenderer> _lineRenderers;
    private CityModel _cityModel;
    private const float _SPAWN_UNIT_DELAY = 20;
    private float _spawnDelay = _SPAWN_UNIT_DELAY;
    private bool _spawnEnabled = false;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public void Connect(CityModel cityModel)
    {
        _cityModel = cityModel;
        SetOwner(_cityModel.Owner);
    }

    public void SetOwner(byte owner)
    {
        _renderer.material.color = PlayerColor.Get(owner);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _spawnEnabled = !_spawnEnabled;
        }
        
        if (_spawnEnabled == false)
        {
            return;
        }
        
        _spawnDelay -= Time.deltaTime;

        if (_spawnDelay > 0)
        {
            return;
        }

        _spawnDelay = _SPAWN_UNIT_DELAY;
        var spawner = Instantiate(unitSpawner);
        spawner.transform.position = transform.position;
        spawner.owner = _cityModel.Owner;
    }
}