using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Terrain;

public class Mover : MonoBehaviour
{
    Environment environment;
    float lastMovementTime;

    void Start()
    {
        environment = FindObjectOfType<Environment>();
        lastMovementTime = Time.time;
    }

    void Update()
    {
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
        Cubes.TerrainCube[,] terrainCubes = environment.getTerrainData().terrainCubes;
        transform.position = terrainCubes[(int)newGridPos.x, (int)newGridPos.z].getPos() + new Vector3(0f, 0.5f, 0f);

        return true;
    }
}