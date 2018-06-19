using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBehavior : MonoBehaviour {

    public enum ShipState { Seeking, Moving, Unloading };

    public GameObject cargoPrefab;

    GameObject landingPad;
    Vector3 targetLocation;
    Vector3 landingPadLocation;
    public ShipState state = ShipState.Seeking;
    int numberOfCargo;
    float timeToUnload = 3.0f;
    float unloadingTimer;

    public float speed = 9.0f;
    public float turningSpeed = 0.25f;
    public float landingDistance = 40.0f;
    bool leaving = false;

    int returnCode;
    AirTrafficControl controller;

    private void Start()
    {
        numberOfCargo = Random.Range(3, 10);
        unloadingTimer = Time.time;
    }

    private void Update()
    {
        RunShip();
    }

    void RunShip()
    {
        // Run the state machine - we do not handle seeking.
        switch(state)
        {
            case ShipState.Moving:
                Moving();
                break;

            case ShipState.Unloading:
                Unloading();
                break;

        }
    }

    public void Seeking(GameObject landingPadToArriveAt, int returnCode, AirTrafficControl controller)
    {
        //Save our return code.
        this.returnCode = returnCode;
        this.controller = controller;

        //Store our landing pad.
        landingPad = landingPadToArriveAt;

        // Get our landing pad location and store it in another variable as well for leaving.
        targetLocation = landingPad.transform.position;// + new Vector3(0.0f, 1.0f, 0.0f);
        landingPadLocation = targetLocation;

        // Go to that node.
        state = ShipState.Moving;
    }

    void Moving()
    {
        // Seek our pad. If we are within a certain range, we orient to land flat.
        float dist = (landingPadLocation - transform.position).sqrMagnitude;

        if (dist <= landingDistance)
        {
            // Get our step.
            float hoverSpeed = 1f + ((dist / landingDistance) * speed);
            float step = hoverSpeed * Time.deltaTime;
            float turingStep = turningSpeed * Time.deltaTime;

            // We hover and slow down.
            Vector3 directionOfFlight = Vector3.RotateTowards(transform.forward, Vector3.forward, turingStep, 0.0f);
            transform.rotation = Quaternion.LookRotation(directionOfFlight);

            // If we haven't arrived, we move towards the point.
            transform.position = Vector3.MoveTowards(transform.position, targetLocation, step);

        } else
        {
            // Get our step.
            float step = speed * Time.deltaTime;
            float turingStep = turningSpeed * Time.deltaTime;

            // We rotate to face our target.
            Vector3 direction = (targetLocation - transform.position).normalized;
            Vector3 directionOfFlight = Vector3.RotateTowards(transform.forward, direction, turingStep, 0.0f);
            transform.rotation = Quaternion.LookRotation(directionOfFlight);

            // Move forwards.
            transform.position += transform.forward * step;
        }

        // If we have arrived, we start unloading.
        if ((transform.position - targetLocation).sqrMagnitude <= 0.5f)
        {
            if (leaving == true)
            {
                Destroy(gameObject);
            } else
            {
                state = ShipState.Unloading;
            }
        }
    }

    void Unloading()
    {
        if (unloadingTimer <= Time.time)
        {
            // If there is space on the landing pad, place our cargo.
            if (landingPad.GetComponent<Node>().IsSpaceInCargo() == true)
            {
                // Reset the timer.
                unloadingTimer = Time.time + timeToUnload;

                // Create our cargo at an adjusted position
                Vector3 adjustedPosition = landingPad.transform.position;
                adjustedPosition.y += 1.0f;
                GameObject newCargo = Instantiate(cargoPrefab, adjustedPosition, Quaternion.identity);

                // Create a job.
                landingPad.GetComponent<Node>().CreateJob(newCargo);

                // Reduce the number of cargo on our ship.
                numberOfCargo += -1;

                // If we have fully unloaded, we leave.
                if (numberOfCargo <= 0)
                {
                    // Change our state
                    state = ShipState.Moving;

                    // Set a spawn position randomly in the air.
                    targetLocation = new Vector3(Random.Range(-150.0f, 150.0f),
                                                 Random.Range(100.0f, 150.0f),
                                                 Random.Range(-150.0f, 150.0f));

                    //Set ourselves to leaving.
                    leaving = true;

                    //Tell Air Traffic Control we are going and send them the code.
                    controller.Leaving(returnCode);
                }
            }
            
        }
    }

}
