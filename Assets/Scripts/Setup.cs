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
    new public Camera camera;

    Mover mover;
    World world;
    
    void Start() {
        UnityEngine.Random.InitState(67);

        world = new World(worldSize);
        Cube start = WorldUtility.randomCube(world);

        while (!start.isWalkable) {
            start = WorldUtility.randomCube(world);
        }

        GameObject moverObject = Instantiate(moverPrefab, CubeUtility.getGlobalPos(start) + new Vector3(0f, 0.5f, 0f), Quaternion.identity).gameObject;
        mover = moverObject.GetComponent<Mover>();
        mover.currentCube = start;
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                GameObject clickedObject = hit.transform.gameObject;
                CubePath path = world.graph.createAbstractPath(mover.currentCube, WorldUtility.getCube(world, (int) clickedObject.transform.position.x, (int) clickedObject.transform.position.z));
                
                if (path != null) {
                    mover.currentPath = path.cubes;
                }
            }
        }
    }
}