using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTimeOfDay : MonoBehaviour {

    public float speedOfDay;
    float timeOfDay;
    public bool isDay;
    public bool debugTime;

    private void Start()
    {
        isDay = true;
        timeOfDay = 0.0f;
    }

    private void Update()
    {
        if (Input.GetButton("Fire1") && debugTime) {
            // Change the time of day.
            transform.Rotate(Vector3.right * Time.deltaTime * 100.0f);
            timeOfDay += Time.deltaTime * 100.0f;
        } else {
            // Change the time of day.
            transform.Rotate(Vector3.right * Time.deltaTime * speedOfDay);
            timeOfDay += Time.deltaTime * speedOfDay;
        }

        

        // Check for day.
        if (timeOfDay > 0.0f)
            isDay = true;
        
        
        // If it is night, we set the daytime to night.
        if (timeOfDay >= 180.0f)
            isDay = false;
        

        if (timeOfDay >= 360.0f)
            timeOfDay = 0.0f;
  
    }
}
