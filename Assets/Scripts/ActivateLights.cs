using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateLights : MonoBehaviour {

    public Light light;
    public ChangeTimeOfDay tod;

    private void Start()
    {
        light = GetComponent<Light>();
        tod = FindObjectOfType<ChangeTimeOfDay>();
    }

    private void Update()
    {
        if (tod.isDay == light.enabled)
        {
            light.enabled = !tod.isDay;
        }
    }

}
