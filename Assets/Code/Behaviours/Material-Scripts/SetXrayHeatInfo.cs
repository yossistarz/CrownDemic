using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetXrayHeatInfo : MonoBehaviour
{
    private void OnEnable()
    {
        SetShader();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetShader();
        //
    }

    void SetShader()
    {
        var material = GetComponent<Renderer>().material;
        material.SetInt("_HeatPointsCount", 6);
        material.SetVectorArray("_HeatPoints", new List<Vector4>()
        {
            new Vector4(1f, 0f, 0f, 1f),
            new Vector4(0f, 1f, 0f, 1f),
            new Vector4(0f, 0f, 1f, 1f),

            new Vector4(-1f, 0f, 0f, 1f),
            new Vector4(0f, -1f, 0f, 1f),
            new Vector4(0f, 0f, -1f, 1f)
        });

        float intensity = 1.8f;
        float size = 1.2f;
        material.SetFloatArray("_Intensitys", new List<float>()
        {
            intensity,
            intensity,
            intensity,

            intensity,
            intensity,
            intensity
        });

        material.SetFloatArray("_HeatSizes", new List<float>()
        {
            size,
            size,
            size,
            
            size,
            size,
            size
        });
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
