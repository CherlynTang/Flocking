using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockAgent : MonoBehaviour
{
    public int AgentId = 0;
    [HideInInspector]
    public Collider AgentCollider;
    [Range(0.05f, 10f)] public float NeighborRadius = 1f;
    [Range(0.0f, 50.0f)] public float MaxVelocity = 2f;
    [Range(0.0f, 1f)] public float SeparationRadiusMultiplier = 0.5f;
    [Range(0.0f, 10.0f)] public float CohesionStrength = 1f;
    [Range(0.0f, 10.0f)] public float AlignmentStrength = 1f;
    [Range(0.0f, 10.0f)] public float SeparationStrength = 1f;
    [Range(0.0f, 10.0f)] public float StayInRadiusStrength = 1f;
    [Range(0.0f, 30.0f)] public float AvoidObstaclesStrength = 10f;
    [Range(0.0f, 30.0f)] public float MoveToTargetStrength = 0f;
    [Range(0.0f, 2f)] public float SmoothTime = 0.2f;
    [Range(1f, 50f)] public float Radius = 10f;
    [Range(1f, 10f)] public float ObstacleAvoidDist = 1.5f;
    [Range(0.1f, 10f)] public float TargetPathRadius = 0.5f;
    public Vector3 CenterPos = Vector3.zero;
    public Vector2 ConeHeight_Radius = new Vector2(2f, 1f);

    float separationRadius;
    Vector3 curCohesionVel = Vector3.zero;
    int numDirection = 80;
    float targetHeight = 0f;
    float targetRadius = 0f;

    public void Move(Vector3 velocity) {
        transform.position += velocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(Vector3.Normalize(velocity));

        FindNewDir();
    }
    public Vector3 CalculateMove(List<Transform> neighbors)
    {
        Vector3 newVelocity = Vector3.zero;
        if (IsNearObstacles())
        {
            newVelocity = AvoidObstacles() * AvoidObstaclesStrength;
        }
        else
        {
            newVelocity = Cohesion(neighbors) * CohesionStrength +
            Alignment(neighbors) * AlignmentStrength +
            Separation(neighbors) * SeparationStrength +
            StayInRadius() * StayInRadiusStrength +
            MoveToTarget() * MoveToTargetStrength;
        }
            
        if (Vector3.Magnitude(newVelocity) > MaxVelocity)
        {
            newVelocity = MaxVelocity * Vector3.Normalize(newVelocity);
        }
        newVelocity = Vector3.SmoothDamp(transform.forward, newVelocity, ref curCohesionVel, SmoothTime);
        return newVelocity;
    }
    Vector3 Cohesion(List<Transform> neighbors)
    {
        if (neighbors.Count == 0)
            return Vector3.zero;
        Vector3 CohesionPosition = Vector3.zero;
        Vector3 cohesionDir = Vector3.zero;
        int nCohesion = 0;
        foreach(Transform item in neighbors)
        {
            if(item.GetComponent<FlockAgent>().AgentId == AgentId)
            {
                CohesionPosition += item.position;
                nCohesion++;
            }    
        }
        if (nCohesion > 0)
        {
            CohesionPosition /= nCohesion;
            cohesionDir = CohesionPosition - transform.position;
        }      
        //cohesionDir = Vector3.SmoothDamp(transform.forward, cohesionDir, ref curCohesionVel, SmoothTime);
        return cohesionDir;
    }
    Vector3 Alignment(List<Transform> neighbors)
    {
        if (neighbors.Count == 0)
            return transform.forward;
        Vector3 alignmentDir = Vector3.zero;
        int nAlignment = 0;
        foreach(Transform item in neighbors)
        {
            if (item.GetComponent<FlockAgent>().AgentId == AgentId)
            {
                alignmentDir += item.forward;
                nAlignment++;
            }
        }
        if(nAlignment > 0)
        {
            alignmentDir /= nAlignment;
        }
        //Debug.DrawRay(transform.position, alignmentDir,Color.green);
        return alignmentDir;
    }
    Vector3 Separation(List<Transform> neighbors)
    {
        if (neighbors.Count == 0)
            return Vector3.zero;
        Vector3 separationDir = Vector3.zero;
        int nSeparation = 0;
        foreach( Transform item in neighbors)
        {
            float dist = Vector3.Distance(item.position, transform.position);
            if (dist < separationRadius)
            {
                nSeparation++;
                dist = Mathf.Max(0.05f, dist);
                separationDir += Vector3.Normalize(transform.position - item.position) / dist;

            }
        }
        if (nSeparation > 0)
        {
            separationDir /= nSeparation;
        }
        return separationDir;
    }

    Vector3 StayInRadius()
    {
        Vector3 centerOffset = CenterPos - transform.position;
        float t = centerOffset.magnitude / Radius;
        if(t< 0.9)
        {
            return Vector3.zero;
        }
        return centerOffset*t*t;
    }

    Vector3 AvoidObstacles()
    {

        Vector3[] rayDirections = FindNewDir();
        for (int i = 0; i < rayDirections.Length; i++)
        {
            RaycastHit hit;
            if (!Physics.SphereCast(transform.position, 0.5f, rayDirections[i], out hit, ObstacleAvoidDist, LayerMask.GetMask("Obstacle")) && !Physics.Raycast(transform.position, rayDirections[i], ObstacleAvoidDist, LayerMask.GetMask("Obstacle")))
            {
                //Debug.DrawRay(transform.position, rayDirections[i] * ObstacleAvoidDist, Color.red);
                return rayDirections[i];
            }
        }

        return Vector3.zero;
    }

    Vector3 MoveToTarget()
    {
        if(MoveToTargetStrength == 0f)
        {
            return Vector3.zero;
        }
        Vector3 targetDir = Vector3.zero;
        float curAngle = Mathf.Atan2(transform.position.z, transform.position.x);
        float targetAngle = curAngle - 0.3f;
        Vector3 targetPosition = new Vector3(targetRadius * Mathf.Cos(targetAngle), targetHeight, targetRadius * Mathf.Sin(targetAngle));
        if(Vector3.Distance(transform.position, targetPosition) > TargetPathRadius)
        {
            targetDir = Vector3.Normalize(targetPosition - transform.position);
        }        
        return targetDir;
    }
    // Start is called before the first frame update
    void Start()
    {
        AgentCollider = GetComponent<Collider>();
        separationRadius = NeighborRadius * SeparationRadiusMultiplier;
        if (MoveToTargetStrength > 0)
        {
            float rand = Mathf.Sqrt(Random.Range(0.02f, 1f));
            targetHeight = rand * ConeHeight_Radius.x;
            targetRadius = rand * ConeHeight_Radius.y;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Transform> GetNearbyObjects()
    {
        List<Transform> neighbors = new List<Transform>();
        Collider[] contextColliders = Physics.OverlapSphere(transform.position, NeighborRadius, LayerMask.GetMask("Fish"));
        foreach(Collider c in contextColliders)
        {
            if(c!= AgentCollider)
            {
                neighbors.Add(c.transform);
            }
        }
        return neighbors;
    }

    Vector3[] FindNewDir()
    {
        Vector3[] allDirections = new Vector3[numDirection];
        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;
        for(int i = 0; i < numDirection; i++)
        {
            float t = (float)i / numDirection;
            float phi = Mathf.Acos(1 - 2*t);
            float theta = angleIncrement * i;
            float x = Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = Mathf.Sin(phi) * Mathf.Sin(theta);
            float z = Mathf.Cos(phi);
            Vector3 dir = transform.TransformDirection(new Vector3(x, y, z));
            allDirections[i] = dir;
        }
        
        return allDirections;
    }
    bool IsNearObstacles()
    {
        RaycastHit hit;
        //Debug.DrawRay(transform.position, transform.forward * ObstacleAvoidDist, Color.green);
        if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out hit, ObstacleAvoidDist, LayerMask.GetMask("Obstacle")) || Physics.Raycast(transform.position, transform.forward, ObstacleAvoidDist, LayerMask.GetMask("Obstacle")))
        {
            return true;
        }
        return false;
    }
    
}
