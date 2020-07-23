using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cubes;
using Worlds;
using System.Collections.Specialized;
using System;

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
            return region.cubes[UnityEngine.Random.Range(0, WorldUtility.Instance.regionSize), UnityEngine.Random.Range(0, WorldUtility.Instance.regionSize)];
        }

        public static void spawnTree(Cube cube) {
            cube.containedObject = Instantiate(Instance.treePrefab, CubeUtility.getPos(cube) + new Vector3(0f, 0.5f, 0f), Quaternion.identity, cube.worldObject.transform).gameObject;
            cube.isWalkable = false;
        }

        public static void spawnRiver(Region region, Cube start, Cube end) {
            List<Vector3> riverPath = bresenhamPath(start, end);

            foreach (Vector3 point in riverPath) {
                Cube cube = region.cubes[(int) point.z, (int) point.x];
                CubeUtility.destroyCube(cube);
                region.cubes[cube.l_zPos, cube.l_xPos] = CubeUtility.newCube(region, "RiverCube", cube.l_xPos, cube.l_zPos, region.container, string.Format("{0}-{1}-{2}", cube.l_xPos, 1, cube.l_zPos));
            }
        }

        private static List<Vector3> bresenhamPath(Cube start, Cube end) {
            int x = start.l_xPos;
            int z = start.l_zPos;
            int x2 = end.l_xPos;
            int z2 = end.l_zPos;

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
        public int xPos, zPos;
        public World world;
        public GameObject container;
        public Cube[,] cubes;

        public Region(int xPos, int zPos, World world, GameObject container) {
            this.xPos = xPos;
            this.zPos = zPos;
            this.world = world;
            this.container = container;
            this.cubes = new Cube[WorldUtility.Instance.regionSize, WorldUtility.Instance.regionSize];

            Generate();
        }

        public void Generate() {
            for (int z = 0; z < WorldUtility.Instance.regionSize; z++) {
                for (int x = 0; x < WorldUtility.Instance.regionSize; x++) {
                    cubes[z, x] = CubeUtility.newCube(this, "GrassCube", x, z, container, string.Format("{0}-{1}-{2}", x, 1, z));
                     
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

