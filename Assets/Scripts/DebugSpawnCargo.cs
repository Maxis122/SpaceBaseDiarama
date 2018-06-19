using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSpawnCargo : MonoBehaviour {

    public GameObject cargoPrefab;
    public GameObject shipPrefab;
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Fire1"))
        {
            SpawnShip();
        }
	}

    void SpawnCargo()
    {
        //Get the nodes
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("LandingNode");

        //Choose a node
        int choice = Random.Range(0, nodes.Length - 1);
        GameObject ourNode = nodes[choice];

        //Create our cargo at an adjusted position
        Vector3 adjustedPosition = ourNode.transform.position;
        adjustedPosition.y += 1.0f;
        GameObject newCargo = Instantiate(cargoPrefab, adjustedPosition, Quaternion.identity);

        //Create a job.
        ourNode.GetComponent<Node>().CreateJob(newCargo);
    }

    void SpawnShip()
    {
        // Set a spawn position randomly in the air.
        Vector3 spawnPosition = new Vector3(Random.Range(-150.0f, 150.0f),
                                            Random.Range(100.0f, 150.0f),
                                            Random.Range(-150.0f, 150.0f));

        //Generate the new ship.
        Instantiate(shipPrefab, spawnPosition, Quaternion.identity);
    }
}
