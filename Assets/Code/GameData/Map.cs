using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using UnityEngine;

public class Map
{
    // This consts are experemental features.
    private const bool LimitOutboundEdge = true;
    private const int OutboundLimit = 3;
    private const bool UseNodeSpacing = true;

    public readonly int Rows;
    public readonly int Cols;

    public Node[] Intersections;

    MapCell[,] mapCells;

    public Map(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;

        mapCells = new MapCell[rows, cols];
        CreateIntersections();
        FillMapCells();
    }

    private void CreateIntersections()
    {
        int maxNodes = System.Math.Max(Rows, Cols) / 2;
        int minNodes = (int)System.Math.Ceiling(maxNodes / 3.0f);
        int nodeCount = Random.Range(minNodes, maxNodes + 1);
        int minNodeDistance = 0;

        if (UseNodeSpacing)
        {
            // This should allow us to space the nodes.
            // This calc returns the amount of nodes that will be empty 
            // (the squar is since the minNodeDistance is actually for both row and cols).
            minNodeDistance = (int)Mathf.Sqrt(maxNodes / nodeCount) + 1;
        }

        Intersections = new Node[nodeCount];

        CreateNodes();
        CreateConnectedNodes();

        void CreateNodes()
        {
            HashSet<MapPoint> mapPointsCreated = new HashSet<MapPoint>();

            for (var i = 0; i < nodeCount; i++)
            {
                MapPoint newPoint = new MapPoint()
                {
                    Row = Random.Range(0, Rows),
                    Col = Random.Range(0, Cols)
                };

                int maxIteretions = maxNodes * 2;
                while (IsPositionSPaced(newPoint) == false)
                {
                    // Trying next possible empty space.
                    newPoint.Row += Random.Range(-minNodeDistance, minNodeDistance + 1);
                    newPoint.Col += Random.Range(-minNodeDistance, minNodeDistance + 1);

                    // Preventing from row and col to be out of bounds.
                    newPoint.Row = System.Math.Min(System.Math.Max(newPoint.Row, 0), Rows - 1);
                    newPoint.Col = System.Math.Min(System.Math.Max(newPoint.Col, 0), Cols - 1);

                    if (maxIteretions <= 0)
                    {
                        throw new System.Exception("Could not find any possible position to place the node");
                    }

                    maxIteretions--;
                }

                SpacePosition(newPoint);

                Intersections[i] = new Node()
                {
                    Position = newPoint
                };
            }
            
            bool IsPositionSPaced(MapPoint point)
            {
                return mapPointsCreated.Contains(point) == false;
            }

            void SpacePosition(MapPoint point)
            {
                for (var r = -minNodeDistance; r <= minNodeDistance; r++)
                {
                    for (var c = -minNodeDistance; c <= minNodeDistance; c++)
                    {
                        mapPointsCreated.Add(new MapPoint(point.Row + r, point.Col + c));
                    }
                }
            }
        }

        void CreateConnectedNodes()
        {
            int maxEdgeCount = nodeCount;
            if (LimitOutboundEdge)
            {
                maxEdgeCount = OutboundLimit;
            }

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
                for (var edgeIndex = 0; edgeIndex < edgeCount; edgeIndex++)
                {
                    int nextNode;
                    do
                    {
                        nextNode = Random.Range(0, nodeCount);
                    } while (selectedNodes.Contains(nextNode) || nextNode == i);

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
        Dictionary<MapPoint, HashSet<MapPoint>> pathsCreated = new Dictionary<MapPoint, HashSet<MapPoint>>();

        FillIntersections();
        FillBlank();

        void FillIntersections()
        {
            for (var i = 0; i < Intersections.Length; i++)
            {
                var node = Intersections[i];
                var mapCell = GetOrCreateCell(node.Position);
                mapCell.MapCellType = MapCellType.Road;
                SetMapCell(node.Position, mapCell);

                foreach(var connectedNode in node.ConnectedNodes)
                {
                    FillRoads(node, connectedNode);
                }
            }
        }

        void FillRoads(Node from, Node to)
        {
            var rowSteps = Random.Range(0, 2);
            var colSteps = 1 - rowSteps;

            var rowSign = System.Math.Sign(to.Position.Row - from.Position.Row);
            var colSign = System.Math.Sign(to.Position.Col - from.Position.Col);

            MapPoint position = from.Position;

            if (pathsCreated.ContainsKey(from.Position) == false) 
            {
                pathsCreated.Add(from.Position, new HashSet<MapPoint>());
            }
            if (pathsCreated.ContainsKey(to.Position) == false)
            {
                pathsCreated.Add(to.Position, new HashSet<MapPoint>());
            }

            if (pathsCreated[to.Position].Contains(from.Position))
            {
                return;
            }

            pathsCreated[from.Position].Add(to.Position);

            while (position != to.Position)
            {
                var cell = GetOrCreateCell(position);
                cell.MapCellType = MapCellType.Road;

                if ((position.Col == to.Position.Col && colSteps != 0) || 
                    (position.Row == to.Position.Row && rowSteps != 0))
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
                fromPath.Points[1] = new CellPoint(0.5f + 0.5f * colSteps * colSign, 0.5f + 0.5f * rowSteps * rowSign);
                if (PathExists(fromCell, fromPath) == false)
                {
                    fromCell.ConnectionPoints.Add(fromPath);
                }

                toPath.Points = new CellPoint[2];
                toPath.Points[0] = new CellPoint(0.5f, 0.5f);
                toPath.Points[1] = new CellPoint(0.5f - 0.5f * colSteps * colSign, 0.5f - 0.5f * rowSteps * rowSign);
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
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Cols; col++)
                {
                    var cell = GetOrCreateCell(new MapPoint(row, col));
                    if (cell.MapCellType == MapCellType.None)
                    {
                        cell.MapCellType = MapCellType.Free;
                    }

                    SetMapCell(cell.Position, cell);
                }
            }
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
