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
        public Region[,] regions;

        public World(int size) {
            this.size = size;
        }
    }
}