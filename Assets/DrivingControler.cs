using System;
using UnityEngine;

public class DrivingControler : MonoBehaviour
{
    private readonly Vector3 _wheelRotationVector = new Vector3(0,1,0);

    const string _isDrivingParameterName = "Driving";
    const string _drivingState = "driving";
    const string _idleState = "idle";
    const string _loosingSpeedState = "loosingSpeed";

    public GameObject FrontWheelLeft;
    public GameObject FrontWheelRight;
    public GameObject AnimatedObject;

    public float MaxSpeed = 5f;
    public float MaxAcceleration = 0.5f;
    public float AccelerationPower = 0.05f;
    public float DeAcceleration = 0.01f;
    public float BreakDeAcceleration = 0.01f;
    public float WheelDistance = 0.8f;
    public float MaxWheelAngle = 15f;
    public float WheelRotationSpeed = 1f;

    private Animator _animator;
    private StateMachine _stateMachine;
    private Transform _transform;
    private Transform _leftWheelTransform;
    private Transform _rightWheelTransform;
    private float _wheelAngle = 0;
    private float _speed = 0f;
    private float _acceleration = 0f;

    public DrivingControler()
    {
        _stateMachine = new StateMachine();
        _stateMachine.AddState(_idleState, new ActionState() { StateAction = Idle });
        _stateMachine.AddState(_drivingState, new ActionState<float>() { StateAction = Driving });
        _stateMachine.AddState(_loosingSpeedState, new ActionState<float>() { StateAction = LoosingSpeed });

        _stateMachine.ChangeState(_idleState);
    }

    // Start is called before the first frame update
    void Start()
    {
        _animator = AnimatedObject.GetComponent<Animator>();
        _leftWheelTransform = FrontWheelLeft?.GetComponent<Transform>();
        _rightWheelTransform = FrontWheelRight?.GetComponent<Transform>();

        _transform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        _stateMachine.RunState();
        UpdateRotation();
        UpdateAsset();
    }

    private void UpdateRotation()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (Mathf.Abs(_wheelAngle) < MaxWheelAngle)
            {
                _wheelAngle += -WheelRotationSpeed;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (Mathf.Abs(_wheelAngle) < MaxWheelAngle)
            {
                _wheelAngle += WheelRotationSpeed;
            }
        }
        else
        {
            _wheelAngle = 0;
        }

        _leftWheelTransform.rotation = _transform.rotation * Quaternion.Euler(0f, _wheelAngle + 90f, 0f);
        _rightWheelTransform.rotation = _transform.rotation * Quaternion.Euler(0f, _wheelAngle + 90f, 0f);
    }

    private void UpdateAsset()
    {
        var localForward = _transform.forward;
        var backwardWheel = localForward * WheelDistance;
        var forwardWheel = localForward * WheelDistance;

        var backwardWheelPosition = _transform.position - backwardWheel;
        var forwardWheelPosition = _transform.position + forwardWheel;

        forwardWheelPosition += Quaternion.AngleAxis(_wheelAngle, _wheelRotationVector) * (localForward * WheelDistance * _speed * (float)Time.deltaTime);
        backwardWheelPosition += localForward * WheelDistance * _speed * (float)Time.deltaTime;

        var carAngle = Vector3.Angle(forwardWheelPosition - backwardWheelPosition, localForward);

        Debug.DrawRay(_transform.position, forwardWheel, Color.blue);
        Debug.DrawRay(_transform.position, backwardWheel, Color.green);
        Debug.DrawLine(backwardWheelPosition, forwardWheelPosition);
        if (_speed != 0)
        {
            _transform.Rotate(_wheelRotationVector, carAngle * Mathf.Sign(_wheelAngle) * Mathf.Sign(_speed));
            _transform.position += Quaternion.AngleAxis(carAngle, _wheelRotationVector) * (_transform.forward * _speed * (float)Time.deltaTime);
        }
    }

    private void Idle(ActionStateType action)
    {
        switch (action)
        {
            case ActionStateType.Start:
                {
                    _acceleration = 0;
                    _speed = 0;
                    _animator.SetBool(_isDrivingParameterName, false);
                    break;
                }
            case ActionStateType.Update:
                {
                    if (Input.GetKey(KeyCode.UpArrow))
                    {
                       _stateMachine.ChangeState<float>(_drivingState, 1);
                    }
                    else if (Input.GetKey(KeyCode.DownArrow))
                    {
                        _stateMachine.ChangeState<float>(_drivingState, -1);
                    }
                    break;
                }
        }
    }

    private void Driving(ActionStateType action, float directionMultiplayer)
    {
        switch (action)
        {
            case ActionStateType.Start:
                {
                    _animator.SetBool(_isDrivingParameterName, true);
                    break;
                }
            case ActionStateType.Update:
                {
                    if (Input.GetKey(KeyCode.UpArrow) == false && directionMultiplayer > 0 ||
                        Input.GetKey(KeyCode.DownArrow) == false && directionMultiplayer < 0)
                    {
                        _stateMachine.ChangeState(_loosingSpeedState, directionMultiplayer);
                    }

                    if (Math.Abs(_acceleration) < MaxAcceleration )
                    {
                        _acceleration += AccelerationPower * directionMultiplayer;
                    }
                    else
                    {
                        _acceleration = MaxAcceleration * directionMultiplayer;
                    }

                    if (Math.Abs(_speed) < MaxSpeed)
                    {
                        _speed += _acceleration;
                    }
                    else
                    {
                        _speed = MaxSpeed * directionMultiplayer;
                    }

                    break;
                }
        }
    }

    private void LoosingSpeed(ActionStateType action, float directionMultiplayer)
    {
        switch (action)
        {
            case ActionStateType.Start:
                {
                    _animator.SetBool(_isDrivingParameterName, true);
                    break;
                }
            case ActionStateType.Update:
                {
                    if (Input.GetKey(KeyCode.UpArrow) && directionMultiplayer > 0 ||
                        Input.GetKey(KeyCode.DownArrow) && directionMultiplayer < 0)
                    {
                        _stateMachine.ChangeState(_drivingState, directionMultiplayer);
                    }

                    if (Math.Abs(_acceleration) > 0)
                    {
                        _acceleration -= AccelerationPower * directionMultiplayer;
                    }
                    else
                    {
                        _acceleration = 0;
                    }

                    if (Input.GetKey(KeyCode.DownArrow) && directionMultiplayer > 0 ||
                        Input.GetKey(KeyCode.UpArrow) && directionMultiplayer < 0)
                    {
                        _speed -= BreakDeAcceleration * directionMultiplayer;
                    }

                    _speed -= DeAcceleration * directionMultiplayer;

                    if (_speed * directionMultiplayer <= 0f)
                    {
                        _speed = 0f;
                        _stateMachine.ChangeState(_idleState);
                    }

                    break;
                }
        }
    }
}
