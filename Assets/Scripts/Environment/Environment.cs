using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Terrain;

public class Environment : MonoBehaviour
{
    GrassTerrain.TerrainData terrainData;
    public Transform moverPrefab;

    void Start()
    {
        var grassTerrain = FindObjectOfType<GrassTerrain>();
        terrainData = grassTerrain.Generate(10);

        Vector3 spawnPos = terrainData.terrainCubes[0, 0].getPos() + new Vector3(0, 0.5f, 0);
        Instantiate(moverPrefab, spawnPos, Quaternion.identity);
    }

    public GrassTerrain.TerrainData getTerrainData()
    {
        return terrainData;
    }
}