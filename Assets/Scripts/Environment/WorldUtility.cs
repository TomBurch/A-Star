using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Regions;
using Cubes;
using AStar;

namespace Worlds {

    public class WorldUtility : MonoBehaviour {
        public static WorldUtility Instance;

        public int regionSize;

        void Awake() {
            Instance = this;
        }

        public static Cube getCube(World world, int x, int z) {
            int regionX = (int) (x / Instance.regionSize);
            int regionZ = (int) (z / Instance.regionSize);

            return world.regions[regionZ, regionX].cubes[z - (Instance.regionSize * regionZ), x - (Instance.regionSize * regionX)];
        }

        public static Region randomRegion(World world) {
            return world.regions[UnityEngine.Random.Range(0, world.size), UnityEngine.Random.Range(0, world.size)];
        }

        public static Cube randomCube(World world) {
            return RegionUtility.randomCube(randomRegion(world));
        }
    }

    public class World {
        public int size;
        public GameObject container;
        public Region[,] regions;
        public AbstractGraph graph;

        public World(int size) {
            this.size = size;
            this.container = new GameObject("World");

            this.regions = new Region[size, size];

            for (int z = 0; z < size; z++) {
                for (int x = 0; x < size; x++) {
                    GameObject regionContainer = new GameObject(string.Format("{0}-{1}-{2}", x, 1, z));
                    regionContainer.transform.parent = container.transform;

                    this.regions[z, x] = new Region(x, z, this, regionContainer);
                }
            }

            this.graph = new AbstractGraph(this);
        }
    }
}