using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Regions;
using Cubes;

namespace Worlds {

    public class WorldUtility : MonoBehaviour {
        public static WorldUtility Instance;

        public Material portalMaterial;
        public Material entranceMaterial;
        public int regionSize;

        void Awake() {
            Instance = this;
        }

        public static Region randomRegion(World world) {
            return world.regions[UnityEngine.Random.Range(0, world.size), UnityEngine.Random.Range(0, world.size)];
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

                    regions[z, x] = new Region(WorldUtility.Instance.regionSize, x, z, regionContainer);
                }
            }

            new RegionAStar().createAbstractGraph(this);
        }
    }

    ////////////////////

    public class RegionAStar {
        class RegionNode {
            public Region region;

            public RegionNode(Region region) {
                this.region = region;
            }
        }

        class Portal {
            public Cube entrance;
            public Cube exit;

            public Portal(Cube entrance, Cube exit) {
                this.entrance = entrance;
                this.exit = exit;
            }
        }

        public void createAbstractGraph(World world) {
            RegionNode[,] grid = new RegionNode[world.size, world.size];

            for (int z = 0; z < world.size; z++) {
                for (int x = 0; x < world.size; x++) {
                    grid[z, x] = new RegionNode(world.regions[z, x]);
                }
            }

            //getPortals(grid, grid[0, 0].region);
            foreach (RegionNode node in grid) {
                getPortals(grid, node.region);
            }
        }

        List<Portal> getPortals(RegionNode[,] grid, Region region) {
            List<Portal> portals = new List<Portal>();

            checkLeftColumn(grid, region, portals);
            checkBottomRow(grid, region, portals);
            checkRightColumn(grid, region, portals);
            checkTopRow(grid, region, portals);

            foreach (Portal portal in portals) {
                CubeUtility.setMaterial(portal.entrance, WorldUtility.Instance.portalMaterial);
            }

            return portals;
        }
        
        void checkRightColumn(RegionNode[,] grid, Region region, List<Portal> portals) {
            if (!((region.xPos + 1) <= grid.GetLength(1) - 1)) { return; }

            RegionNode neighbour = grid[region.zPos, region.xPos + 1];
            List<Cube> adjacentCubes = new List<Cube>();

            for (int z = 0; z < region.size; z++) {
                Cube regionCube = region.cubes[z, region.size - 1];
                Cube neighbourCube = neighbour.region.cubes[z, 0];

                if (regionCube.isWalkable && neighbourCube.isWalkable) {
                    adjacentCubes.Add(regionCube);
                    CubeUtility.setMaterial(regionCube, WorldUtility.Instance.entranceMaterial);

                    if (z != region.size - 1) {
                        continue;
                    }
                }

                if (adjacentCubes.Count > 0) {
                    regionCube = adjacentCubes[adjacentCubes.Count >> 1];
                    neighbourCube = neighbour.region.cubes[regionCube.zPos, 0];

                    portals.Add(new Portal(regionCube, neighbourCube));
                    adjacentCubes = new List<Cube>();
                }
            }
        }

        void checkTopRow(RegionNode[,] grid, Region region, List<Portal> portals) {
            if (!((region.zPos + 1) <= grid.GetLength(0) - 1)) { return; }

            RegionNode neighbour = grid[region.zPos + 1, region.xPos];
            List<Cube> adjacentCubes = new List<Cube>();

            for (int x = 0; x < region.size; x++) {
                Cube regionCube = region.cubes[region.size - 1, x];
                Cube neighbourCube = neighbour.region.cubes[0, x];

                if (regionCube.isWalkable && neighbourCube.isWalkable) {
                    adjacentCubes.Add(regionCube);
                    CubeUtility.setMaterial(regionCube, WorldUtility.Instance.entranceMaterial);

                    if (x != region.size - 1) {
                        continue;
                    }
                }

                if (adjacentCubes.Count > 0) {
                    regionCube = adjacentCubes[adjacentCubes.Count >> 1];
                    neighbourCube = neighbour.region.cubes[0, regionCube.xPos];

                    portals.Add(new Portal(regionCube, neighbourCube));
                    adjacentCubes = new List<Cube>();
                }
            }
        }
    
        void checkBottomRow(RegionNode[,] grid, Region region, List<Portal> portals) {
            if (!((region.zPos - 1) >= 0)) { return; }

            RegionNode neighbour = grid[region.zPos - 1, region.xPos];
            List<Cube> adjacentCubes = new List<Cube>();

            for (int x = 0; x < region.size; x++) {
                Cube regionCube = region.cubes[0, x];
                Cube neighbourCube = neighbour.region.cubes[region.size - 1, x];

                if (regionCube.isWalkable && neighbourCube.isWalkable) {
                    adjacentCubes.Add(regionCube);
                    CubeUtility.setMaterial(regionCube, WorldUtility.Instance.entranceMaterial);

                    if (x != region.size - 1) {
                        continue;
                    }
                }

                if (adjacentCubes.Count > 0) {
                    regionCube = adjacentCubes[adjacentCubes.Count >> 1];
                    neighbourCube = neighbour.region.cubes[region.size - 1, regionCube.xPos];

                    portals.Add(new Portal(regionCube, neighbourCube));
                    adjacentCubes = new List<Cube>();
                }
            }
        }

        void checkLeftColumn(RegionNode[,] grid, Region region, List<Portal> portals) {
            if (!((region.xPos - 1) >= 0)) { return; }

            RegionNode neighbour = grid[region.zPos, region.xPos - 1];
            List<Cube> adjacentCubes = new List<Cube>();

            for (int z = 0; z < region.size; z++) {
                Cube regionCube = region.cubes[z, 0];
                Cube neighbourCube = neighbour.region.cubes[z, region.size - 1];

                if (regionCube.isWalkable && neighbourCube.isWalkable) {
                    adjacentCubes.Add(regionCube);
                    CubeUtility.setMaterial(regionCube, WorldUtility.Instance.entranceMaterial);

                    if (z != region.size - 1) {
                        continue;
                    }
                }

                if (adjacentCubes.Count > 0) {
                    regionCube = adjacentCubes[adjacentCubes.Count >> 1];
                    neighbourCube = neighbour.region.cubes[regionCube.zPos, region.size - 1];

                    portals.Add(new Portal(regionCube, neighbourCube));
                    adjacentCubes = new List<Cube>();
                }
            }
        }
    }
}