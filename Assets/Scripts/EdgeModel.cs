using System;
using System.Linq;
using UnityEngine;

public readonly struct EdgeModel
{
    public CityModel A => _a;
    public CityModel B => _b;
    
    private readonly CityModel _a;
    private readonly CityModel _b;
    private readonly int _hash;

    public EdgeModel(CityModel a, CityModel b)
    {
        _a = a;
        _b = b;

        var aBytes = BitConverter.GetBytes(_a.Index);
        var bBytes = BitConverter.GetBytes(_b.Index);
        byte[] hashBytes = _a.Index < b.Index ? aBytes.Concat(bBytes).ToArray() : bBytes.Concat(aBytes).ToArray();
        _hash = BitConverter.ToInt32(hashBytes);
    }

    public override int GetHashCode()
    {
        return _hash;
    }

    public override string ToString()
    {
        return $"{_a.Index.ToString()}:{_b.Index.ToString()}";
    }

    public override bool Equals(object other)
    {
        if (other == null)
        {
            return false;
        }

        if (other is EdgeModel edgeModel)
        {
            return Equals(edgeModel);
        }

        return false;
    }

    private bool Equals(EdgeModel edgeModel)
    {
        return (_a == edgeModel._a && _b == edgeModel._b) || (_a == edgeModel._b && _b == edgeModel._a);
    }

    
}