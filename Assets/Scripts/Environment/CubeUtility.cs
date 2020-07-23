using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using UnityEngine;

using Regions;
using Worlds;

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
            if (cube.containedObject != null) {
                Destroy(cube.containedObject);
                cube.containedObject = null;
                cube.isWalkable = true;
            }
        }

        public static void destroyCube(Cube cube) {
            clearCube(cube);
            Destroy(cube.worldObject);
        }

        public static Cube newCube(Region region, string cubeType, int l_xPos, int l_zPos, GameObject parent, string name = "Cube") {
            Cube cube = null;
            int g_xPos = l_xPos + (WorldUtility.Instance.regionSize * region.xPos);
            int g_zPos = l_zPos + (WorldUtility.Instance.regionSize * region.zPos);

            switch (cubeType) {
                case "GrassCube": cube = new GrassCube(g_xPos, g_zPos, region, l_xPos, l_zPos, parent, name);
                    break;
                case "RiverCube": cube = new RiverCube(g_xPos, g_zPos, region, l_xPos, l_zPos, parent, name);
                    break;
            }
        
            Transform cubeObject = Instantiate(cube.prefab, new Vector3(g_xPos, 0f, g_zPos), Quaternion.identity, parent.transform);
            cubeObject.name = name;
            cube.worldObject = cubeObject.gameObject;

            return cube;
        }
    }

    public class Cube {
        public int g_xPos, g_zPos;
        public int l_xPos, l_zPos;
        public Region region;
        public bool isWalkable;
        public Transform prefab;
        public GameObject containedObject;
        public GameObject worldObject;

        public Cube(int g_xPos, int g_zPos, Region region, int l_xPos, int l_zPos, bool isWalkable, Transform prefab, GameObject parent, string name) {
            this.g_xPos = g_xPos;
            this.g_zPos = g_zPos;
            this.region = region;
            this.l_xPos = l_xPos;
            this.l_zPos = l_zPos;
            this.isWalkable = isWalkable;
            this.prefab = prefab;
            this.containedObject = null;
        }
    }

    public class GrassCube : Cube {
        public GrassCube(int g_xPos, int g_zPos, Region region, int l_xPos, int l_zPos, GameObject parent, string name = "GrassCube") : base(g_xPos, g_zPos, region, l_xPos, l_zPos, true, CubeUtility.Instance.grassPrefab, parent, name) { }
    }

    public class RiverCube : Cube {
        public RiverCube(int g_xPos, int g_zPos, Region region, int l_xPos, int l_zPos, GameObject parent, string name = "RiverCube") : base(g_xPos, g_zPos, region, l_xPos, l_zPos, true, CubeUtility.Instance.riverPrefab, parent, name) { }
    }
}