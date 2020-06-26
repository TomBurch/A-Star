using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Terrain;
//using Cube;

abstract public class Animal : MonoBehaviour
{
    //public Vector3 gridPos;
    int xPos;
    int yPos;

    public float maxHunger = 10;
    public float maxThirst = 10;
    float hunger = 0;
    float thirst = 0;

    float lastMovementTime;

    Environment environment;

    void Start()
    {
        environment = FindObjectOfType<Environment>();
        lastMovementTime = Time.time;
    }

    void Update()
    {       
        hunger += Time.deltaTime * (1 / maxHunger);
        //print(hunger);
        if (hunger >= 1 || thirst >= 1)
        {
            //die();
        }

        float currentTime = Time.time;
        if (currentTime - lastMovementTime >= 1)
        {
            Vector3 randomPos = new Vector3(Random.Range(0f, 10f), 0.5f, Random.Range(0f, 10f));
            move(randomPos);
            lastMovementTime = Time.time;
        }
    }

    public bool move(Vector3 newGridPos)
    {
        Cubes.TerrainCube[, ] terrainCubes = environment.getTerrainData().terrainCubes;
        transform.position = terrainCubes[(int) newGridPos.x, (int) newGridPos.z].getPos() + new Vector3(0f, 0.5f, 0f);

        return true;
    }

    public void die()
    {
        Destroy(gameObject);
    }
}
