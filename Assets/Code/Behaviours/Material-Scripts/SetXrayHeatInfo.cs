using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[ExecuteInEditMode]
public class SetXrayHeatInfo : MonoBehaviour
{
    public GameObject spotLight;

    private Material _material;

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
        _material = GetComponent<Renderer>().material;
        _material.SetInt("_HeatPointsCount", 6);
        _material.SetVectorArray("_HeatPoints", new List<Vector4>()
        {
            new Vector4(1f, 0f, 0f, 1f),
            new Vector4(0f, 1f, 0f, 1f),
            new Vector4(0f, 0f, 1f, 1f),

            new Vector4(-1f, 0f, 0f, 1f),
            new Vector4(0f, -1f, 0f, 1f),
            new Vector4(0f, 0f, -1f, 1f)
        });

        float intensity = 1.5f;
        float size = 1.3f;
        _material.SetFloatArray("_Intensitys", new List<float>()
        {
            intensity,
            intensity,
            intensity,

            intensity,
            intensity,
            intensity
        });

        _material.SetFloatArray("_HeatSizes", new List<float>()
        {
            size,
            size,
            size,
            
            size,
            size,
            size
        });
        SetLightInfo();
    }

    void SetLightInfo()
    {
        _material.SetVector("_LightPos", spotLight.transform.position);
        
        //var spotlightNormal = spotLight.orientation * (new Vector4(0, 0, 1, 1));
        _material.SetVector("_LightDirection", spotLight.transform.forward);
    }

    //Update is called once per frame
    void Update()
    {
        SetLightInfo();
    }
}
