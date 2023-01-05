using System;
using System.Collections.Generic;

public class Algorithms
{
    private Dictionary<CityModel, CityModel> GetPrevious(Graph graph, CityModel start)
    {
        var previous = new Dictionary<CityModel, CityModel>();
        var queue = new Queue<CityModel>();

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var vertex = queue.Dequeue();
            foreach (var neighbor in graph.AdjacencyList[vertex])
            {
                if (previous.ContainsKey(neighbor))
                    continue;

                previous[neighbor] = vertex;
                queue.Enqueue(neighbor);
            }
        }

        return previous;
    }
    
    public Func<CityModel, (IEnumerable<CityModel> , byte)> ShortestPathFunction(Graph graph, CityModel start)
    {
        var previous = GetPrevious(graph, start);

        (IEnumerable<CityModel> , byte) ShortestPath(CityModel v)
        {
            var path = new List<CityModel> { };
            byte transfersCount = 0;
            var current = v;
            byte currentLine = graph.GetEgeIndex(new EdgeModel(current, previous[current]));

            while (!current.Equals(start))
            {
                path.Add(current);
                var prev = previous[current];
                byte previousLine = graph.GetEgeIndex(new EdgeModel(current, prev));

                current = previous[current];

                if (currentLine != previousLine)
                {
                    transfersCount++;
                }

                currentLine = previousLine;
            }

            path.Add(start);
            path.Reverse();

            return (path, transfersCount);
        }

        return ShortestPath;
    }

    public Func<CityModel, (CityModel, CityModel)> ShortestEnemyCity(Graph graph, CityModel start, byte enemy)
    {
        var previous = GetPrevious(graph, start);

        (CityModel, CityModel) ShortestPath(CityModel v)
        {
            var current = v;

            while (!current.Equals(start) && current.Owner != enemy)
            {
                current = previous[current];
            }

            return (MapModel.GetCity(current.Index), MapModel.GetCity(previous[current].Index));
        }

        return ShortestPath;
    }
}

