using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCity : MonoBehaviour
{
    const float cellSize = 20f;
    private Map _map;

    public GameObject RoadTemplate;
    public GameObject IntersectionTemplate;
    public GameObject FreeTemplate;


    // Start is called before the first frame update
    void Start()
    {
        _map = new Map(20, 20);
        GameObject newCube;

        for (var row = 0; row < _map.Rows; row++)
        {
            for (var col = 0; col < _map.Cols; col++)
            {
                var cell = _map.GetMapCell(row, col);

                if (cell != null)
                {
                    if (cell.MapCellType == Map.MapCellType.Road)
                    {
                       
                        foreach (var path in cell.ConnectionPoints)
                        {
                            newCube = Instantiate<GameObject>(RoadTemplate);
                            float cubeSize = cellSize / 2f;
                            newCube.transform.localScale = new Vector3(cubeSize, 0.3f, cubeSize);
                            newCube.transform.parent = this.transform;
                            newCube.transform.position = new Vector3(
                                row * cellSize - cellSize / 2 + cubeSize * (path.Points[1].Y - 0.5f) , 
                                0, 
                                col * cellSize - cellSize / 2 + cubeSize * (path.Points[1].X - 0.5f));
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
                    newCube.transform.position = new Vector3(row * cellSize - cellSize / 2, 0, col * cellSize - cellSize / 2);

                }

            }
        }

        foreach(var intersect in _map.Intersections)
        {
            newCube = Instantiate<GameObject>(IntersectionTemplate);
            float cubeSize = cellSize / 4f;
            newCube.transform.localScale = new Vector3(cubeSize, 0.4f, cubeSize);
            newCube.transform.parent = this.transform;
            newCube.transform.position = new Vector3(
                intersect.Position.Row * cellSize - cellSize / 2 ,
                0,
                intersect.Position.Col * cellSize - cellSize / 2);
            newCube.SetActive(true);

            var cell = _map.GetMapCell(intersect.Position);
            foreach (var path in cell.ConnectionPoints)
            {
                newCube = Instantiate<GameObject>(IntersectionTemplate);
                cubeSize = cellSize / 1.8f;
                newCube.transform.localScale = new Vector3(cubeSize, 0.2f, cubeSize);
                newCube.transform.parent = this.transform;
                newCube.transform.position = new Vector3(
                    intersect.Position.Row * cellSize - cellSize / 2 + cubeSize * (path.Points[1].Y - 0.5f),
                    0,
                    intersect.Position.Col * cellSize - cellSize / 2 + cubeSize * (path.Points[1].X - 0.5f));
                newCube.SetActive(true);
            }
        }

        // var newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // Instantiate()
        // https://docs.unity3d.com/ScriptReference/Transform-parent.html?_ga=2.51063529.1903334404.1586621367-266296751.1584777261 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
