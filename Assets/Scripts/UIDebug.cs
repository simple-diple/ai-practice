using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TargetType
{
    Attack, CollectArmy, Protect
}

[Serializable]
public class TargetColor
{
    public TargetType type;
    public Color color;
}

public class UIDebug : MonoBehaviour
{
    [SerializeField] private Image target1;
    [SerializeField] private Image target2;
    [SerializeField] private Camera cam;
    [SerializeField] private TMP_Text letterPrefab;
    [SerializeField] private TMP_Text player1Text;
    [SerializeField] private TMP_Text player2Text;
    [SerializeField] private List<TargetColor> targetColors;

    private static UIDebug instance;
    private Dictionary<TargetType, Color> _targetColorsMap;

    private void Awake()
    {
        instance = this;
        _targetColorsMap = new Dictionary<TargetType, Color>(targetColors.Count);
        foreach (var target in targetColors)
        {
            _targetColorsMap.Add(target.type, target.color);
        }
    }

    private IEnumerator Start()
    {
        while (MapModel.Exists == false)
        {
            yield return null;
        }

        foreach (var city in MapModel.Cities)
        {
            var letter = Instantiate(letterPrefab, transform);
            Vector3 screenPosition = instance.cam.WorldToScreenPoint(city.Position);
            letter.transform.position = screenPosition;
            letter.text = city.Index.ToString();
        }
    }

    public static void SetTarget(CityModel city, TargetType targetType, byte player)
    {
        if (city == null)
        {
            return;
        }
        
        Image target = player == 1 ? instance.target1 : instance.target2;
        target.color = instance._targetColorsMap[targetType];
        Vector3 screenPosition = instance.cam.WorldToScreenPoint(city.Position);
        target.transform.position = screenPosition;
        
        TMP_Text tmpText = player == 1 ? instance.player1Text : instance.player2Text;
        tmpText.text = targetType + " city " + city.Index;
    }
    
     
}
