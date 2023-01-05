using System;
using System.Collections.Generic;
using UnityEngine;

public class CityView : MonoBehaviour
{
    [SerializeField] private string nodeName;

    public char Node => nodeName[0];
    public byte Owner => _cityModel.Owner;
    public CityModel CityModel => _cityModel;

    private Renderer _renderer;
    private List<LineRenderer> _lineRenderers;
    private CityModel _cityModel;

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
        //_cityModel.CheckOwner();
    }
}