using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cubes;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System;
using System.Runtime.InteropServices;

namespace Regions {

    public class RegionUtility : MonoBehaviour {
        public static RegionUtility Instance;

        public Transform treePrefab;
        public float treeSpawnChance;
        public float riverSpawnChance;

        void Awake() {
            Instance = this;
        }

        public static Cube randomCube(Region region) {
            return region.cubes[UnityEngine.Random.Range(0, region.size), UnityEngine.Random.Range(0, region.size)];
        }

        public static void spawnTree(Cube cube) {
            cube.containedObject = Instantiate(Instance.treePrefab, CubeUtility.getPos(cube) + new Vector3(0f, 0.5f, 0f), Quaternion.identity).gameObject;
            cube.isWalkable = false;
        }

        public static void spawnRiver(Region region, Cube start, Cube end) {
            List<Vector3> riverPath = bresenhamPath(start, end);
            GameObject floorObject = GameObject.Find("/Floor/");

            foreach (Vector3 point in riverPath) {
                Cube cube = region.cubes[(int) point.z, (int) point.x];
                CubeUtility.destroyCube(cube);
                region.cubes[cube.zPos, cube.xPos] = CubeUtility.newCube("RiverCube", cube.xPos, cube.zPos, floorObject);
            }
        }

        private static List<Vector3> bresenhamPath(Cube start, Cube end) {
            int x = start.xPos;
            int z = start.zPos;
            int x2 = end.xPos;
            int z2 = end.zPos;

            int w = x2 - x;
            int h = z2 - z;
            int w_Abs = Mathf.Abs(w);
            int h_Abs = Mathf.Abs(h);

            int dx_v = 0, dz_v = 0; // Vertical movement
            int dx_h = 0, dz_h = 0; // Horizontal movement

            if (w > 0) { // end is right of start, so move forwards
                dx_v = 1;
                dx_h = 1;
            }
            else if (w < 0) { // end is left of start, so move backwards
                dx_v = -1;
                dx_h = -1;
            }

            if (h > 0) { // end is above start, so move upwards
                dz_v = 1;
            }
            else if (h < 0) { // end is below start, so move downwards
                dz_v = -1;
            }

            int longEdge = Mathf.Max(w_Abs, h_Abs);
            int shortEdge = Mathf.Min(w_Abs, h_Abs);

            if (longEdge == h_Abs) { // Change horizontal movement, as width < height
                dx_h = 0;

                if (h > 0) {
                    dz_h = 1;
                }
                else if (h < 0) {
                    dz_h = -1;
                }
            }

            List<Vector3> path = new List<Vector3>();
            path.Add(new Vector3(x, 0, z));

            int offset = longEdge >> 1; // Half longEdge rounded down
            int prev_dx = dx_h;
            int prev_dz = dz_h;

            for (int i = 1; i <= longEdge; i++) {
                offset += shortEdge;

                if (offset >= longEdge) { //Move vertically
                    offset -= longEdge;
                    x += dx_v;
                    z += dz_v;

                    path.Add(new Vector3(x - dx_v + dx_h, 0, z - dz_v + dz_h)); //Fill in gaps to have fully connected line
                }
                else { //Move horizontally
                    x += dx_h;
                    z += dz_h;
                }

                path.Add(new Vector3(x, 0, z));
            }

            return path;
        }
    }

    public class Region {
        public int size;
        public Cube[,] cubes;

        public Region(int size) {
            this.size = size;
        }

        public void Generate() {
            GameObject floorObject = GameObject.Find("/Floor/");
            cubes = new Cube[size, size];

            for (int z = 0; z < size; z++) {
                for (int x = 0; x < size; x++) {
                    cubes[z, x] = CubeUtility.newCube("GrassCube", x, z, floorObject, string.Format("{0}-{1}-{2}", x, 1, z));
                     
                    float treeRoll = UnityEngine.Random.Range(0.0f, 1.0f);
                    if (treeRoll <= RegionUtility.Instance.treeSpawnChance) {
                        RegionUtility.spawnTree(cubes[z, x]);
                    }
                }
            }

            float riverRoll = UnityEngine.Random.Range(0.0f, 1.0f);
            if (riverRoll <= RegionUtility.Instance.riverSpawnChance) {
                Cube start = RegionUtility.randomCube(this);
                Cube end = RegionUtility.randomCube(this);

                while (start.worldObject == end.worldObject) {
                    end = RegionUtility.randomCube(this);
                }

                RegionUtility.spawnRiver(this, start, end);
            }
        } 
    }
}

