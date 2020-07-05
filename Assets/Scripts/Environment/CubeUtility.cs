﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using UnityEngine;

namespace Cubes {

    public class CubeUtility : MonoBehaviour {
        public static CubeUtility Instance;
        public Transform grassPrefab;
        public Transform riverPrefab;

        void Awake() {
            Instance = this;
        }

        public static Dictionary<string, float> SpeedModifiers = new Dictionary<string, float>() {
            { "Cubes.GrassCube", 1.0f },
            { "Cubes.RiverCube", 2.0f }
        };

        public static Vector3 getPos(Cube cube) {
            return cube.worldObject.transform.position;
        }

        public static void setMaterial(Cube cube, Material newMaterial) {
            cube.worldObject.GetComponent<MeshRenderer>().material = newMaterial;
        }

        public static IEnumerator setMaterialAfterDelay(Cube cube, Material newMaterial, float delay) {
            yield return new WaitForSeconds(delay);
            cube.worldObject.GetComponent<MeshRenderer>().material = newMaterial;
        }

        public static void clearCube(Cube cube) {
            if (cube.containedObject != null)
            {
                Destroy(cube.containedObject);
                cube.containedObject = null;
                cube.isWalkable = true;
            }
        }

        public static void destroyCube(Cube cube) {
            clearCube(cube);
            Destroy(cube.worldObject);
        }
    }

    public class Cube : MonoBehaviour {
        public bool isWalkable;
        public GameObject containedObject;
        public GameObject worldObject;
        public int xPos, zPos;

        public Cube(int xPos, int zPos, bool isWalkable, Transform prefab, GameObject parent, string name) {
            this.containedObject = null;
            this.isWalkable = isWalkable;
            this.xPos = xPos;
            this.zPos = zPos;

            Transform newCube = Instantiate(prefab, new Vector3(xPos * 1f, 0f, zPos * 1f), Quaternion.identity, parent.transform);
            newCube.name = name;
            this.worldObject = newCube.gameObject;
        }
    }

    public class GrassCube : Cube {
        public GrassCube(int xPos, int zPos, GameObject parent, string name = "GrassCube") : base(xPos, zPos, true, CubeUtility.Instance.grassPrefab, parent, name) { }
    }

    public class RiverCube : Cube {
        public RiverCube(int xPos, int zPos, GameObject parent, string name = "RiverCube") : base(xPos, zPos, true, CubeUtility.Instance.riverPrefab, parent, name) { }
    }
}