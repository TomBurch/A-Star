using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cubes;
using Regions;
using Worlds;
using System.IO;
using System;

namespace AStar {

    public class AStarUtility : MonoBehaviour {
        public static AStarUtility Instance;
        public enum PathMethod {
            Dijkstras,
            AStar_Manhattan
        }

        public Material visitedMaterial;
        public Material neighbourMaterial;
        public Material pathMaterial;
        public Material portalMaterial;
        public Material entranceMaterial;

        public PathMethod pathMethod;
        public PathMethod abstractPathMethod;
        public bool animatePath;
        public bool animateAbstractPath;
        public bool drawPortals;
        public bool drawEntrances;
        public float animationDelay;
        
        void Awake() {
            Instance = this;
        }

        public static CubePath createPath(Cube start, Cube target, bool animate = false) {
            if (!target.isWalkable) { return null; }

            CubeNode[,] grid = new CubeNode[WorldUtility.Instance.regionSize, WorldUtility.Instance.regionSize];
            List<CubeNode> unvisited = new List<CubeNode>();

            // Wrap each Cube into a Node (giving it a weight variable)
            // Assign all nodes as 0 (start) or infinity
            for (int z = 0; z < WorldUtility.Instance.regionSize; z++) {
                for (int x = 0; x < WorldUtility.Instance.regionSize; x++) {
                    Cube cube = start.region.cubes[z, x];

                    if (cube.isWalkable) {
                        grid[z, x] = new CubeNode(cube, Mathf.Infinity);
                        unvisited.Add(grid[z, x]);
                    } else {
                        grid[z, x] = null;
                    }
                }
            }

            grid[start.zPos, start.xPos] = new CubeNode(start, 0);
            unvisited.Add(grid[start.zPos, start.xPos]);

            int whileIncrement = 0;
            CubeNode bestNode = null;
            Action<CubeNode, CubeNode, float, Cube> updateWeight = getWeightMethod(Instance.pathMethod);

            while (unvisited.Count != 0) {
                whileIncrement++;
                bestNode = AStarUtility.getBestNode<CubeNode>(unvisited);

                if (bestNode == null) { return null; } //No possible path
                if (bestNode.cube == target) { break; } //Path found
                
                unvisited.Remove(bestNode);

                //if (animate) {
                //    if (bestNode.cube.worldObject.tag == "Cube") {
                //        Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(bestNode.cube, Instance.visitedMaterial, Instance.animationDelay * whileIncrement));
                //    }
                //}

                List<CubeNode> neighbours = getNeighbours(bestNode, grid);

                foreach (CubeNode neighbourNode in neighbours) {
                    if (unvisited.Contains(neighbourNode)) {
                        //if (animate) {
                        //    if (neighbour.cube.worldObject.tag == "Cube") {
                        //        Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(neighbour.cube, Instance.neighbourMaterial, Instance.animationDelay * whileIncrement));
                        //    }
                        //}

                        updateWeight(bestNode, neighbourNode, CubeUtility.getSpeedModifier(neighbourNode.cube), target);
                    }
                }
            }

            List<Cube> path = new List<Cube>();
            CubeNode tailNode = bestNode;
            int pathIncrement = 0;

            while (tailNode.cube != start) {
                pathIncrement++;
                //if (animate) {
                //    if (tailNode.cube.worldObject.tag == "Cube") {
                //        Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(tailNode.cube, Instance.pathMaterial, (Instance.animationDelay * whileIncrement) + (Instance.animationDelay * pathIncrement)));
                //    }
                //}

                path.Insert(0, tailNode.cube);
                tailNode = (CubeNode) tailNode.prevNode;    
            }

            return new CubePath(path, bestNode.g);
        }

        public static T getBestNode<T>(List<T> list) where T : Node {
            T bestNode = list[0];

            for (int i = 1; i < list.Count; i++) {
                if ((list[i].g + list[i].h) < (bestNode.g + bestNode.h)) {
                    bestNode = list[i];
                }
            }

            if (bestNode.g == Mathf.Infinity) {
                return null;
            }

            return bestNode;
        }

        static List<CubeNode> getNeighbours(CubeNode centerNode, CubeNode[,] grid) {
            List<CubeNode> neighbours = new List<CubeNode>();

            int centerX = centerNode.cube.xPos;
            int centerZ = centerNode.cube.zPos;

            if ((centerX - 1) >= 0) {
                CubeNode neighbourNode = grid[centerZ, centerX - 1];

                if (neighbourNode != null) {
                    neighbours.Add(neighbourNode);
                }
            }

            if ((centerZ - 1) >= 0) {
                CubeNode neighbourNode = grid[centerZ - 1, centerX];

                if (neighbourNode != null) {
                    neighbours.Add(neighbourNode);
                }
            }

            if ((centerX + 1) <= WorldUtility.Instance.regionSize - 1) {
                CubeNode neighbourNode = grid[centerZ, centerX + 1];

                if (neighbourNode != null) {
                    neighbours.Add(neighbourNode);
                }
            }

            if ((centerZ + 1) <= WorldUtility.Instance.regionSize - 1) {
                CubeNode neighbourNode = grid[centerZ + 1, centerX];

                if (neighbourNode != null) {
                    neighbours.Add(neighbourNode);
                }
            }

            return neighbours;
        }
    
        static void dijkstras<T>(T bestNode, T neighbourNode, float arcWeight, Cube target) where T : Node {
            float tentativeWeight = bestNode.g + arcWeight;

            if (tentativeWeight < neighbourNode.g) {
                neighbourNode.g = tentativeWeight;
                neighbourNode.prevNode = bestNode;
            }
        }

        static void astar_manhattan<T>(T bestNode, T neighbourNode, float arcWeight, Cube target) where T : Node {
            float tentative_g = bestNode.g + arcWeight;
            float tentative_h = manhattan(neighbourNode.cube, target);

            if ((tentative_g + tentative_h) < (neighbourNode.g + neighbourNode.h)) {
                neighbourNode.g = tentative_g;
                neighbourNode.h = tentative_h;
                neighbourNode.prevNode = bestNode;
            }
        }
        
        public static Action<Node, Node, float, Cube> getWeightMethod(PathMethod method) {
            switch (method) {
                case (PathMethod.Dijkstras):
                    return dijkstras;

                case (PathMethod.AStar_Manhattan):
                    return astar_manhattan;

                default:
                    return null;
            }
        }

        public static float manhattan(Cube c1, Cube c2) {
            Vector3 c1Pos = CubeUtility.getGlobalPos(c1);
            Vector3 c2Pos = CubeUtility.getGlobalPos(c2);
            return (Mathf.Abs(c2Pos.x - c1Pos.x) + Mathf.Abs(c2Pos.z - c1Pos.z));
        }
    }

    public class AbstractGraph {
        public List<AbstractNode> nodes;
        public AbstractRegion[,] regions;

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        public AbstractGraph(World world) {
            nodes = new List<AbstractNode>();
            regions = new AbstractRegion[world.size, world.size];

            for (int z = 0; z < world.size; z++) {
                for (int x = 0; x < world.size; x++) {
                    regions[z, x] = new AbstractRegion(world.regions[z, x]);
                }
            }

            foreach (Region region in world.regions) {
                this.linkPortals(region);
            }

            foreach (AbstractRegion region in this.regions) {
                this.linkIntraedges(region);
            }
        }

        void linkPortals(Region region) {
            List<Portal> portals = new List<Portal>();

            checkRightColumn(region, portals);
            checkTopRow(region, portals);

            foreach (Portal portal in portals) {
                AbstractNode entrance = this.addAbstractNode(portal.entrance);
                AbstractNode exit = this.addAbstractNode(portal.exit);

                entrance.arcs.Add(exit, 1f);
                exit.arcs.Add(entrance, 1f);
            }
        }

        void linkIntraedges(AbstractRegion region) {
            for (int i = 0; i < region.nodes.Count - 1; i++) {
                foreach (AbstractNode node in region.nodes.GetRange(i + 1, region.nodes.Count - (i + 1))) {
                    CubePath path = AStarUtility.createPath(region.nodes[i].cube, node.cube);

                    if (path != null) {
                        region.nodes[i].arcs.Add(node, path.weight);
                        node.arcs.Add(region.nodes[i], path.weight);
                    }
                }
            }
        }

        static void checkRightColumn(Region region, List<Portal> portals) {
            if (!((region.xPos + 1) <= region.world.regions.GetLength(1) - 1)) { return; }

            Region neighbour = region.world.regions[region.zPos, region.xPos + 1];
            List<Cube> adjacentCubes = new List<Cube>();
            int regionSize = WorldUtility.Instance.regionSize;

            for (int z = 0; z < regionSize; z++) {
                Cube regionCube = region.cubes[z, regionSize - 1];
                Cube neighbourCube = neighbour.cubes[z, 0];

                if (regionCube.isWalkable && neighbourCube.isWalkable) {
                    adjacentCubes.Add(regionCube);

                    if (AStarUtility.Instance.drawEntrances) {
                        CubeUtility.setMaterial(regionCube, AStarUtility.Instance.entranceMaterial);
                    }

                    if (z != regionSize - 1) {
                        continue;
                    }
                }

                if (adjacentCubes.Count > 0) {
                    regionCube = adjacentCubes[adjacentCubes.Count >> 1];
                    neighbourCube = neighbour.cubes[regionCube.zPos, 0];

                    portals.Add(new Portal(regionCube, neighbourCube));
                    adjacentCubes = new List<Cube>();
                }
            }
        }

        static void checkTopRow(Region region, List<Portal> portals) {
            if (!((region.zPos + 1) <= region.world.regions.GetLength(0) - 1)) { return; }

            Region neighbour = region.world.regions[region.zPos + 1, region.xPos];
            List<Cube> adjacentCubes = new List<Cube>();
            int regionSize = WorldUtility.Instance.regionSize;

            for (int x = 0; x < regionSize; x++) {
                Cube regionCube = region.cubes[regionSize - 1, x];
                Cube neighbourCube = neighbour.cubes[0, x];

                if (regionCube.isWalkable && neighbourCube.isWalkable) {
                    adjacentCubes.Add(regionCube);

                    if (AStarUtility.Instance.drawEntrances) {
                        CubeUtility.setMaterial(regionCube, AStarUtility.Instance.entranceMaterial);
                    }

                    if (x != regionSize - 1) {
                        continue;
                    }
                }

                if (adjacentCubes.Count > 0) {
                    regionCube = adjacentCubes[adjacentCubes.Count >> 1];
                    neighbourCube = neighbour.cubes[0, regionCube.xPos];

                    portals.Add(new Portal(regionCube, neighbourCube));
                    adjacentCubes = new List<Cube>();
                }
            }
        }

        public AbstractNode addAbstractNode(Cube cube, float weight = Mathf.Infinity, bool temporary = false) {
            if (cube.worldObject.tag == "Node") { return getAbstractNode(cube); }

            AbstractNode newNode = new AbstractNode(cube, weight);
            AbstractRegion region = this.regions[cube.region.zPos, cube.region.xPos];

            if (AStarUtility.Instance.drawPortals) {
                CubeUtility.setMaterial(cube, AStarUtility.Instance.portalMaterial);
            }

            if (temporary) {
                cube.worldObject.tag = "TempNode";

                foreach (AbstractNode node in region.nodes) {
                    CubePath path = AStarUtility.createPath(newNode.cube, node.cube);

                    if (path != null) {
                        newNode.arcs.Add(node, path.weight);
                        node.arcs.Add(newNode, path.weight);
                    }
                }
            } else {
                cube.worldObject.tag = "Node";
            }

            this.nodes.Add(newNode);
            region.nodes.Add(newNode);

            return newNode;
        }

        public void removeAbstractNode(AbstractNode oldNode) {
            if (oldNode.cube.worldObject.tag == "Node") { return; }
            
            AbstractRegion region = this.regions[oldNode.cube.region.zPos, oldNode.cube.region.xPos];
            this.nodes.Remove(oldNode);
            region.nodes.Remove(oldNode);
            oldNode.arcs = null;
            oldNode.cube.worldObject.tag = "Cube";

            foreach (AbstractNode node in region.nodes) {
                node.arcs.Remove(oldNode);
            }
        }

        public AbstractNode getAbstractNode(Cube cube) {
            return this.nodes.Find(x => x.cube == cube);
        }

        public CubePath createAbstractPath(Cube startCube, Cube targetCube) {
            if (!targetCube.isWalkable) { return null; }
            if (startCube.region == targetCube.region) { return AStarUtility.createPath(startCube, targetCube, AStarUtility.Instance.animatePath); }

            stopwatch.Start();

            AbstractNode start = this.addAbstractNode(startCube, 0f, true);
            AbstractNode target = this.addAbstractNode(targetCube, temporary: true);
            List<AbstractNode> unvisited = new List<AbstractNode>();

            foreach (AbstractNode node in this.nodes) {
                node.g = Mathf.Infinity;
                node.h = 0;
                node.prevNode = null;
                unvisited.Add(node);
            }

            start.g = 0;
            target.g = Mathf.Infinity;

            int whileIncrement = 0;
            AbstractNode bestNode = null;
            Action<Node, Node, float, Cube> updateWeight = AStarUtility.getWeightMethod(AStarUtility.Instance.abstractPathMethod);

            while (unvisited.Count != 0) {
                whileIncrement++;
                bestNode = AStarUtility.getBestNode<AbstractNode>(unvisited);

                if (bestNode == null) {                // No possible path
                    this.removeAbstractNode(start);
                    this.removeAbstractNode(target);

                    stopwatch.Stop();
                    AStarUtility.print("Failed abstract path in " + stopwatch.ElapsedMilliseconds + " ms");
                    stopwatch.Reset();

                    return null; 
                } 
                
                if (bestNode == target) { break; }     // Path found

                unvisited.Remove(bestNode);

                //if (AStarUtility.Instance.animateAbstractPath) {
                //    if ((bestNode.cube != target.cube) && (bestNode.cube != start.cube)) {
                //        AStarUtility.Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(bestNode.cube, AStarUtility.Instance.visitedMaterial, AStarUtility.Instance.animationDelay * whileIncrement));
                //    }
                //}

                foreach (var arc in bestNode.arcs) {
                    AbstractNode neighbourNode = arc.Key;

                    if (unvisited.Contains(neighbourNode)) {
                        //if (AStarUtility.Instance.animateAbstractPath) {
                        //    if ((neighbourNode.cube != target.cube) && (neighbourNode.cube != start.cube)) {
                        //        AStarUtility.Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(neighbourNode.cube, AStarUtility.Instance.neighbourMaterial, AStarUtility.Instance.animationDelay * whileIncrement));
                        //    }
                        //}

                        updateWeight(bestNode, neighbourNode, arc.Value, targetCube);
                    }
                }
            }

            List<Cube> path = new List<Cube>();
            AbstractNode tailNode = target;
            int pathIncrement = 0;

            while (tailNode != start) {
                pathIncrement++;
                //if (AStarUtility.Instance.animateAbstractPath) {
                //    AStarUtility.Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(tailNode.cube, AStarUtility.Instance.pathMaterial, (AStarUtility.Instance.animationDelay * whileIncrement) + (AStarUtility.Instance.animationDelay * pathIncrement)));
                //}

                path.Insert(0, tailNode.cube);
                tailNode = (AbstractNode) tailNode.prevNode;
            }

            path.Remove(start.cube);

            this.removeAbstractNode(start);
            this.removeAbstractNode(target);

            stopwatch.Stop();
            AStarUtility.print("Created abstract path in " + stopwatch.ElapsedMilliseconds + " ms");
            stopwatch.Reset();

            return new CubePath(path, target.g);
        }
    }

    public class AbstractRegion {
        public List<AbstractNode> nodes;
        public Region region;

        public AbstractRegion(Region region) {
            this.nodes = new List<AbstractNode>();
            this.region = region;
        }
    }

    public class Node {
        public Cube cube;
        public float g;
        public float h;
        public Node prevNode;

        public Node(Cube cube, float g) {
            this.cube = cube;
            this.g = g;
            this.h = 0;
        }
    }

    public class CubeNode : Node {
        public CubeNode(Cube cube, float g = Mathf.Infinity) : base(cube, g) { }
    }

    public class AbstractNode : Node {
        public Dictionary<AbstractNode, float> arcs;

        public AbstractNode(Cube cube, float g = Mathf.Infinity) : base(cube, g) { 
            this.arcs = new Dictionary<AbstractNode, float>();
        }
    }

    public class CubePath {
        public List<Cube> cubes;
        public float weight;

        public CubePath(List<Cube> cubes, float weight) {
            this.cubes = cubes;
            this.weight = weight;
        }
    }

    public class Portal {
        public Cube entrance;
        public Cube exit;

        public Portal(Cube entrance, Cube exit) {
            this.entrance = entrance;
            this.exit = exit;
        }
    }
}