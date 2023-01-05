using UnityEngine;

public static class Map
{
    public static MapModel Create()
    {
        var cities = new[]
        {
            new CityModel('A', 1), new CityModel('B', 1), new CityModel('C', 1), new CityModel('D', 1),
            new CityModel('E', 1), new CityModel('F', 2), new CityModel('G', 2), new CityModel('H', 2),
            new CityModel('J', 2), new CityModel('I', 2)
        };

        var roads = new[]
        {
            new Road(1, new[]
            {
                new CityModel('A', 1), new CityModel('B', 1), new CityModel('C', 1),
                new CityModel('E', 1), new CityModel('D', 1), new CityModel('B', 1)
            },1),
            
            new Road(2, new[]
            {
                new CityModel('J', 1), new CityModel('I', 2), new CityModel('G', 2),
                new CityModel('E', 2), new CityModel('F', 2), new CityModel('H', 2), new CityModel('G', 2)
            }, 2)
        };

        return new MapModel(cities, roads);
    }
}