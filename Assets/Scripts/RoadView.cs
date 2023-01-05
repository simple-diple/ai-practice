using UnityEngine;

public class RoadView : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    private CityView _cityA;
    private CityView _cityB;
    
    public void Connect(CityView cityA, CityView cityB, Road road)
    {
        _cityA = cityA;
        _cityB = cityB;
        
        lineRenderer.SetPosition(0, cityA.transform.position);
        lineRenderer.SetPosition(1, cityB.transform.position);
        lineRenderer.material.color = PlayerColor.Get(road.Owner);
        gameObject.SetActive(true);
        
        cityA.CityModel.OnOwnerChanged += OnRoadOwnerChanged;
        cityB.CityModel.OnOwnerChanged += OnRoadOwnerChanged;
    }

    private void OnRoadOwnerChanged(byte newOwner)
    {
        if (_cityA.CityModel.Owner != _cityB.CityModel.Owner)
        {
            return;
        }
        
        lineRenderer.material.color = PlayerColor.Get(newOwner);
    }
}