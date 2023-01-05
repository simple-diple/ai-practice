using System.Collections.Generic;
using UnityEngine;

public static class PlayerColor
{
    private static readonly Dictionary<byte, Color> _colors;

    static PlayerColor()
    {
        _colors = new Dictionary<byte, Color>(2)
        {
            { 1, Color.red },
            { 2, Color.blue }
        };
    }

    public static Color Get(byte player)
    {
        return _colors[player];
    }
}