using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirTrafficControl : MonoBehaviour {

    GameObject[] landingPads;
    Queue<GameObject> shipWaitingList;
    Queue<GameObject> landingPadQueue;
    Queue<int> landingPadQueueReturnCodes;
    float remainingSpaces;

    public GameObject shipPrefab;

    public float baseSpawningRate = 5.0f;
    public float varianceSpawningRate = 5.0f;
    float spawningTimer;

    public void InitController()
    {
        // Get an array of landing pads and create array of free landing pads.
        landingPads = GameObject.FindGameObjectsWithTag("LandingNode");
        landingPadQueue = new Queue<GameObject>();
        landingPadQueueReturnCodes = new Queue<int>();

        // Loop through the landing pads and add their data to the queues.
        for (int i = 0; i < landingPads.Length; i++) {
            landingPadQueue.Enqueue(landingPads[i]);
            landingPadQueueReturnCodes.Enqueue(i);
        }
            

        // Create our waiting list
        shipWaitingList = new Queue<GameObject>();

        // Set the timer
        spawningTimer = Time.time;

        // Remember the maximum amount of free spaces.
        remainingSpaces = landingPads.Length;
    }

    private void Update()
    {
        Spawning();
        HandleWaitingList();
    }

    void Spawning()
    {
        if (spawningTimer <= Time.time)
        {
            //Reset the spawning timer.
            spawningTimer = Time.time + baseSpawningRate + Random.Range(0.0f, varianceSpawningRate);

            // Set a spawn position randomly in the air.
            Vector3 spawnPosition = new Vector3(Random.Range(-150.0f, 150.0f),
                                                Random.Range(100.0f, 150.0f),
                                                Random.Range(-150.0f, 150.0f));

            //Generate the new ship.
            GameObject newShip = Instantiate(shipPrefab, spawnPosition, Quaternion.identity);
            shipWaitingList.Enqueue(newShip);
        }
    } 

    void HandleWaitingList()
    {
        // Check if there are any ships waiting and if there is any space.
        if (shipWaitingList.Count > 0 && remainingSpaces > 0)
        {
            // If there are, get the first free space.
            GameObject landing = landingPadQueue.Dequeue();
            int returnCode = landingPadQueueReturnCodes.Dequeue();

            // We get the ship at the front of this queue and remove it.
            GameObject priorityShip = shipWaitingList.Dequeue();

            // Seeking is called on the ship with the found landing pad.
            priorityShip.GetComponent<ShipBehavior>().Seeking(landing, returnCode, this);

            // Landing pad is no longer free.
            remainingSpaces--;

            
        }
    }

    public void Leaving(int returnCode)
    {
        // Add the free space back onto the queue.
        landingPadQueueReturnCodes.Enqueue(returnCode);
        landingPadQueue.Enqueue(landingPads[returnCode]);
        remainingSpaces++;
    }
}
