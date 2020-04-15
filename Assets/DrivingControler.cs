using System;
using UnityEngine;

public class DrivingControler : MonoBehaviour
{
    private readonly Vector3 _wheelRotationVector = new Vector3(0,1,0);

    const string _isDrivingParameterName = "Driving";
    const string _drivingState = "driving";
    const string _idleState = "idle";
    const string _loosingSpeedState = "loosingSpeed";
    
    // From Unity?
    public Animator _animator;
    public StateMachine _stateMachine;
    public Transform _transform;
    public Transform _leftWheelTransform;
    public Transform _rightWheelTransform;
    public GameObject FrontWeelLeft;
    public GameObject FrontWeelRight;
    public GameObject AnimatedObject;

    public float _wheelAngle = 0;
    public float _speed = 0f;
    public float _acceleration = 0f;
    public float MaxSpeed = 5f;
    public float MaxAcceleration = 0.5f;
    public float AccelerationPower = 0.05f;
    public float DeAcceleration = 0.01f;
    public float BreakDeAcceleration = 0.01f;
    public float WeelDistance = 0.8f;
    public float MaxWeelAngle = 15f;
    public float WeelRotationSpeed = 1f;

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
        _leftWheelTransform = FrontWeelLeft?.GetComponent<Transform>();
        _rightWheelTransform = FrontWeelRight?.GetComponent<Transform>();

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
            if (Mathf.Abs(_wheelAngle) < MaxWeelAngle)
            {
                _wheelAngle += -WeelRotationSpeed;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (Mathf.Abs(_wheelAngle) < MaxWeelAngle)
            {
                _wheelAngle += WeelRotationSpeed;
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
        //_weelAngle = 30f;
        //_speed = 5;

        var localForward = _transform.forward;
        var backwardWeel = localForward * WeelDistance;
        var forwardWeel = localForward * WeelDistance;

        var backwardWeelPosition = _transform.position - backwardWeel;
        var forwardWeelPosition = _transform.position + forwardWeel;

        forwardWeelPosition += Quaternion.AngleAxis(_wheelAngle, _wheelRotationVector) * (localForward * WeelDistance * _speed * (float)Time.deltaTime);
        backwardWeelPosition += localForward * WeelDistance * _speed * (float)Time.deltaTime;

        var carAngle = Vector3.Angle(forwardWeelPosition - backwardWeelPosition, localForward);

        Debug.DrawRay(_transform.position, forwardWeel, Color.blue);
        Debug.DrawRay(_transform.position, backwardWeel, Color.green);
        Debug.DrawLine(backwardWeelPosition, forwardWeelPosition);
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
