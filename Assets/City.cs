﻿using System.Collections.Generic;
using System.Reflection.Emit;
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
        FillMapCells();
    }

    private void CreateIntersections()
    {
        int maxNodes = Rows / 2 * Cols / 2;
        int minNodes = (int)System.Math.Ceiling(maxNodes / 4.0f);
        int nodeCount = Random.Range(minNodes, maxNodes);
        Intersections = new Node[nodeCount];

        CreateNodes();
        CreateConnectedNodes();

        void CreateNodes()
        {
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
        }

        void CreateConnectedNodes()
        {
            int maxEdgeCount = nodeCount - 1;

            Dictionary<int, HashSet<int>> graph = new Dictionary<int, HashSet<int>>();
            for (var i = 0; i < nodeCount; i++)
            {
                graph.Add(i, new HashSet<int>());
            }

            for (var i = 0; i < nodeCount; i++)
            {
                int edgeCount = Random.Range(1, maxEdgeCount);

                // Currently doing this in the naive way
                HashSet<int> selectedNodes = new HashSet<int>();
                for (var edgeIndex = graph[i].Count; edgeIndex < edgeCount; edgeIndex++)
                {
                    int nextNode;
                    do
                    {
                        nextNode = Random.Range(0, Intersections.Length - 1);
                    } while (selectedNodes.Contains(nextNode));

                    graph[i].Add(nextNode);
                    graph[nextNode].Add(i);

                    selectedNodes.Add(nextNode);
                }
            }

            for (var nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                var node = Intersections[nodeIndex];
                var nextNodeIndex = 0;
                node.ConnectedNodes = new Node[graph[nodeIndex].Count];
                foreach (var nextNode in graph[nodeIndex])
                {
                    node.ConnectedNodes[nextNodeIndex] = Intersections[nextNode];
                    nextNodeIndex++;
                }
            }
        }
    }

    private void FillMapCells()
    {
        FillIntersections();
        FillBlank();

        void FillIntersections()
        {
            for (var i = 0; i < Intersections.Length; i++)
            {
                var node = Intersections[i];
                var mapCell = new MapCell()
                {
                    MapCellType = MapCellType.Road,
                    ConnectionPoints = new List<PathInCell>()
                };

                SetMapCell(node.Position, mapCell);
                foreach(var connectedNode in node.ConnectedNodes)
                {
                    FillRoads(node, connectedNode);
                }
            }
        }

        void FillRoads(Node from, Node to)
        {
            var rowSteps = Random.Range(0, 1);
            var colSteps = 1 - rowSteps;

            var rowSign = System.Math.Sign(to.Position.Row - from.Position.Row);
            var colSign = System.Math.Sign(to.Position.Col - from.Position.Col);

            MapPoint position = from.Position;

            while (position != to.Position)
            {
                var cell = GetOrCreateCell(position);
                cell.MapCellType = MapCellType.Road;

                if (position.Col == to.Position.Col || 
                    position.Row == to.Position.Row)
                {
                    rowSteps = 1 - rowSteps;
                    colSteps = 1 - colSteps;
                }

                var nextPosition = new MapPoint(position.Row + rowSteps * rowSign, position.Col + colSteps * colSign);
                var nextCell = GetOrCreateCell(nextPosition);
                BuildPath(cell, nextCell);

                SetMapCell(position, cell);
                SetMapCell(nextPosition, nextCell);

                position = nextPosition;
            }

            void BuildPath(MapCell fromCell, MapCell toCell)
            {
                var fromPath = new PathInCell();
                var toPath = new PathInCell();

                fromPath.Points = new CellPoint[2];
                fromPath.Points[0] = new CellPoint(0.5f, 0.5f);
                fromPath.Points[1] = new CellPoint(0.5f + 0.5f * colSign, 0.5f + 0.5f * rowSign);
                if (PathExists(fromCell, fromPath) == false)
                {
                    fromCell.ConnectionPoints.Add(fromPath);
                }

                toPath.Points = new CellPoint[2];
                toPath.Points[0] = new CellPoint(0.5f, 0.5f);
                toPath.Points[1] = new CellPoint(0.5f - 0.5f * colSign, 0.5f - 0.5f * rowSign);
                if (PathExists(toCell, toPath) == false)
                {
                    toCell.ConnectionPoints.Add(toPath);
                }
            }

            bool PathExists(MapCell cell, PathInCell path)
            {
                for (var i =0; i < cell.ConnectionPoints.Count; i++)
                {
                    var existingPath = cell.ConnectionPoints[i];
                    if (path.Points.Length ==2 && existingPath.Points.Length == 2 && existingPath.Points[1] == path.Points[1])
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        void FillBlank()
        {

        }

        MapCell GetOrCreateCell(MapPoint point)
        {
            var cell = GetMapCell(point);

            if (cell == null)
            {
                cell = new MapCell()
                {
                    ConnectionPoints = new List<PathInCell>(),
                    MapCellType = MapCellType.None,
                    Position = point
                };
            }

            return cell;
        }
    }

    private void SetMapCell(MapPoint point, MapCell cell)
    {
        cell.Position = point;
        mapCells[point.Row, point.Col] = cell;
    }

    public MapCell GetMapCell(MapPoint point)
    {
        return mapCells[point.Row, point.Col];

    }

    private void SetMapCell(int row, int col, MapCell cell)
    {
        var point = new MapPoint()
        {
            Row = row,
            Col = col
        };

        SetMapCell(point, cell);
    }

    public MapCell GetMapCell(int row, int col)
    {
        var point = new MapPoint()
        {
            Row = row,
            Col = col
        };
        return GetMapCell(point);
    }

    public class Node
    {
        public MapPoint Position;
        public Node[] ConnectedNodes;
    }

    public class MapCell
    {
        //public GameObject[] GameObjects;
        public MapPoint Position;
        public MapCellType MapCellType;
        public List<PathInCell> ConnectionPoints;
    }

    public struct MapPoint
    {
        public int Row;
        public int Col;

        public MapPoint(MapPoint point) : this(point.Row, point.Col)
        {
        }

        public MapPoint(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public static bool operator !=(MapPoint left, MapPoint right)
        {
            return left.Equals(right) == false;
        }

        public static bool operator ==(MapPoint left, MapPoint right)
        {
            return left.Equals(right);
        }

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

        public CellPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static bool operator !=(CellPoint left, CellPoint right)
        {
            return left.Equals(right) == false;
        }

        public static bool operator ==(CellPoint left, CellPoint right)
        {
            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is CellPoint cellPoint)
            {
                return X == cellPoint.X && Y == cellPoint.Y;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() * 31 + Y.GetHashCode();
        }
    }

    public struct PathInCell
    {
        public CellPoint[] Points;
    }

    public enum MapCellType
    {
        None,
        Free,
        Road
    }
}
