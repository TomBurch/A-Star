using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Worlds;
using Regions;
using Cubes;

public class Setup : MonoBehaviour {
    public Transform moverPrefab;
    public int worldSize;

    void Start() {
        UnityEngine.Random.InitState(67);

        GameObject worldContainer = new GameObject("World");
        World world = new World(worldSize, worldContainer);

        AStar astar = (AStar)FindObjectOfType(typeof(AStar));
        Region region = WorldUtility.randomRegion(world);

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
