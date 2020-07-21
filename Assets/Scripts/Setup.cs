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

        //Region region = WorldUtility.randomRegion(world);

        //Cube start = RegionUtility.randomCube(region);
        //Cube end = RegionUtility.randomCube(region);

        //while (!start.isWalkable) {
        //    start = RegionUtility.randomCube(region);
        //}

        //while (!end.isWalkable || start.worldObject == end.worldObject) {
        //    end = RegionUtility.randomCube(region);
        //}

        AbstractNode start = world.graph.nodes[0];
        AbstractNode end = world.graph.nodes[world.graph.nodes.Count - 1];

        GameObject moverObject = Instantiate(moverPrefab, CubeUtility.getPos(start.cube) + new Vector3(0f, 0.5f, 0f), Quaternion.identity).gameObject;
        Mover moverScript = moverObject.GetComponent<Mover>();
        moverScript.currentCube = start.cube;

        List<Cube> path = AStarUtility.createAbstractPath(world.graph, start, end);
        if (path != null) {
            moverScript.currentPath = path;
        }
    }
}
