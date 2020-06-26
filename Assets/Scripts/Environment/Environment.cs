using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Terrain;

public class Environment : MonoBehaviour
{
    GrassTerrain.TerrainData terrainData;

    public Transform bunnyPrefab;
    List<Transform> population = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        var grassTerrain = FindObjectOfType<GrassTerrain>();
        terrainData = grassTerrain.Generate(10);
        initialPopulation(5);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public Transform spawnBeing(Vector3 position)
    {
        GameObject populationObject = GameObject.Find("/Population/");
        Transform newBeing = Instantiate(bunnyPrefab, position, Quaternion.identity, populationObject.transform);
        population.Add(newBeing);

        return newBeing;
    }

    public void initialPopulation(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPos = terrainData.terrainCubes[i, i].getPos() + new Vector3(0, 0.5f, 0);
            Transform newBeing = spawnBeing(spawnPos);    
        }
    }

    public GrassTerrain.TerrainData getTerrainData()
    {
        return terrainData;
    }
}