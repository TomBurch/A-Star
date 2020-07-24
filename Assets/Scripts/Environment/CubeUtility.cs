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

        ///<summary> 
        /// Float used to multiply the mover's movement time <br></br><br></br>
        /// (greater speedModifier = lesser speed) 
        ///</summary>
        static Dictionary<string, float> SpeedModifiers = new Dictionary<string, float>() {
            { "Cubes.GrassCube", 1.0f },
            { "Cubes.RiverCube", 2.0f }
        };

        public static float getSpeedModifier(Cube cube) {
            return SpeedModifiers[cube.GetType().ToString()];
        }

        public static Vector3 getGlobalPos(Cube cube) {
            return cube.worldObject.transform.position;
        }

        public static void setMaterial(Cube cube, Material newMaterial) {
            cube.worldObject.GetComponent<MeshRenderer>().material = newMaterial;
        }

        public static IEnumerator setMaterialAfterDelay(Cube cube, Material newMaterial, float delay) {
            yield return new WaitForSeconds(delay);
            cube.worldObject.GetComponent<MeshRenderer>().material = newMaterial;
        }

        /// <summary> 
        /// Remove the containedObject of a cube 
        /// </summary>
        public static void clearCube(Cube cube) {
            if (cube.containedObject != null) {
                Destroy(cube.containedObject);
                cube.containedObject = null;
                cube.isWalkable = true;
            }
        }

        public static void destroyCube(Cube cube) {
            Destroy(cube.worldObject);
            cube = null;
        }

        public static Cube newCube(Region region, string cubeType, int xPos, int zPos, GameObject parent, string name = "Cube") {
            Cube cube = null;

            switch (cubeType) {
                case "GrassCube": cube = new GrassCube(region, xPos, zPos, parent, name);
                    break;
                case "RiverCube": cube = new RiverCube(region, xPos, zPos, parent, name);
                    break;
            }

            return cube;
        }
    }

    public class Cube {
        public int xPos, zPos;
        public Region region;
        public bool isWalkable;
        public GameObject containedObject;
        public GameObject worldObject;

        public Cube(Region region, int xPos, int zPos, bool isWalkable, Transform prefab, GameObject parent, string name) {
            this.region = region;
            this.xPos = xPos;
            this.zPos = zPos;
            this.isWalkable = isWalkable;
            this.containedObject = null;

            int g_xPos = xPos + (WorldUtility.Instance.regionSize * region.xPos);
            int g_zPos = zPos + (WorldUtility.Instance.regionSize * region.zPos);

            this.worldObject = CubeUtility.Instantiate(prefab, new Vector3(g_xPos, 0f, g_zPos), Quaternion.identity, parent.transform).gameObject;
            worldObject.name = name;
        }
    }

    public class GrassCube : Cube {
        public GrassCube(Region region, int xPos, int zPos, GameObject parent, string name = "GrassCube") : base(region, xPos, zPos, true, CubeUtility.Instance.grassPrefab, parent, name) { }
    }

    public class RiverCube : Cube {
        public RiverCube(Region region, int xPos, int zPos, GameObject parent, string name = "RiverCube") : base(region, xPos, zPos, true, CubeUtility.Instance.riverPrefab, parent, name) { }
    }
}