using System;
using System.Collections.Generic;

public class Graph
{
    
    public Dictionary<CityModel, HashSet<CityModel>> AdjacencyList { get; } = new Dictionary<CityModel, HashSet<CityModel>>();
    public Dictionary<EdgeModel, Road> EgeRoads{ get; }

    public Graph()
    {
        EgeRoads = new Dictionary<EdgeModel, Road>();
        
        foreach (var vertex in MapModel.Cities)
            AddVertex(vertex);

        foreach (Road road in MapModel.Roads)
        {
            for (var i = 0; i < road.Cities.Length - 1; i++)
            {
                var station = road.Cities[i];
                var nextStation = road.Cities[i + 1];
                AddEdge(new EdgeModel(station, nextStation), road);
            }
        }
    }

    public byte GetEgeIndex(EdgeModel edge)
    {
        if (EgeRoads.ContainsKey(edge))
        {
            return EgeRoads[edge].Index;
        }

        throw new Exception($"EdgeModel {edge} not found!");
    }


    private void AddVertex(CityModel vertex)
    {
        AdjacencyList[vertex] = new HashSet<CityModel>();
    }

    private void AddEdge(EdgeModel edge, Road road)
    {
        if (AdjacencyList.ContainsKey(edge.A) && AdjacencyList.ContainsKey(edge.B))
        {
            AdjacencyList[edge.A].Add(edge.B);
            AdjacencyList[edge.B].Add(edge.A);
            EgeRoads.Add(edge, road);
        }
    }
    
    
}

