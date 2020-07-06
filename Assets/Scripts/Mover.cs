using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Regions;
using Cubes;

public class Mover : MonoBehaviour
{
    Environment environment;
    float nextMovementTime;
    float movementSpeed = 0.2f;

    public List<Cube> currentPath = new List<Cube>();

    void Start() {
        environment = FindObjectOfType<Environment>();
        nextMovementTime = Time.time;
    }

    void Update() {
        if (currentPath.Count == 0) { return; }

        float currentTime = Time.time;
        if (currentTime > nextMovementTime) {
            float travelTime = CubeUtility.SpeedModifiers[currentPath[0].GetType().ToString()] * movementSpeed;
            move(currentPath[0]);
            currentPath.RemoveAt(0);

            nextMovementTime = Time.time + travelTime;
        }
    }

    public void move(Cube cube) {
        transform.position = CubeUtility.getPos(cube) + new Vector3(0f, 0.5f, 0f);
    }
}