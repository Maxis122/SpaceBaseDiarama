using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotate : MonoBehaviour {

    //Public parameters
    public Vector3 centerPoint;
    public float speed = 1.0f;
    public float cameraDistance = 50.0f;

    //Private calculation variables
    float cameraRotation = 0.0f;
	
	// Update is called once per frame
	void Update () {
        //Find the polar coordinates from the set distances.
        float cx, cy, cz;
        cx = centerPoint.x + (Mathf.Cos(cameraRotation) * cameraDistance);
        cz = centerPoint.z + (Mathf.Sin(cameraRotation) * cameraDistance);
        cy = transform.position.y;
        Vector3 newCameraPosition = new Vector3(cx, cy, cz);
        transform.position = newCameraPosition;

        //Rotate to look towards the center.
        transform.LookAt(centerPoint);

        //Rotate
        cameraRotation += speed;
        if (cameraRotation > 360.0f) cameraRotation = 0.0f;
        
    }
}
