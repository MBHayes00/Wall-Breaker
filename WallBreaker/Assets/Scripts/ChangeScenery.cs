using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeScenery : MonoBehaviour
{
    public Color fogColor;
    public Light light;
    public Color lightColor;

    // Start is called before the first frame update
    void Start()
    {
        RenderSettings.fogColor = fogColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        RenderSettings.fogColor = fogColor;
        light.color = lightColor;
    }
}
