using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    // Define our connector distance.
    const float distanceToNextNode = 10.0f;

    // Store our children for cargo passing.
    public List<GameObject> childNodes;
    public GameObject parentNode;

    // Cargo variables.
    int cargoCapacity = 20;
    List<GameObject> cargoStorage;

    // Reference to our drone
    public GameObject nodeDrone;

    // A variable to check if we are the master node
    public bool masterNode;
    int masterResources = 0;
    public GameObject[] masterDrones;
    int masterDroneQueue = 0;

    public void InitMasterNode(GameObject dronePrefab, int breadth)
    {
        // Init this node as a master node.
        masterNode = true;
        childNodes = new List<GameObject>();
        cargoStorage = new List<GameObject>();

        // Create our drones and link it them this node.
        masterDrones = new GameObject[breadth / 2];
        for (int i = 0; i < breadth / 2; i++)
        {
            Vector3 droneSpawnPosition = transform.position;
            masterDrones[i] = Instantiate(dronePrefab, droneSpawnPosition, Quaternion.identity);
            masterDrones[i].GetComponent<Drone>().InitDrone(this, breadth/2);
        }
        
    }

    public void InitLandingNode(GameObject nodeParent) {
        // Give the node it's parent.
        this.parentNode = nodeParent;
        cargoStorage = new List<GameObject>();


    }

    public void Spawn(int depth, int breadth, float height, GameObject nodeParent, GameObject mainBase)
    {
        // Setup our spawning angle.
        float angle = 0f;

        // Get our base for access to prefabs.
        ConstructBase baseComponenet = mainBase.GetComponent<ConstructBase>();

        // Give the node it's parent.
        this.parentNode = nodeParent;

        // If we are spawned, we cannot be the master.
        masterNode = false;

        // Init our lists
        childNodes = new List<GameObject>();
        cargoStorage = new List<GameObject>();

        for (int i = 0; i < breadth; i++)
        {
            // Create a random angle that is additive from our previous angle.
            // We use our breadth to evenly distribute them around our circle.
            angle += Random.Range(180.0f / breadth, 360.0f / breadth);

            // Choose a random length for our connectors.
            float distanceToNode = Random.Range(0.0f, baseComponenet.baseConnectorsVariance) + distanceToNextNode;

            // Find our target position from our angle.
            float xx = Mathf.Sin(angle) * distanceToNode;
            float zz = Mathf.Cos(angle) * distanceToNode;
            Vector3 targetPosition = new Vector3(xx + transform.position.x, height, zz + transform.position.z);

            // Check if we haven't already created a node in this position;
            if (Physics.OverlapBox(targetPosition, new Vector3(3f, 3f, 3f)).Length == 0)
            {
                //Check if there is another node or connector between us.
                Vector3 direction = (targetPosition - transform.position).normalized;
                if (Physics.Raycast(transform.position, direction, distanceToNode) == false)
                {
                    // Create our node.
                    if (depth == 0 || Random.Range(0, 10) > 7)
                    {
                        // Build a landing pad if we are at the end of our depth or if we randomly determine it.
                        GameObject newNode = Instantiate(baseComponenet.landingNode, targetPosition, Quaternion.identity);
                        newNode.transform.parent = nodeParent.transform;
                        newNode.GetComponent<Node>().InitLandingNode(gameObject);

                        // Add it to our children
                        childNodes.Add(newNode);
                    } else
                    {
                        // Create another node.
                        GameObject newNode = Instantiate(baseComponenet.node, targetPosition, Quaternion.identity);
                        
                        // Add it to our children
                        childNodes.Add(newNode);
                    }

                    // Create our connector by finding the midpoint and rotating.
                    Vector3 midPoint = (transform.position + targetPosition) / 2.0f;
                    GameObject newConnector = Instantiate(baseComponenet.connector, midPoint, Quaternion.identity);
                    Vector3 newScale = new Vector3(0.5f, 0.5f, distanceToNode);
                    newConnector.transform.localScale = newScale;
                    newConnector.transform.LookAt(targetPosition);
                    newConnector.transform.parent = nodeParent.transform;
                }

            }
        }

        // Semi-Breadth first spawning system.
        // Will create all nodes before spawning next set on THIS path.
        // Does not account for the current depth of other nodes in the system.
        for (int i = 0; i < childNodes.Count; i++)
        {
            // Check for landingPad
            if (childNodes[i].tag == "LandingNode")
                continue;

            // Call the recusive spawn function on our node.
            GameObject newNode = childNodes[i];
            newNode.GetComponent<Node>().Spawn(depth - 1, breadth, height, gameObject, mainBase);
            newNode.transform.parent = nodeParent.transform;
        }

        // Create our own drone and link it to this node.
        nodeDrone = Instantiate(baseComponenet.drone, transform.position, Quaternion.identity);
        nodeDrone.GetComponent<Drone>().InitDrone(this, childNodes.Count);
        nodeDrone.transform.parent = nodeParent.transform;
    }

    public bool IsSpaceInCargo()
    {
        if (cargoStorage.Count < cargoCapacity)
        {
            return true;
        }
        return false;
    }

    public void GetCargo(GameObject cargoToGet)
    {
        // Remove cargo from cargo hold.
        cargoStorage.Remove(cargoToGet);
        
    }

    public bool PutCargo(GameObject cargo)
    {
        // If we are the master, we have infinte space.
        if (masterNode)
        {
            masterResources++;
            Destroy(cargo);
            return true;
        }

        // If we are node, we check if we have space.
        if (IsSpaceInCargo() == true)
        {
            // If we do, we take on the cargo.
            cargoStorage.Add(cargo);

            // If we are not the master, we must create a job to have the cargo sent back.
            CreateJob(cargo);

            return true;
        }

        // Else we do not.
        return false;

    }

    public void CreateJob(GameObject cargo)
    {
        // Get reference to parent node.
        Node clientNode = parentNode.GetComponent<Node>();

        //-------------Master Override - If we are giving a job to the master node.
        if (clientNode.masterNode == true)
        {
            // Get the drone at the front of the queue and give it the job.
            clientNode.masterDrones[clientNode.masterDroneQueue].GetComponent<Drone>().jobs.Add(new Job(gameObject, cargo));
            clientNode.masterDroneQueue++;

            // Check if we have reached the end of the queue, and reset.
            if (clientNode.masterDroneQueue == clientNode.masterDrones.Length)
            {
                clientNode.masterDroneQueue = 0;
            }
        }
        //-------------Master Override End
        else
        //-------------Regular Node
        {
            // We get the Node component of our parentNode object.
            // We then get it's drone's Drone component and add a job.
            clientNode.nodeDrone.GetComponent<Drone>().jobs.Add(new Job(gameObject, cargo));
            cargoStorage.Add(cargo);
        }
        //-------------Regular Node End

    }
}
