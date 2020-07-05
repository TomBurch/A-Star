using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Terrain;
using Cubes;

public class Mover : MonoBehaviour
{
    Environment environment;
    float nextMovementTime;
    float movementSpeed = 0.2f;

    public List<Cubes.TerrainCube> currentPath = new List<Cubes.TerrainCube>();

    void Start() {
        environment = FindObjectOfType<Environment>();
        nextMovementTime = Time.time;
    }

    void Update() {
        if (currentPath.Count == 0) { return; }

        float currentTime = Time.time;
        if (currentTime > nextMovementTime) {
            float travelTime = currentPath[0].speedModifier * movementSpeed;
            move(currentPath[0]);
            currentPath.RemoveAt(0);

            nextMovementTime = Time.time + travelTime;
        }
    }

    public void move(Cubes.TerrainCube cube) {
        transform.position = cube.getPos() + new Vector3(0f, 0.5f, 0f);
    }
}