using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cubes;
using Regions;
using Worlds;

namespace AStar {

    public class AStarUtility : MonoBehaviour {
        public static AStarUtility Instance;

        public Material visitedMaterial;
        public Material neighbourMaterial;
        public Material pathMaterial;
        public Material portalMaterial;
        public Material entranceMaterial;

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

            while (unvisited.Count != 0) {
                whileIncrement++;
                bestNode = AStarUtility.getBestNode<CubeNode>(unvisited);

                if (bestNode == null) { return null; } //No possible path
                if (bestNode.cube == target) { break; } //Path found
                
                unvisited.Remove(bestNode);

                if (animate) {
                    if (bestNode.cube.worldObject.tag == "Cube") {
                        Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(bestNode.cube, Instance.visitedMaterial, Instance.animationDelay * whileIncrement));
                    }
                }

                List<CubeNode> neighbours = getNeighbours(bestNode, grid);

                foreach (CubeNode neighbour in neighbours) {
                    if (unvisited.Contains(neighbour)) {
                        if (animate) {
                            if (neighbour.cube.worldObject.tag == "Cube") {
                                Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(neighbour.cube, Instance.neighbourMaterial, Instance.animationDelay * whileIncrement));
                            }
                        }

                        //Dijkstra's -> f = g + speedModifier
                        float tentativeWeight = bestNode.weight + CubeUtility.getSpeedModifier(neighbour.cube);

                        //A* manhattan -> f = g + speedModifier + h
                        //print("[" + currentNode.cube.g_xPos + ", " + currentNode.cube.g_zPos + "] [" + neighbour.cube.g_xPos + ", " + neighbour.cube.g_zPos + "] g: " + currentNode.weight + " / speed: " + CubeUtility.SpeedModifiers[neighbour.cube.GetType().ToString()] + " / h: " + manhattan(neighbour.cube, target));

                        //float tentativeWeight = bestNode.weight + CubeUtility.SpeedModifiers[neighbour.cube.GetType().ToString()] + manhattan(neighbour.cube, target);

                        if (tentativeWeight < neighbour.weight) {
                            neighbour.weight = tentativeWeight;
                            neighbour.prevNode = bestNode;
                        }
                    }
                }
            }

            List<Cube> path = new List<Cube>();
            CubeNode tailNode = bestNode;
            int pathIncrement = 0;

            while (tailNode.cube != start) {
                pathIncrement++;
                if (animate) {
                    if (tailNode.cube.worldObject.tag == "Cube") {
                        Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(tailNode.cube, Instance.pathMaterial, (Instance.animationDelay * whileIncrement) + (Instance.animationDelay * pathIncrement)));
                    }
                }

                path.Insert(0, tailNode.cube);
                tailNode = tailNode.prevNode;    
            }

            return new CubePath(path, bestNode.weight);
        }

        public static float manhattan(Cube c1, Cube c2) {
            Vector3 c1Pos = CubeUtility.getGlobalPos(c1);
            Vector3 c2Pos = CubeUtility.getGlobalPos(c2);
            return (Mathf.Abs(c2Pos.x - c1Pos.x) + Mathf.Abs(c2Pos.z - c1Pos.z));
        }

        public static T getBestNode<T>(List<T> list) where T : Node {
            T bestNode = list[0];

            for (int i = 1; i < list.Count; i++) {
                if (list[i].weight < bestNode.weight) {
                    bestNode = list[i];
                }
            }

            if (bestNode.weight == Mathf.Infinity) {
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
                node.weight = Mathf.Infinity;
                node.prevNode = null;
                unvisited.Add(node);
            }

            start.weight = 0;
            target.weight = Mathf.Infinity;

            int whileIncrement = 0;
            AbstractNode bestNode = null;

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

                if (AStarUtility.Instance.animateAbstractPath) {
                    if ((bestNode.cube != target.cube) && (bestNode.cube != start.cube)) {
                        AStarUtility.Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(bestNode.cube, AStarUtility.Instance.visitedMaterial, AStarUtility.Instance.animationDelay * whileIncrement));
                    }
                }

                foreach (var arc in bestNode.arcs) {
                    AbstractNode neighbourNode = arc.Key;

                    if (unvisited.Contains(neighbourNode)) {
                        if (AStarUtility.Instance.animateAbstractPath) {
                            if ((neighbourNode.cube != target.cube) && (neighbourNode.cube != start.cube)) {
                                AStarUtility.Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(neighbourNode.cube, AStarUtility.Instance.neighbourMaterial, AStarUtility.Instance.animationDelay * whileIncrement));
                            }
                        }

                        float tentativeWeight = bestNode.weight + arc.Value;

                        if (tentativeWeight < neighbourNode.weight) {
                            neighbourNode.weight = tentativeWeight;
                            neighbourNode.prevNode = bestNode;
                        }
                    }
                }
            }

            List<Cube> path = new List<Cube>();
            AbstractNode tailNode = target;
            int pathIncrement = 0;

            while (tailNode != start) {
                pathIncrement++;
                if (AStarUtility.Instance.animateAbstractPath) {
                    AStarUtility.Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(tailNode.cube, AStarUtility.Instance.pathMaterial, (AStarUtility.Instance.animationDelay * whileIncrement) + (AStarUtility.Instance.animationDelay * pathIncrement)));
                }

                path.Insert(0, tailNode.cube);
                tailNode = tailNode.prevNode;
            }

            path.Remove(start.cube);

            this.removeAbstractNode(start);
            this.removeAbstractNode(target);

            stopwatch.Stop();
            AStarUtility.print("Created abstract path in " + stopwatch.ElapsedMilliseconds + " ms");
            stopwatch.Reset();

            return new CubePath(path, target.weight);
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
        public float weight;

        public Node(Cube cube, float weight) {
            this.cube = cube;
            this.weight = weight;
        }
    }

    public class CubeNode : Node {
        public CubeNode prevNode;

        public CubeNode(Cube cube, float weight = Mathf.Infinity) : base(cube, weight) { }
    }

    public class AbstractNode : Node {
        public Dictionary<AbstractNode, float> arcs;
        public AbstractNode prevNode;

        public AbstractNode(Cube cube, float weight = Mathf.Infinity) : base(cube, weight) { 
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