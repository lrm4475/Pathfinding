using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public GameObject humanPrefab;
    public GameObject zombiePrefab;
    public GameObject psgPrefab;
    public List<GameObject> humans;
    public List<GameObject> zombies;
    public GameObject psg;

    Terrain activeTerrain;
    public Vector3 lowerBound;
    public Vector3 upperBound;
    float randX;
    float randZ;

    bool pressed;
    bool keyPressed;

    // Start is called before the first frame update
    void Start()
    {
        activeTerrain = Terrain.activeTerrain;
        upperBound = activeTerrain.terrainData.bounds.max;
        lowerBound = activeTerrain.terrainData.bounds.min;

        pressed = false;
        keyPressed = false;

        for (int i = 0; i <= 5; i++)
        {
            randX = Random.Range(lowerBound.x, upperBound.x);
            randZ = Random.Range(lowerBound.z, upperBound.z);
            GameObject human = Instantiate(humanPrefab, new Vector3(randX, 0, randZ), Quaternion.identity);
            humans.Add(human);
        }
        for (int i = 0; i < 1; i++)
        {
            randX = Random.Range(lowerBound.x, upperBound.x);
            randZ = Random.Range(lowerBound.z, upperBound.z);
            GameObject zombie = Instantiate(zombiePrefab, new Vector3(randX, 0, randZ), Quaternion.identity);
            zombies.Add(zombie);
        }

        randX = Random.Range(lowerBound.x, upperBound.x);
        randZ = Random.Range(lowerBound.z, upperBound.z);
        psg = Instantiate(psgPrefab, new Vector3(randX, 0, randZ), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            keyPressed = !keyPressed;
            pressed = !pressed;
            foreach(GameObject human in humans)
            {
                human.GetComponent<Human>().pressed = pressed;
            }
            foreach (GameObject zombie in zombies)
            {
                zombie.GetComponent<Zombie>().pressed = pressed;
            }
        }
    }

    public void Teleport()
    {
        randX = Random.Range(lowerBound.x +5, upperBound.x -5);
        randZ = Random.Range(lowerBound.z +5, upperBound.z -5);
        Vector3 newPos = new Vector3(randX, 0, randZ);
        psg.transform.position = newPos;
    }

}
