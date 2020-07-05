using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cubes;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System;
using System.Runtime.InteropServices;

namespace Terrains {

    public class Region : MonoBehaviour {

        public Transform treePrefab;
        public float treeSpawnChance;
        public float riverSpawnChance;

        public TerrainData Generate(int size) {
            TerrainData terrainData = new TerrainData(size);
            GameObject floorObject = GameObject.Find("/Floor/");

            for (int z = 0; z < size; z++) {
                for (int x = 0; x < size; x++) {
                    terrainData.cubes[z, x] = new GrassCube(x, z, floorObject, string.Format("{0}-{1}-{2}", x, 1, z));
                     
                    float treeRoll = UnityEngine.Random.Range(0.0f, 1.0f);
                    if (treeRoll <= treeSpawnChance) {
                        spawnTree(terrainData.cubes[z, x]);
                    }
                }
            }

            float riverRoll = UnityEngine.Random.Range(0.0f, 1.0f);
            if (riverRoll <= riverSpawnChance) {
                Cube start = terrainData.randomCube();
                Cube end = terrainData.randomCube();

                while (start.worldObject == end.worldObject) {
                    end = terrainData.randomCube();
                }

                spawnRiver(start, end, terrainData);
            }

            return terrainData;
        }

        public void spawnTree(Cube cube) {
            cube.containedObject = Instantiate(treePrefab, CubeUtility.getPos(cube) + new Vector3(0f, 0.5f, 0f), Quaternion.identity).gameObject;
            cube.isWalkable = false;
        }

        public void spawnRiver(Cube start, Cube end, TerrainData terrainData) {
            List<Vector3> riverPath = bresenhamPath(start, end);
            GameObject floorObject = GameObject.Find("/Floor/");

            foreach (Vector3 point in riverPath) {
                Cube cube = terrainData.cubes[(int) point.z, (int) point.x];
                CubeUtility.destroyCube(cube);
                terrainData.cubes[cube.zPos, cube.xPos] = new RiverCube(cube.xPos, cube.zPos, floorObject);
            }
        }

        private List<Vector3> bresenhamPath (Cube start, Cube end) {
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
            } else if (w < 0) { // end is left of start, so move backwards
                dx_v = -1;
                dx_h = -1;
            }

            if (h > 0) { // end is above start, so move upwards
                dz_v = 1;
            } else if (h < 0) { // end is below start, so move downwards
                dz_v = -1;
            }

            int longEdge = Mathf.Max(w_Abs, h_Abs);
            int shortEdge = Mathf.Min(w_Abs, h_Abs);

            if (longEdge == h_Abs) { // Change horizontal movement, as width < height
                dx_h = 0;

                if (h > 0) {
                    dz_h = 1;
                } else if (h < 0) {
                    dz_h = -1;
                }
            }

            //Vector3[] path = new Vector3[longEdge + 1];
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
                } else { //Move horizontally
                    x += dx_h;
                    z += dz_h;
                }

                path.Add(new Vector3(x, 0, z));
            }

            return path;
        }
    }

    public class TerrainData {
        public int size;
        public Cube[,] cubes;

        public TerrainData(int size) {
            this.size = size;
            cubes = new Cube[size, size];
        }

        public Cube randomCube() {
            return cubes[UnityEngine.Random.Range(0, size), UnityEngine.Random.Range(0, size)];
        }
    }
}

