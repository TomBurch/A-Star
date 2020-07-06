using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Regions;
using Cubes;
using System.Runtime.InteropServices;

public class Environment : MonoBehaviour {
    Region region;
    public Transform moverPrefab;
    public int chunkSize;

    public Region[,] regions;

    void Start() {
        UnityEngine.Random.InitState(67);

        AStar astar = (AStar) FindObjectOfType(typeof(AStar));
        region = new Region(chunkSize);
        region.Generate();

        Cube start = RegionUtility.randomCube(region);
        Cube end = RegionUtility.randomCube(region);

        while (!start.isWalkable) {
            start = RegionUtility.randomCube(region);
        }

        while (!end.isWalkable || start.worldObject == end.worldObject) {
            end = RegionUtility.randomCube(region);
        }

        GameObject moverObject = Instantiate(moverPrefab, CubeUtility.getPos(start) + new Vector3(0f, 0.5f, 0f), Quaternion.identity).gameObject;
        Mover moverScript = moverObject.GetComponent<Mover>();

        List<Cube> path = astar.createPath(region, start, end);
        if (path != null) {
            moverScript.currentPath = path;
        }
    }
}