using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapView : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private UnitSpawner spawner;
    [SerializeField] private List<CityView> cityViews;
    [SerializeField] private RoadView roadViewPrefab;
    
    public List<CityView> Cities => cityViews;
    
    private Func<char, (IEnumerable<char>, byte)> _shortestPath;
    private char _startVertex;
    private Dictionary<char, CityView> _cityViews;
    private Dictionary<EdgeModel, RoadView> _roadsViews;

    private Planner _planner1;
    private Planner _planner2;

    private void Start()
    {
        Map.Create();
        MapModel.UpdateGraph();
        
        _cityViews = new Dictionary<char, CityView>(cityViews.Count);
        _roadsViews = new Dictionary<EdgeModel, RoadView>(MapModel.Graph.EgeRoads.Count);

        foreach (var cityView in cityViews)
        {
            _cityViews.Add(cityView.Node, cityView);
        }

        // ToDo Remove connect each other
        foreach (var city in MapModel.Cities)
        {
            city.ConnectView(_cityViews[city.Index]);
            _cityViews[city.Index].Connect(city);
        }

        foreach (var pair in MapModel.Graph.EgeRoads)
        {
            CityView cityA = _cityViews[pair.Key.A.Index];
            CityView cityB = _cityViews[pair.Key.B.Index]; ;
            var roadView = Instantiate(roadViewPrefab, transform);
            roadView.name = pair.Key.A.Index + "<>" + pair.Key.B.Index;
            roadView.Connect(cityA, cityB, pair.Value);
            _roadsViews.Add(pair.Key, roadView);
        }

        _planner1 = new Planner(1);
        _planner2 = new Planner(2);
        StartCoroutine(WaitUnitsThenStart());

    }

    // ToDo remove this heck
    private IEnumerator WaitUnitsThenStart()
    {
        yield return new WaitForSeconds(0.5f);
        
        _planner1.CreateAttackEnemyCityPlan();
        _planner2.CreateAttackEnemyCityPlan();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
            var spawn = Instantiate(spawner);
            spawn.transform.position = worldPosition;
            spawn.owner = 1;
            Destroy(spawn.gameObject,1);
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 worldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
            var spawn = Instantiate(spawner);
            spawn.transform.position = worldPosition;
            spawn.owner = 2;
            Destroy(spawn.gameObject,1);
        }
        
        _planner1.ManualUpdate(Time.deltaTime);
        _planner2.ManualUpdate(Time.deltaTime);
    }
}