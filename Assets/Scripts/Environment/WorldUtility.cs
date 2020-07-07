using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Regions;
using Cubes;

namespace Worlds {

    public class WorldUtility : MonoBehaviour {
        public static WorldUtility Instance;

        public int regionSize;

        void Awake() {
            Instance = this;
        }
    }

    public class World {
        public int size;
        public GameObject container;
        public Region[,] regions;

        public World(int size, GameObject container) {
            this.size = size;
            this.container = container;

            regions = new Region[size, size];

            for (int z = 0; z < size; z++) {
                for (int x = 0; x < size; x++) {
                    GameObject regionContainer = new GameObject(string.Format("{0}-{1}-{2}", x, 1, z));
                    regionContainer.transform.parent = container.transform;

                    regions[z, x] = new Region(WorldUtility.Instance.regionSize, regionContainer);
                    regions[z, x].Generate();
                }
            }
        }
    }
}