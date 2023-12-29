
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public FlockAgent[] agentPrefabs;
    [Range(1, 500)]
    public int[] numberOfSpawns;
    [Range(1f, 10f)]
    public float spawnSphereRadius = 2f;
    [Range(1f, 10f)]
    public float driveFactor = 1f;
    List<FlockAgent> agentList = new List<FlockAgent>();
    // Start is called before the first frame update
    void Start()
    {
        for(int j = 0; j < numberOfSpawns.Length; j++)
        {
            for (int i = 0; i < numberOfSpawns[j]; i++)
            {
                FlockAgent newAgent = Instantiate(agentPrefabs[j], Random.insideUnitSphere * spawnSphereRadius, Random.rotation, transform);
                agentList.Add(newAgent);
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (FlockAgent agent in agentList) {
            List<Transform> context = agent.GetNearbyObjects();
            Vector3 agentMove = agent.CalculateMove(context);

            agent.Move(agentMove * driveFactor);
        }
    }
    /*private void OnDrawGizmos()
    {
        foreach(FlockAgent agent in agentList)
        {
            Handles.Label(agent.transform.position, agent.GetNearbyObjects().Count.ToString());
            //Gizmos.DrawWireSphere(agent.transform.position, agent.NeighborRadius);
        }
    }*/
}
