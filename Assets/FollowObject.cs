using System;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public GameObject ObjectToFollow = null;
    public float Distance = 50f;
    public float StepFactor = 1f;
    public float Angle = 45f;

    Transform _followTransform = null;
    Transform _currentTransform = null;

    // Start is called before the first frame update
    void Start()
    {
        _followTransform = ObjectToFollow.GetComponent<Transform>();
        _currentTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        var desiredDistanceVertor = Quaternion.Euler(Angle, 0f, 0f) * new Vector3(0, 0, - Distance);
        desiredDistanceVertor = _followTransform.rotation * desiredDistanceVertor;

        var distanceVector = _currentTransform.position - _followTransform.position;

        Vector3 newPositionVector = distanceVector;

        if (distanceVector != desiredDistanceVertor)
        {
            var stepSize = (StepFactor / (float)Time.deltaTime);
            newPositionVector = newPositionVector + (desiredDistanceVertor - distanceVector) / stepSize;
        }
        else
        {
            newPositionVector = desiredDistanceVertor;
        }

        _currentTransform.position = _followTransform.position + newPositionVector ;
    }
}
