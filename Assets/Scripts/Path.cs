using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    //variables
    public List<GameObject> waypoints;
   public Vector3 wpPosition; 

    // Start is called before the first frame update
    void Start()
    {
        waypoints.Add(GameObject.Find("WP0"));
        waypoints.Add(GameObject.Find("WP1"));
        waypoints.Add(GameObject.Find("WP2"));
        waypoints.Add(GameObject.Find("WP3"));
        waypoints.Add(GameObject.Find("WP4"));
        waypoints.Add(GameObject.Find("WP5"));
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject wp in waypoints)
        {
            wpPosition = wp.transform.position;
        }
    }
}
