using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cubes;
using System.Security.Cryptography;
using System.Collections.Specialized;

namespace Terrain {

    public class GrassTerrain : MonoBehaviour {

        public Transform treePrefab;
        public float treeSpawnChance;

        public TerrainData Generate(int size) {
            TerrainData terrainData = new TerrainData(size);
            GameObject floorObject = GameObject.Find("/Floor/");

            for (int z = 0; z < size; z++) {
                for (int x = 0; x < size; x++) {
                    terrainData.terrainCubes[z, x] = new GrassCube(x, z, floorObject, string.Format("{0}-{1}-{2}", x, 1, z));
                     
                    float treeRoll = Random.Range(0.0f, 1.0f);
                    if (treeRoll <= treeSpawnChance) {
                        spawnTree(terrainData.terrainCubes[z, x]);
                    }
                }
            }

            return terrainData;
        }

        public void spawnTree(Cubes.TerrainCube cube) {
            cube.containedObject = Instantiate(treePrefab, cube.getPos() + new Vector3(0f, 0.5f, 0f), Quaternion.identity).gameObject;
            cube.isWalkable = false;
        }
    }

    public class TerrainData {
        public int size;
        public TerrainCube[,] terrainCubes;

        public TerrainData(int size) {
            this.size = size;
            terrainCubes = new TerrainCube[size, size];
        }
    }
}

