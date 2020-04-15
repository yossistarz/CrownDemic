using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Map;

public class SetRandomPositionOnMap : MonoBehaviour
{
    public GenerateCity GenerateCity;

    // Start is called before the first frame update
    void Start()
    {
        var possibleNodePositioning = Random.Range(0, GenerateCity.Map.Intersections.Length);
        var node = GenerateCity.Map.Intersections[possibleNodePositioning];

        this.transform.position = GenerateCity.GetWorldPosition(node.Position, CellPoint.Center, transform.localScale);
    }
}
