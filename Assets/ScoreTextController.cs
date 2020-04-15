using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreTextController : MonoBehaviour
{
    private int _infectedCount;
    public Text ScoreText;
    public bool EmulateInfected;

    // Start is called before the first frame update
    void Start()
    {
        _infectedCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInfectedCount();
        ScoreText.text = $"Infected {_infectedCount}";

        // when people get infected, increase, when healed, decrease, for now just randomly increase
        void UpdateInfectedCount()
        {
            if (EmulateInfected && Random.Range(0, 10) > 8)
            {
                _infectedCount++;
            }

        }
    }
}
