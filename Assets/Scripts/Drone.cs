using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour {

    public enum State { Seeking, Traveling, Collecting, Depositing };

    public Node homeBase;
    public State state;

    float collectionTimer;
    float collectionSpeed = 1.0f;
    bool collecting = false;

    GameObject cargoInBay;
    Vector3 targetNode;

    public float droneSpeed = 7.0f;

    public List<Job> jobs;
    Job currentWork;

    public void InitDrone(Node homeBase, int breadth)
    {
        //Link drone to base and vice versa.
        this.homeBase = homeBase;
        homeBase.nodeDrone = gameObject;
        jobs = new List<Job>();
        droneSpeed += (float)breadth;
    }

    private void Update()
    {
        switch (state)
        {
            case State.Seeking:
                Seeking();
                break;

            case State.Traveling:
                Traveling();
                break;

            case State.Collecting:
                Collecting();
                break;
                
            case State.Depositing:
                Depositing();
                break;
        }
    }

    void Seeking()
    {
        //If there is a job...
        if (jobs.Count != 0)
        {
            // We get the first job.
            currentWork = jobs[0];
            jobs.RemoveAt(0);

            // Get the details of the job.
            targetNode = currentWork.location.transform.position;

            // Get the cargo.
            cargoInBay = currentWork.cargo;

            // Go to the node with the cargo for collection.
            state = State.Traveling;

        }
        
    }

    void Traveling()
    {
        if (transform.position != targetNode)
        {
            // If we haven't arrived, we move towards the point.
            float step = droneSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetNode, step);
        }
        else
        {
            // Check if we are collecting or depositing.
            if (collecting == false)
            {
                state = State.Collecting;
            } else
            {
                state = State.Depositing;
            }
        }
    }

    void Collecting()
    {
        // When we arrive, we start the timer.
        if (collecting == false)
        {
            //Begin collecting the object.
            collecting = true;
            collectionTimer = Time.time + collectionSpeed;

            // Get the cargo
            currentWork.location.GetComponent<Node>().GetCargo(cargoInBay);
        }

        // If we have collected the cargo, we go.
        if (collectionTimer <= Time.time)
        {
            //We choose a position and move.
            state = State.Traveling;
            targetNode = homeBase.transform.position;
            //targetNode.x += Random.Range(3.0f, 6.0f);
            //targetNode.y += 3.0f;
            //targetNode.z += Random.Range(3.0f, 6.0f);

            // The cargo is now parented to the drone to move it.
            cargoInBay.transform.parent = gameObject.transform;
        }
        
    }

    void Depositing()
    {
        // When we arrive, we use our collecting variable to check if this is the first call.
        if (collecting == true)
        {
            // Begin to deposit the object.
            collecting = false;
            collectionTimer = Time.time + collectionSpeed;

            //We unparent ourselves from the cargo.
            cargoInBay.transform.parent = null;
        }

        // Check the timer and then deposit.
        if (collectionTimer <= Time.time)
        {
            if (homeBase.PutCargo(cargoInBay))
            {
                // If there is space in our node, we deposit the cargo and reset the behavior.
                cargoInBay = null;
                state = State.Seeking;
            }
        }

    }

}

public struct Job
{
    public GameObject location;
    public GameObject cargo;

    public Job(GameObject location, GameObject cargo)
    {
        this.location = location;
        this.cargo = cargo;
    }
}
