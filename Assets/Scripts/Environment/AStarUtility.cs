using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cubes;
using Regions;
using Worlds;

namespace AStar {

    public class AStarUtility : MonoBehaviour {
        public static AStarUtility Instance;

        public Material startMaterial;
        public Material targetMaterial;
        public Material visitedMaterial;
        public Material neighbourMaterial;
        public Material pathMaterial;
        public Material portalMaterial;
        public Material entranceMaterial;

        public float animationDelay;

        void Awake() {
            Instance = this;
        }

        public static AbstractGraph createAbstractGraph(World world) {
            AbstractGraph graph = new AbstractGraph(world);

            // Create each node by region w/ interedges

            foreach (Region region in world.regions) {
                List<Portal> portals = getPortals(region);

                foreach (Portal portal in portals) {
                    AbstractNode entrance = graph.addAbstractNode(portal.entrance);
                    AbstractNode exit = graph.addAbstractNode(portal.exit);

                    entrance.arcs.Add(exit, 1f);
                    exit.arcs.Add(entrance, 1f);
                }
            }

            // Find intraedges

            foreach (AbstractRegion region in graph.regions) {
                for (int i = 0; i < region.nodes.Count - 1; i++) {
                    foreach (AbstractNode node in region.nodes.GetRange(i + 1, region.nodes.Count - (i + 1))) {
                        CubePath path = createPath(region.region, region.nodes[i].cube, node.cube);
            
                        if (path != null) {
                            region.nodes[i].arcs.Add(node, path.weight);
                            node.arcs.Add(region.nodes[i], path.weight);
                        }
                    }
                }
            }

            return graph;
        }

        public static CubePath createPath(Region region, Cube start, Cube target, bool animate = false) {
            //print("Pathing from [" + start.g_xPos + ", " + start.g_zPos + "] to [" + target.g_xPos + ", " + target.g_zPos + "]");
            List<List<Node>> grid = new List<List<Node>>();
            List<Node> unvisited = new List<Node>();
            List<Node> visited = new List<Node>();
            Node currentNode = null;

            //CubeUtility.setMaterial(start, startMaterial);
            //CubeUtility.setMaterial(target, targetMaterial);

            // Wrap each Cube into a Node (giving it a weight variable)
            // Assign all nodes as 0 (start) or infinity
            for (int z = 0; z < WorldUtility.Instance.regionSize; z++) {
                List<Node> row = new List<Node>();

                for (int x = 0; x < WorldUtility.Instance.regionSize; x++) {
                    Cube cube = region.cubes[z, x];

                    if (cube.worldObject != start.worldObject) {
                        Node newNode = new Node(cube, Mathf.Infinity);
                        row.Add(newNode);
                        unvisited.Add(newNode);
                    }
                    else {
                        currentNode = new Node(cube, 0);
                        row.Add(currentNode);
                        unvisited.Add(currentNode);
                    }
                }

                grid.Add(row);
            }

            int whileIncrement = 1;

            while (true) {
                List<Node> neighbours = getNeighbours(currentNode, grid);

                foreach (Node neighbour in neighbours) {
                    //if (animate) {
                    //    if ((neighbour.cube.worldObject != start.worldObject) && neighbour.cube.worldObject != target.worldObject) {
                    //        Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(neighbour.cube, Instance.neighbourMaterial, Instance.animationDelay * whileIncrement));
                    //    }
                    //}

                    //Dijkstra's -> f = g + speedModifier
                    //float tentativeWeight = currentNode.weight + neighbour.cube.speedModifier;

                    //A* manhattan -> f = g + speedModifier + h
                    //print("[" + currentNode.cube.g_xPos + ", " + currentNode.cube.g_zPos + "] [" + neighbour.cube.g_xPos + ", " + neighbour.cube.g_zPos + "] g: " + currentNode.weight + " / speed: " + CubeUtility.SpeedModifiers[neighbour.cube.GetType().ToString()] + " / h: " + manhattan(neighbour.cube, target));
                    float tentativeWeight = currentNode.weight + CubeUtility.SpeedModifiers[neighbour.cube.GetType().ToString()] + manhattan(neighbour.cube, target);

                    if (tentativeWeight < neighbour.weight) {
                        neighbour.weight = tentativeWeight;
                        neighbour.previousNode = currentNode;
                    }
                }

                unvisited.Remove(currentNode);
                visited.Add(currentNode);
                currentNode.visited = true;

                //if (animate) {
                //    if ((currentNode.cube.worldObject != start.worldObject) && currentNode.cube.worldObject != target.worldObject) {
                //        Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(currentNode.cube, Instance.visitedMaterial, Instance.animationDelay * whileIncrement));
                //    }
                //}

                if (currentNode.cube.worldObject == target.worldObject) {
                    print("Target reached");
                    break;
                }

                currentNode = getClosestNode(unvisited);

                if (currentNode == null) {
                    print("No possible path");
                    return null;
                }
                whileIncrement++;
            }

            List<Cube> path = new List<Cube>();
            Node tailNode = currentNode;
            int pathIncrement = 1;
            float pathWeight = currentNode.weight;

            path.Add(currentNode.cube);

            while (tailNode.previousNode != null) {
                if (tailNode.previousNode.cube.worldObject != start.worldObject) {
                    if (animate) {
                        Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(tailNode.previousNode.cube, Instance.pathMaterial, (Instance.animationDelay * whileIncrement) + (Instance.animationDelay * pathIncrement)));
                    }
                    path.Add(tailNode.previousNode.cube);
                    pathWeight += tailNode.previousNode.weight;
                }
                tailNode = tailNode.previousNode;
                pathIncrement++;
            }

            path.Reverse();
            return new CubePath(path, pathWeight);
        }

        public static float manhattan(Cube c1, Cube c2) {
            return (Mathf.Abs(c2.g_xPos - c1.g_xPos) + Mathf.Abs(c2.g_zPos - c1.g_zPos));
        }

        static List<Portal> getPortals(Region region) {
            List<Portal> portals = new List<Portal>();

            checkRightColumn(region, portals);
            checkTopRow(region, portals);

            return portals;
        }

        static Node getClosestNode(List<Node> nodeSet) {
            Node bestNode = nodeSet[0];
            float bestWeight = bestNode.weight;

            for (int i = 1; i < nodeSet.Count; i++) {
                if (nodeSet[i].weight < bestWeight) {
                    bestNode = nodeSet[i];
                    bestWeight = bestNode.weight;
                }
            }

            if (bestWeight == Mathf.Infinity) {
                return null;
            }

            return bestNode;
        }

        static List<Node> getNeighbours(Node centerNode, List<List<Node>> grid) {
            List<Node> neighbours = new List<Node>();

            int centerX = centerNode.cube.l_xPos;
            int centerZ = centerNode.cube.l_zPos;

            if ((centerX - 1) >= 0) {
                Node neighbourNode = grid[centerZ][centerX - 1];

                if (!neighbourNode.visited && neighbourNode.cube.isWalkable == true) {
                    neighbours.Add(neighbourNode);
                }
            }

            if ((centerZ - 1) >= 0) {
                Node neighbourNode = grid[centerZ - 1][centerX];

                if (!neighbourNode.visited && neighbourNode.cube.isWalkable == true) {
                    neighbours.Add(neighbourNode);
                }
            }

            if ((centerX + 1) <= grid.Count - 1) {
                Node neighbourNode = grid[centerZ][centerX + 1];

                if (!neighbourNode.visited && neighbourNode.cube.isWalkable == true) {
                    neighbours.Add(neighbourNode);
                }
            }

            if ((centerZ + 1) <= grid.Count - 1) {
                Node neighbourNode = grid[centerZ + 1][centerX];

                if (!neighbourNode.visited && neighbourNode.cube.isWalkable == true) {
                    neighbours.Add(neighbourNode);
                }
            }

            return neighbours;
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
                    CubeUtility.setMaterial(regionCube, AStarUtility.Instance.entranceMaterial);

                    if (z != regionSize - 1) {
                        continue;
                    }
                }

                if (adjacentCubes.Count > 0) {
                    regionCube = adjacentCubes[adjacentCubes.Count >> 1];
                    neighbourCube = neighbour.cubes[regionCube.l_zPos, 0];

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
                    CubeUtility.setMaterial(regionCube, AStarUtility.Instance.entranceMaterial);

                    if (x != regionSize - 1) {
                        continue;
                    }
                }

                if (adjacentCubes.Count > 0) {
                    regionCube = adjacentCubes[adjacentCubes.Count >> 1];
                    neighbourCube = neighbour.cubes[0, regionCube.l_xPos];

                    portals.Add(new Portal(regionCube, neighbourCube));
                    adjacentCubes = new List<Cube>();
                }
            }
        }

    }

    public class CubePath {
        public float weight;
        public List<Cube> cubes;

        public CubePath(List<Cube> cubes, float weight) {
            this.cubes = cubes;
            this.weight = weight;
        }
    }

    public class Node {
        public float weight;
        public Cube cube;
        public bool visited;
        public Node previousNode;

        public Node(Cube cube, float weight) {
            this.weight = weight;
            this.cube = cube;
            this.visited = false;
            this.previousNode = null;
        }
    }

    public class AbstractGraph {
        public List<AbstractNode> nodes;
        public AbstractRegion[,] regions;

        public AbstractGraph(World world) {
            nodes = new List<AbstractNode>();
            regions = new AbstractRegion[world.size, world.size];

            for (int z = 0; z < world.size; z++) {
                for (int x = 0; x < world.size; x++) {
                    regions[z, x] = new AbstractRegion(world.regions[z, x]);
                }
            }
        }

        public AbstractNode addAbstractNode(Cube cube, bool temporary = false) {
            if (cube.worldObject.tag == "Node") { return getAbstractNode(cube); }

            AbstractNode newNode = new AbstractNode(cube);
            AbstractRegion region = this.regions[cube.region.zPos, cube.region.xPos];

            CubeUtility.setMaterial(cube, AStarUtility.Instance.portalMaterial);

            if (temporary) {
                cube.worldObject.tag = "TempNode";

                foreach (AbstractNode node in region.nodes) {
                    CubePath path = AStarUtility.createPath(newNode.cube.region, newNode.cube, node.cube);

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
            return this.nodes.Find(x => x.cube.worldObject == cube.worldObject);
        }
        
        public List<Cube> createAbstractPath(Cube startCube, Cube targetCube, bool animate = false) {
            AbstractNode start = this.addAbstractNode(startCube, true);
            AbstractNode target = this.addAbstractNode(targetCube, true);
            List<AbstractNode> unvisited = new List<AbstractNode>();

            if (animate) {
                CubeUtility.setMaterial(start.cube, AStarUtility.Instance.startMaterial);
                CubeUtility.setMaterial(target.cube, AStarUtility.Instance.targetMaterial);
            }

            foreach (AbstractNode node in this.nodes) {
                node.weight = Mathf.Infinity;
                node.prevNode = null;
                unvisited.Add(node);
            }
            start.weight = 0;

            int whileIncrement = 0;

            while (unvisited.Count != 0) {
                whileIncrement++;
                AbstractNode bestNode = this.getBestNode(unvisited);

                if (bestNode == target) { break; }     // Path found
                if (bestNode == null) { return null; } // No possible path

                unvisited.Remove(bestNode);

                if (animate) {
                    if ((bestNode.cube.worldObject != target.cube.worldObject) && (bestNode.cube.worldObject != start.cube.worldObject)) {
                        AStarUtility.Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(bestNode.cube, AStarUtility.Instance.visitedMaterial, AStarUtility.Instance.animationDelay * whileIncrement));
                    }
                }

                foreach (var arc in bestNode.arcs) {
                    AbstractNode neighbourNode = arc.Key;

                    if (unvisited.Contains(neighbourNode)) {
                        if (animate) {
                            if ((neighbourNode.cube.worldObject != target.cube.worldObject) && (neighbourNode.cube.worldObject != start.cube.worldObject)) {
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
                if (animate) {
                    AStarUtility.Instance.StartCoroutine(CubeUtility.setMaterialAfterDelay(tailNode.cube, AStarUtility.Instance.pathMaterial, (AStarUtility.Instance.animationDelay * whileIncrement) + (AStarUtility.Instance.animationDelay * pathIncrement)));
                }

                path.Insert(0, tailNode.cube);
                tailNode = tailNode.prevNode;
            }

            path.Remove(start.cube);

            this.removeAbstractNode(start);
            this.removeAbstractNode(target);

            return path;
        }

        AbstractNode getBestNode(List<AbstractNode> list) {
            AbstractNode bestNode = list[0];

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
    }

    public class AbstractRegion {
        public List<AbstractNode> nodes;
        public Region region;

        public AbstractRegion(Region region) {
            this.nodes = new List<AbstractNode>();
            this.region = region;
        }
    }

    public class AbstractNode {
        public Dictionary<AbstractNode, float> arcs;
        public Cube cube;
        public float weight;
        public AbstractNode prevNode;

        public AbstractNode(Cube cube) {
            arcs = new Dictionary<AbstractNode, float>();
            this.cube = cube;
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