using UnityEngine;
using Random = UnityEngine.Random;

public class Pedestrian : MonoBehaviour
{
    public float SpeedMagnitude = 1f;
    public Vector3 Direction;
    public GameObject healthIndicator;
    public bool IsHealthy, IsSick;
    public float _maxDistanceForPlayerToAffectPedestrian = 10;

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
        IsHealthy = true;
        IsSick = false;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
        UpdateHealth();

        void UpdatePosition()
        {
            transform.position += Direction * SpeedMagnitude * Time.deltaTime;
        }

        void UpdateHealth()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

            if (distanceToPlayer < _maxDistanceForPlayerToAffectPedestrian)
            {
                MakeHealthy();
            }
            else
            {
                MakeSick();
            }
        }
    }

    public void MakeSick()
    {
        IsSick = true;
        IsHealthy = false;
        healthIndicator.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
    }

    public void MakeHealthy()
    {
        IsSick = false;
        IsHealthy = true;
        healthIndicator.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
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
