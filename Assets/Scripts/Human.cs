using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Vehicle
{
    //Variables
    Vector3 ultimateSteeringForce;
    public Vector3 avgDir;
    float count;

    private void OnRenderObject()
    {
        if (pressed)
        {
            posMat.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Vertex(transform.position);
            GL.Vertex(centroid);
            GL.End();

            dirMat.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Vertex(centroid);
            GL.Vertex(centroid + (avgDir * 3));
            GL.End();
        }
    }

// Start is called before the first frame update
void Start()
    {
        mass = 1;
        maxSpeed = 10;
        base.Start();

        separationWeight = 1.5f;
    }

    // Update // Update is called once per frame
    public override void Update()
    {
        base.Update();
        CalcAvgDir();
        //draw fleeing vector
    }

    override public void CalcSteeringForces()
    {
        ultimateSteeringForce = Vector3.zero;
        float dist = Vector3.Distance(vehiclePosition, psg.transform.position);

        ultimateSteeringForce += Seek(psg) * 2f;
        if(dist < 5)
        {
            sceneManager.Teleport();
        }

        ultimateSteeringForce += Evade(zombies[0].GetComponent<Vehicle>()) * 1.2f;
        ultimateSteeringForce += Alignment() * 1f;
        ultimateSteeringForce += Cohesion() * 1f;
        ultimateSteeringForce += Separation(humans) * separationWeight;
        ultimateSteeringForce = Vector3.ClampMagnitude(ultimateSteeringForce, maxSpeed);
        ApplyForce(ultimateSteeringForce);
    }

    Vector3 CalcAvgDir()
    {
        avgDir = Vector3.zero;
        count = 0;
        foreach (GameObject human in humans)
        {
            avgDir += human.GetComponent<Vehicle>().direction; 
            count++; //for average calculation
        }
        if (count > 0)
        {
            avgDir = (avgDir / count).normalized;
            return avgDir;
        }
        else
        {
            return avgDir;
        }
    }

}
