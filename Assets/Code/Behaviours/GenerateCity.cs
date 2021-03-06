﻿using System;
using System.Collections.Generic;
using UnityEngine;
using static Map;
using Random = UnityEngine.Random;

public class GenerateCity : MonoBehaviour
{
    const float cellSize = 10f;
    private const int Rows = 20;
    private const int Cols = 20;
    private Map _map;

    public Pedestrian pedestrian;
    public GameObject RoadTemplate;
    public GameObject IntersectionTemplate;
    public GameObject FreeTemplate;
    public Pedestrian prefabPedestrian;
    public float CreatePedestrainProbability = 0.7f;

    private List<Pedestrian> _pedestrians = new List<Pedestrian>();
    private Vector3 bottomLeftCornerPosition = new Vector3(-10, 0, -10);
    private float _lookAheadDistanceMagnitude = 0.3f;

    public Map Map { get => _map; }

    // Start is called before the first frame update
    void Start()
    {
        Vector3 GetNewPedestrianPosition(MapCell cell)
        {
            var pedestrianPositionVector = GetWorldPosition(cell.Position) - new Vector3(-5f, 0, -5f);
            pedestrianPositionVector.y = pedestrian.transform.position.y;
            return pedestrianPositionVector;
        }

        _map = new Map(Rows, Cols);
        GameObject newCube;

        for (var row = 0; row < _map.Rows; row++)
        {
            for (var col = 0; col < _map.Cols; col++)
            {
                var cell = _map.GetMapCell(row, col);

                if (cell != null)
                {
                    if (cell.MapCellType == MapCellType.Road)
                    {
                        if (Random.Range(0f, 1f) < CreatePedestrainProbability)
                        {
                            var newPedestrian = Instantiate(pedestrian, GetNewPedestrianPosition(cell), Quaternion.identity);
                            newPedestrian.SpeedMagnitude = Random.Range(0f, 3f);
                            _pedestrians.Add(newPedestrian);
                        }

                        foreach (var path in cell.ConnectionPoints)
                        {
                            newCube = Instantiate<GameObject>(RoadTemplate);
                            newCube.transform.parent = this.transform;
                            if (path.Points[1].Y == 0.5f)
                            {
                                newCube.transform.localScale = new Vector3(cellSize / 1.2f, 0.3f, cellSize / 2f);
                            }
                            else
                            {
                                newCube.transform.localScale = new Vector3(cellSize / 2f, 0.3f, cellSize / 1.2f);
                            }
                            newCube.transform.position = GetWorldPosition(cell.Position, path.Points[1], newCube.transform.localScale);

                            newCube.SetActive(true);
                        }

                    }

                    if (cell.MapCellType == Map.MapCellType.Free)
                    {
                        newCube = Instantiate<GameObject>(FreeTemplate);
                        newCube.SetActive(true);
                    }
                    else
                    {
                        newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    }

                    newCube.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
                    newCube.transform.parent = this.transform;
                    newCube.transform.position = GetWorldPosition(cell.Position);

                }
            }
        }

        foreach (var intersect in _map.Intersections)
        {
            newCube = Instantiate<GameObject>(IntersectionTemplate);
            float cubeSize = cellSize / 4f;
            newCube.transform.localScale = new Vector3(cubeSize, 0.4f, cubeSize);
            newCube.transform.parent = this.transform;
            newCube.transform.position = GetWorldPosition(intersect.Position);
            newCube.SetActive(true);

            var cell = _map.GetMapCell(intersect.Position);
            foreach (var path in cell.ConnectionPoints)
            {
                newCube = Instantiate<GameObject>(IntersectionTemplate);
                cubeSize = cellSize / 1.8f;
                newCube.transform.localScale = new Vector3(cubeSize, 0.2f, cubeSize);
                newCube.transform.parent = this.transform;
                newCube.transform.position = GetWorldPosition(cell.Position, path.Points[1], new Vector2(cubeSize, cubeSize));
                newCube.SetActive(true);
            }
        }

        // var newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // Instantiate()
        // https://docs.unity3d.com/ScriptReference/Transform-parent.html?_ga=2.51063529.1903334404.1586621367-266296751.1584777261 
    }

    public MapCell GetCellFromPosition(Vector3 position)
    {
        var relativePosition = position - bottomLeftCornerPosition;
        var row = Mathf.FloorToInt(relativePosition.x) / Mathf.RoundToInt(cellSize);
        var col = Mathf.FloorToInt(relativePosition.z) / Mathf.RoundToInt(cellSize);

        return _map.GetMapCell(row, col);
    }

    public bool IsPositionOutOfBounds(Vector3 position)
    {
        return (position.x < bottomLeftCornerPosition.x || position.x > bottomLeftCornerPosition.x + Rows * cellSize ||
                position.z < bottomLeftCornerPosition.z || position.z > bottomLeftCornerPosition.x + Cols * cellSize);
    }

    void Update()
    {
        foreach (var pedestrian in _pedestrians)
        {
            var positionOfLookingAhead = pedestrian.transform.position + pedestrian.Direction * _lookAheadDistanceMagnitude;
            var cellOfLookingForward = IsPositionOutOfBounds(positionOfLookingAhead) ? null : GetCellFromPosition(positionOfLookingAhead);

            if (cellOfLookingForward == null || cellOfLookingForward.MapCellType != MapCellType.Road)
            {
                pedestrian.TurnLeft();
            }
        }
    }

    public Vector3 GetWorldPosition(MapPoint point)
    {
        return GetWorldPosition(point, CellPoint.Center, new Vector2(cellSize, cellSize));
    }

    public Vector3 GetWorldPosition(MapPoint point, CellPoint relativeUVPositioning)
    {
        return GetWorldPosition(point, relativeUVPositioning, new Vector2(cellSize, cellSize));
    }

    public Vector3 GetWorldPosition(MapPoint point, CellPoint relativeUVPositioning, Vector3 objectSize2D)
    {
        return GetWorldPosition(point, relativeUVPositioning, new Vector2(objectSize2D.x, objectSize2D.z));
    }

    public Vector3 GetWorldPosition(MapPoint point, CellPoint relativeUVPositioning, Vector2 objectSize2D)
    {
        // Not sure abou the objectSize2D if it should be in the oposite direction or not.
        // In reality, the X is x while y is Z for the objectSize2D.
        return new Vector3(
            point.Row * cellSize - cellSize / 2 + objectSize2D.x * (relativeUVPositioning.Y - 0.5f),
            0,
            point.Col * cellSize - cellSize / 2 + objectSize2D.y * (relativeUVPositioning.X - 0.5f));
    }
}
