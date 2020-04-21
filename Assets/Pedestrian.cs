using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Pedestrian : MonoBehaviour
{
    public float SpeedMagnitude = 1f;
    public Vector3 Direction;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 GetStartDirection()
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    return new Vector3(1, 0, 0);
                case 1:
                    return new Vector3(-1, 0, 0);
                case 2:
                    return new Vector3(0, 0, 1);
                case 3:
                    return new Vector3(0, 0, -1);
                default:
                    return new Vector3(1, 0, 0);
            }            
        }

        Direction = GetStartDirection();

    }

    // Update is called once per frame
    void Update()
    {

        transform.position += Direction * SpeedMagnitude * Time.deltaTime;

    }

    public Vector3 TurnRight()
    {
        Direction = Quaternion.AngleAxis(90, Vector3.up) * Direction;
        return Direction; 
    }


    public Vector3 TurnLeft()
    {
        Direction = Quaternion.AngleAxis(-90, Vector3.up) * Direction;
        return Direction;
    }
}
