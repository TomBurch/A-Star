using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using Cube;

namespace Terrain {

    public class GrassTerrain : MonoBehaviour { 

        public TerrainData Generate(int size)
        {
            TerrainData terrainData = new TerrainData(size);
            GameObject floorObject = GameObject.Find("/Floor/");

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    terrainData.terrainCubes[z, x] = new Cubes.GrassCube(z, x, floorObject, string.Format("{0}-{1}-{2}", x, 1, z));
                }
            }

            return terrainData;
        }

        public class TerrainData {
            public int size;
            public Cubes.TerrainCube[,] terrainCubes;

            public TerrainData(int size) {
                this.size = size;
                terrainCubes = new Cubes.TerrainCube[size, size];
            }
        }
    }
}

