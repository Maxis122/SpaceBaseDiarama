using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructBase : MonoBehaviour {

    //Prefabs for base nodes
    [Header ("Node Prefabs:")]
    public GameObject node;
    public GameObject connector;
    public GameObject landingNode;
    public GameObject drone;

    //Base contruction variables
    [Header ("Base paramters:")]
    public int baseDepth = 1;
    public int baseNodeBreadth = 2;
    public float baseHeight = 3.0f;
    public float baseConnectorsVariance = 2.0f;

    //Define our connector's minimum distance.
    const float distanceToNextNode = 10.0f;

    //Our node variables.
    Node thisNode;

    private void Start()
    {
        //Spawn the nodes
        SpawnNodes();

        //Once all the nodes are added, init the Air Traffic Controller
        AirTrafficControl atc = GetComponent<AirTrafficControl>();
        atc.InitController();
    }

    void SpawnNodes()
    {
        //Setup our angle
        float angle = 0;

        //Get our node and init it as a master node.
        thisNode = GetComponent<Node>();
        thisNode.InitMasterNode(drone, baseNodeBreadth);

        for (int i = 0; i < baseNodeBreadth; i++)
        {
            // Create a random angle that is additive from our previous angle.
            // We use our breadth to evenly distribute them around our circle.
            angle += Random.Range(180.0f / baseNodeBreadth, 360.0f / baseNodeBreadth);

            // Choose a random length for our connectors.
            float distanceToNode = Random.Range(0.0f,
                                                baseConnectorsVariance) + distanceToNextNode;

            // Find our target position from our angle.
            float xx = Mathf.Sin(angle) * distanceToNode;
            float zz = Mathf.Cos(angle) * distanceToNode;
            Vector3 targetPosition = new Vector3(xx, baseHeight, zz);

            // Check if we haven't already created a node in this position;
            if (Physics.OverlapBox(targetPosition, new Vector3(3f, 3f, 3f)).Length == 0)
            {
                //Check if there is another node or connector between us.
                Vector3 direction = (targetPosition - transform.position).normalized;
                if (Physics.Raycast(transform.position, direction, distanceToNode) == false)
                {
                    // Create our node.
                    GameObject newNode = Instantiate(node, targetPosition, Quaternion.identity);

                    // Add it to our children
                    thisNode.childNodes.Add(newNode);

                    // Create our connector by finding the midpoint and rotating.
                    Vector3 midPoint = (transform.position + targetPosition) / 2.0f;
                    GameObject newConnector = Instantiate(connector, midPoint, Quaternion.identity);
                    Vector3 newScale = new Vector3(0.5f, 0.5f, distanceToNode);
                    newConnector.transform.localScale = newScale;
                    newConnector.transform.LookAt(targetPosition);
                    newConnector.transform.parent = transform;
                }
            }
        }

        // Breadth first spawning system.
        for (int i = 0; i < thisNode.childNodes.Count; i++)
        {
            // Get each child and run Spawn
            GameObject newNode = thisNode.childNodes[i];
            newNode.GetComponent<Node>().Spawn(baseDepth - 1, baseNodeBreadth, baseHeight, gameObject, gameObject);
            newNode.transform.parent = transform;

        }

        
    }

    private void Update()
    {
        HubBaseUpdate();
    }

    void HubBaseUpdate()
    {

    }
}
