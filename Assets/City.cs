using System.Collections.Generic;
using UnityEngine;

public class City
{
    public readonly int Rows = 10;
    public readonly int Cols = 10;

    public Node[] Intersections;

    MapCell[,] mapCells;

    public City(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;

        mapCells = new MapCell[rows, cols];
        CreateIntersections();
    }

    private void CreateIntersections()
    {
        int maxNodes = Rows / 2 * Cols / 2;
        int minNodes = (int)System.Math.Ceiling(maxNodes / 4.0f);
        int nodeCount = Random.Range(minNodes, maxNodes);
        Intersections = new Node[nodeCount];

        HashSet<MapPoint> mapPointsCreated = new HashSet<MapPoint>();

        for (var i = 0; i < Intersections.Length; i++)
        {
            MapPoint newPoint = new MapPoint()
            {
                Row = Random.Range(0, Rows - 1),
                Col = Random.Range(0, Cols - 1)
            };
            int maxIteretions = maxNodes + 1;
            
            while (mapPointsCreated.Contains(newPoint))
            {
                newPoint.Row += Random.Range(-1, 1) * 2;
                newPoint.Col += Random.Range(-1, 1) * 2;

                newPoint.Row = System.Math.Min(System.Math.Max(newPoint.Row, 0), Rows - 1);
                newPoint.Col = System.Math.Min(System.Math.Max(newPoint.Col, 0), Cols - 1);

                if (maxIteretions <= 0)
                {
                    throw new System.Exception("Could not find any possible position to place the node");
                }
                
                maxIteretions--;
            }

            mapPointsCreated.Add(newPoint);
            Intersections[i] = new Node()
            {
                Position = newPoint
            };
        }

        CreateRoudes();
    }

    private void CreateRoudes()
    {
        int maxEdgeCount = Intersections.Length - 1;

        Dictionary<MapPoint, HashSet<MapPoint>> edges = new Dictionary<MapPoint, HashSet<MapPoint>>();

        for (var i = 0; i < Intersections.Length; i++)
        {
            var node = Intersections[i];
            int edgeCount = Random.Range(1, maxEdgeCount);

            
        }
        // Intersections.Edges =
        // Roudes = 
    }

    public class Node
    {
        public MapPoint Position;
        public MapPoint[] ConnectedNodes;
    }

    public class MapCell
    {
        //public GameObject[] GameObjects;
        public MapPoint Position;
        public MapCellType MapCellType;
        public LineInCellPoint[] ConnectionPoints;
    }

    public struct MapPoint
    {
        public int Col;
        public int Row;

        public override bool Equals(object obj)
        {
            if (obj is MapPoint mapPoint)
            {
                return Col == mapPoint.Col && Row == mapPoint.Row;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Col * 31 + Row;
        }
    }

    public struct CellPoint
    {
        public float X;
        public float Y;
    }

    public struct LineInCellPoint
    {
        public CellPoint Point1;
        public CellPoint Point2;
    }

    public enum MapCellType
    {
        None,
        Free,
        Roude
    }
}
