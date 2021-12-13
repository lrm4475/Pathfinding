using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Vehicle : MonoBehaviour
{
    // Vectors necessary for force-based movement
    public Vector3 vehiclePosition;
    public Vector3 acceleration;
    public Vector3 direction;
    public Vector3 velocity;
    public Vector3 avgVelocity;
    public Vector3 centroid;

    //debug line materials
    public Material dirMat;
    public Material posMat;

    //for terrain
    Terrain activeTerrain;
    public Vector3 lowerBound;
    public Vector3 upperBound;

    //GameObjects & Scripts
    public SceneManager sceneManager;
    public Path pathScript;

    public List<GameObject> humans;
    public List<GameObject> zombies;
    public List<GameObject> obstacles;

    public GameObject psg;

    // Floats
    public float mass;
    public float maxSpeed;
    public float radius;
    float angle;

    //weights
    public float separationWeight;
    public float evadeWeight;
    public float pursueWeight;
    public float wanderWeight;


    //bools
    public bool collision;
    public bool pressed;

    // Start is called before the first frame update
    public void Start()
    {
        vehiclePosition = transform.position;

        activeTerrain = Terrain.activeTerrain;
        upperBound = activeTerrain.terrainData.bounds.max;
        lowerBound = activeTerrain.terrainData.bounds.min;

        sceneManager = GameObject.Find("GameManager").GetComponent<SceneManager>();
        pathScript = GameObject.Find("GameManager").GetComponent<Path>();

        humans = sceneManager.humans;
        zombies = sceneManager.zombies;
        psg = sceneManager.psg;

        radius = 1;
        angle = Random.Range(1, 360);

        collision = false;
        pressed = false;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        StayInBounds();
        CalcSteeringForces();
        ApplyForce(GetFriction(2.0f));

        // REMAIN SAME - NO CHANGES
        if (velocity.magnitude < 0.01f) { velocity = Vector3.zero; }
        velocity += acceleration * Time.deltaTime;
        velocity.y = 0f;
        vehiclePosition += velocity * Time.deltaTime;
        direction = velocity.normalized;
        acceleration = Vector3.zero;
        vehiclePosition.y = 0.5f;
        transform.position = vehiclePosition;
        transform.forward = direction;

        if (pressed)
        {
            //debug lines
            Debug.DrawLine(transform.position, transform.position + transform.forward * 3, Color.green); //draw forward vector
            Debug.DrawLine(transform.position, transform.position + transform.right, Color.blue); //draw right vector
        }

    }

    // ApplyForce
    // Receive an incoming force, divide by mass, and apply to the cumulative accel vector
    public void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    // ApplyGravityForce
    // Receive an incoming force, divide by mass, and apply to the cumulative accel vector
    public void ApplyGravityForce(Vector3 force)
    {
        acceleration += force;
    }

    //Friction Method
    public Vector3 GetFriction(float coeff)
    {
        Vector3 friction = velocity * -1.0f;
        friction.Normalize();
        friction = friction * coeff;
        return friction;
    }

    // Seek Method
    // All Vehicles have the knowledge of how to seek
    // They just may not be calling it all the time
    public Vector3 Seek(Vector3 targetPosition)
    {
        // Step 1: Find DV (desired velocity)
        // TargetPos - CurrentPos
        Vector3 desiredVelocity = targetPosition - vehiclePosition;

        // Step 2: Scale vel to max speed
        // desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);
        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;

        // Step 3:  Calculate seeking steering force
        Vector3 seekingForce = desiredVelocity - velocity;

        // Step 4: Return force
        return seekingForce;
    }

    /// Overloaded Seek
    /// </summary>
    /// <param name="target">GameObject of the target</param>
    /// <returns>Steering force calculated to seek the desired target</returns>
    public Vector3 Seek(GameObject target)
    {
        return Seek(target.transform.position);
    }

    //Pursue Method
    public Vector3 Pursue(Vehicle target)
    {
        Vector3 pursueVector = target.transform.position + target.velocity * 1.5f;
        return Seek(pursueVector);
    }

    //Flee Method
    public Vector3 Flee(Vector3 targetPos)
    {
        //distance vector
        Vector3 distance = vehiclePosition - targetPos;

        //calculate steering vector
        Vector3 targetVelocity = Vector3.Normalize(distance) * maxSpeed;
        Vector3 steeringVector = targetVelocity - velocity;

        return steeringVector;

    }

    //Overloaded Flee
    public Vector3 Flee(GameObject target)
    {
        return Flee(target.transform.position);
    }

    //Evade
    public Vector3 Evade(Vehicle target)
    {
        Vector3 evadeVector = target.transform.position + target.velocity * 1.5f;
        return Flee(evadeVector);
    }

    abstract public void CalcSteeringForces();

    //keep vehicles on terrain
    void StayInBounds()
    {
        float walls = 5.0f;
        Vector3 seekCenter = Vector3.zero;
        if (vehiclePosition.x < lowerBound.x + walls)
        {
            seekCenter = Seek(activeTerrain.terrainData.bounds.center);
        }
        if (vehiclePosition.x > upperBound.x - walls)
        {
            seekCenter = Seek(activeTerrain.terrainData.bounds.center);
        }
        if (vehiclePosition.z < lowerBound.z + walls)
        {
            seekCenter = Seek(activeTerrain.terrainData.bounds.center);
        }
        if (vehiclePosition.z > upperBound.z - walls)
        {
            seekCenter = Seek(activeTerrain.terrainData.bounds.center);
        }

        ApplyForce(seekCenter * 3f);
    }

    protected Vector3 Separation(List<GameObject> list)
    {
        Vector3 forces = Vector3.zero;

        foreach (GameObject body in list)
        {
            Vehicle bodyScript = body.GetComponent<Vehicle>();

            if (GetComponent<Vehicle>() != bodyScript)  //make sure it doesn't check against itself
            {
                float dist = (vehiclePosition - bodyScript.vehiclePosition).magnitude;
                if (dist <= 5)
                {
                    forces += Flee(body) * (1 / dist);
                }
            }
        }

        forces = forces.normalized * maxSpeed;
        Vector3 ultForce = forces - velocity;
        return ultForce;
    }

    protected Vector3 Alignment()
    {
        avgVelocity = Vector3.zero;
        Vector3 steeringVect = Vector3.zero;
        float flockDist = 50f; //ignore neighbors outside of a certain distance
        int count = 0;

        foreach (GameObject human in humans)
        {
            float dist = Vector3.Distance(vehiclePosition, human.transform.position);
            if ((dist > 0) && (dist < flockDist)) //check if its not the current human and if its within the boundary of the flock
            {
                avgVelocity += human.GetComponent<Vehicle>().velocity; //add velocities of every human
                count++; //for average calculation
            }
        }
        if (count > 0)
        {
            avgVelocity = (avgVelocity / count); //divide by the # of humans to get avg
            steeringVect = avgVelocity.normalized * maxSpeed;
            steeringVect = (steeringVect - velocity);

        }
        else
        {
            steeringVect = Vector3.zero;
            return steeringVect;
        }
        return steeringVect;

    }

    protected Vector3 Cohesion()
    {
        centroid = Vector3.zero;
        Vector3 steeringVect = Vector3.zero;
        float flockDist = 50f; //ignore neighbors outside of a certain distance
        int count = 0;

        foreach (GameObject human in humans)
        {
            float dist = Vector3.Distance(vehiclePosition, human.transform.position);
            if ((dist > 0) && (dist < flockDist)) //check if its not the current human and if its within the boundary of the flock
            {
                centroid += human.GetComponent<Vehicle>().vehiclePosition; //add positions of every human
                count++; //for average calculation
            }
        }
        if (count > 0)
        {
            centroid = (centroid / count); //divide by the # of humans to get avg
        }
        else
        {
            steeringVect = Vector3.zero;
            return steeringVect;
        }
        return Seek(centroid);
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(0, 50, 200, 25), "Press 'D' to show debug lines.");
    }
}
