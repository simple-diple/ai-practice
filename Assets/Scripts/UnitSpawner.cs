using System;
using System.Collections;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public byte owner;
    [SerializeField] private UnitView unitPrefab;

    private IEnumerator Start()
    {
        while (MapModel.Exists == false)
        {
            yield return null;
        }

        Spawn();
    }

    private void Spawn()
    {
        UnitView unitView = Instantiate(unitPrefab, transform);
        UnitModel unitModel = new UnitModel(owner,unitView);
        unitView.transform.SetParent(null);
        unitView.Connect(unitModel);
        MapModel.AddUnit(unitModel);
    }
}