using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Regions;
using Cubes;
using AStar;

public class Mover : MonoBehaviour {
    float nextMovementTime;
    float movementSpeed = 0.2f;

    public List<Cube> currentPath = new List<Cube>();
    public Cube currentCube;

    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    void Start() {
        nextMovementTime = Time.time;
    }

    void Update() {
        if (currentPath.Count == 0) { return; }

        if (AStarUtility.manhattan(currentCube, currentPath[0]) > 1f) {
            stopwatch.Start();

            List<Cube> path = AStarUtility.createPath(currentCube, currentPath[0], AStarUtility.Instance.animatePath).cubes;

            stopwatch.Stop();
            print("Path created in " + stopwatch.ElapsedMilliseconds + " ms");
            stopwatch.Reset();

            currentPath.RemoveAt(0);

            path.AddRange(currentPath);
            currentPath = path;
        }

        float currentTime = Time.time;
        if (currentTime > nextMovementTime) {
            float travelTime = CubeUtility.getSpeedModifier(currentPath[0]) * this.movementSpeed;
            move(currentPath[0]);
            currentPath.RemoveAt(0);

            nextMovementTime = Time.time + travelTime;
        }
    }

    public void move(Cube cube) {
        transform.position = CubeUtility.getGlobalPos(cube) + new Vector3(0f, 0.5f, 0f);
        currentCube = cube;
    }
}