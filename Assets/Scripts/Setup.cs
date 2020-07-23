using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Worlds;
using Regions;
using Cubes;
using AStar;

public class Setup : MonoBehaviour {
    public Transform moverPrefab;
    public int worldSize;

    void Start() {
        UnityEngine.Random.InitState(67);

        GameObject worldContainer = new GameObject("World");
        World world = new World(worldSize, worldContainer);

        Cube start = WorldUtility.randomCube(world);
        Cube end = WorldUtility.randomCube(world);

        while (!start.isWalkable) {
            start = WorldUtility.randomCube(world);
        }

        while (!end.isWalkable || start.worldObject == end.worldObject) {
            end = WorldUtility.randomCube(world);
        }

        GameObject moverObject = Instantiate(moverPrefab, CubeUtility.getPos(start) + new Vector3(0f, 0.5f, 0f), Quaternion.identity).gameObject;
        Mover moverScript = moverObject.GetComponent<Mover>();
        moverScript.currentCube = start;

        List<Cube> path = world.graph.createAbstractPath(start, end);
        if (path != null) {
            moverScript.currentPath = path;
        }
    }
}
