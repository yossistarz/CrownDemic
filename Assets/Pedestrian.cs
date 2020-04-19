using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestrian : MonoBehaviour
{
    private float SPEED_MAGNITUDE = 1f;
    private Vector3 _direction;
    private bool _rightKeyAlreadyPressed = false;
    private bool _leftKeyAlreadyPressed = false;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 GetStartDirection()
        {
            return new Vector3(1, 0, 0);
        }

        _direction = GetStartDirection();

    }

    // Update is called once per frame
    void Update()
    {

        bool ShouldTurnLeft()
        {
            if (Input.GetKey("a"))
            {
                if (_leftKeyAlreadyPressed)
                {
                    return false;
                }
                else
                {
                    _leftKeyAlreadyPressed = true;
                    return true;
                }
            }
            else
            {
                _leftKeyAlreadyPressed = false;
                return false;
            }
        }

        bool ShouldTurnRight()
        {
            if (Input.GetKey("d"))
            {
                if (_rightKeyAlreadyPressed)
                {
                    return false;
                }
                else
                {
                    _rightKeyAlreadyPressed = true;
                    return true;
                }
            }
            else
            {
                _rightKeyAlreadyPressed = false;
                return false;
            }
        }


        if (ShouldTurnLeft())
        {
            _direction = GetDirectionAfterLeftTurn(_direction);
        }
        if (ShouldTurnRight())
        {
            _direction = GetDirectionAfterRightTurn(_direction);
        }


        transform.position += _direction * SPEED_MAGNITUDE * Time.deltaTime;

    }

    public static Vector3 GetDirectionAfterRightTurn(Vector3 direction)
    {
        return Quaternion.AngleAxis(90, Vector3.up) * direction;
    }

    private Vector3 GetDirectionAfterLeftTurn(Vector3 direction)
    {
        return Quaternion.AngleAxis(-90, Vector3.up) * direction;

    }
}
