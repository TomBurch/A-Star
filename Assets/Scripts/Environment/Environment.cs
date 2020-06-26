using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Terrain;
using Cubes;
using System.Collections.Specialized;

public class Environment : MonoBehaviour
{
    Terrain.TerrainData terrainData;
    public Transform moverPrefab;

    public Material startMaterial;
    public Material endMaterial;

    public Vector3 startPos;
    public Vector3 endPos;

    void Start()
    {
        var grassTerrain = FindObjectOfType<GrassTerrain>();
        terrainData = grassTerrain.Generate(10);

        TerrainCube start = terrainData.terrainCubes[Random.Range(0, 10), Random.Range(0, 10)];
        TerrainCube end = terrainData.terrainCubes[Random.Range(0, 10), Random.Range(0, 10)];

        start.setMaterial(startMaterial);
        end.setMaterial(endMaterial);

        Instantiate(moverPrefab, start.getPos() + new Vector3(0f, 0.5f, 0f), Quaternion.identity);
    }

    public Terrain.TerrainData getTerrainData()
    {
        return terrainData;
    }
}