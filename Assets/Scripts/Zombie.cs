using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;  //prevents error when modifying list to delete humans

public class Zombie : Vehicle
{
    int wpNum;
    public List<GameObject> waypoints;

    private void OnRenderObject()
    {
        if (pressed)
        {
            dirMat.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Vertex(transform.position);
            GL.Vertex(transform.position + velocity * 1.5f);
            GL.End();

            for (int i = 0; i < waypoints.Count; i++)
            {
                posMat.SetPass(0);
                GL.Begin(GL.LINES);
                if(i == 0)
                {
                    GL.Vertex(waypoints[0].transform.position);
                    GL.Vertex(waypoints[i+1].transform.position);
                }
                else
                {
                    GL.Vertex(waypoints[i - 1].transform.position);
                    GL.Vertex(waypoints[i].transform.position);
                }
                GL.Vertex(waypoints[5].transform.position);
                GL.Vertex(waypoints[0].transform.position);
                GL.End();
            }
        }

        separationWeight = 2f;        
    }

    // Start is called before the first frame update
    void Start()
    {
        mass = 5;
        maxSpeed = 7;
        wpNum = 0;
        base.Start();
        waypoints = pathScript.waypoints;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        vehiclePosition.y = 1f;
    }

    override public void CalcSteeringForces()
    {
        Vector3 ultimateSteeringForce = Vector3.zero;
        float dist = Vector3.Distance(vehiclePosition, waypoints[wpNum].transform.position);

        if(dist < 5)
        {
            wpNum += 1;
            if(wpNum > 5)
            {
                wpNum = 0;
            }
        }

        ultimateSteeringForce += Seek(waypoints[wpNum]) * 2f;
        foreach (GameObject human in humans)
        {
            float fleeDist = Vector3.Distance(vehiclePosition, human.transform.position);
            if (fleeDist < 5)
            {
                ultimateSteeringForce += Evade(human.GetComponent<Vehicle>()) * 2f;
            }
        }
        ultimateSteeringForce = Vector3.ClampMagnitude(ultimateSteeringForce, maxSpeed);
        ApplyForce(ultimateSteeringForce);
    }
}
