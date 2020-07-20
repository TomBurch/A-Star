using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cubes;
using Regions;
using Worlds;

public class AStar : MonoBehaviour {
    public Material startMaterial;
    public Material targetMaterial;
    public Material visitedMaterial;
    public Material neighbourMaterial;
    public Material pathMaterial;

    public float animationDelay;

    public CubePath createPath(Region region, Cube start, Cube target) {
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
                } else {
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
                //if ((neighbour.cube.worldObject != start.worldObject) && neighbour.cube.worldObject != target.worldObject) {
                    //StartCoroutine(CubeUtility.setMaterialAfterDelay(neighbour.cube, neighbourMaterial, animationDelay * whileIncrement));
                //}

                //Dijkstra's -> f = g + speedModifier
                //float tentativeWeight = currentNode.weight + neighbour.cube.speedModifier;

                //A* manhattan -> f = g + speedModifier + h
                float tentativeWeight = currentNode.weight + CubeUtility.SpeedModifiers[neighbour.cube.GetType().ToString()] + manhattan(neighbour, target);

                if (tentativeWeight < neighbour.weight) {
                    neighbour.weight = tentativeWeight;
                    neighbour.previousNode = currentNode;
                }
            }

            unvisited.Remove(currentNode);
            visited.Add(currentNode);
            currentNode.visited = true;

            //if ((currentNode.cube.worldObject != start.worldObject) && currentNode.cube.worldObject != target.worldObject) {
                //StartCoroutine(CubeUtility.setMaterialAfterDelay(currentNode.cube, visitedMaterial, animationDelay * whileIncrement));
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
        float distance = currentNode.weight;

        path.Add(currentNode.cube);

        while (tailNode.previousNode != null) {
            if (tailNode.previousNode.cube.worldObject != start.worldObject) {
                ////StartCoroutine(CubeUtility.setMaterialAfterDelay(tailNode.previousNode.cube, pathMaterial, (animationDelay * whileIncrement) + (animationDelay * pathIncrement)));
                path.Add(tailNode.previousNode.cube);
                distance += tailNode.previousNode.weight;
            }
            tailNode = tailNode.previousNode;
            pathIncrement++;
        }

        path.Reverse();
        return new CubePath(path, distance);
    }

    private float manhattan(Node node, Cube target) {
        return (Mathf.Abs(target.l_xPos - node.cube.l_xPos) + Mathf.Abs(target.l_zPos - node.cube.l_zPos));
    }

    private List<Node> getNeighbours(Node centerNode, List<List<Node>> grid) {
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

    private Node getClosestNode(List<Node> nodeSet) {
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

    public class CubePath {
        public float distance;
        public List<Cube> cubes;

        public CubePath(List<Cube> cubes, float distance) {
            this.cubes = cubes;
            this.distance = distance;
        }
    }
    
    private class Node {
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

/// <summary>
/// ///////
/// </summary>

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
        public float distance;
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

    public List<AbstractNode> createAbstractPath(AbstractGraph graph, AbstractNode start, AbstractNode target) {
        List<AbstractNode> path = new List<AbstractNode>();
        List<AbstractNode> unvisited = new List<AbstractNode>();

        CubeUtility.setMaterial(start.cube, startMaterial);
        CubeUtility.setMaterial(target.cube, targetMaterial);

        foreach (AbstractNode node in graph.nodes) {
            node.distance = Mathf.Infinity;
            node.prevNode = null;
            unvisited.Add(node);
        }
        start.distance = 0;

        int whileIncrement = 0;

        while (unvisited.Count != 0) {
            whileIncrement++;
            AbstractNode bestNode = getBestNode(unvisited);
            print(bestNode.cube.g_xPos + ", " + bestNode.cube.g_zPos + " - " + bestNode.distance);
            unvisited.Remove(bestNode);

            if ((bestNode.cube.worldObject != target.cube.worldObject) && (bestNode.cube.worldObject != start.cube.worldObject)) {
                StartCoroutine(CubeUtility.setMaterialAfterDelay(bestNode.cube, visitedMaterial, animationDelay * whileIncrement));
            }

            if (bestNode == target) { break; } 

            foreach (var arc in bestNode.arcs) {
                AbstractNode neighbourNode = arc.Key;

                if (unvisited.Contains(neighbourNode)) {
                    if ((neighbourNode.cube.worldObject != target.cube.worldObject) && (neighbourNode.cube.worldObject != start.cube.worldObject)) {
                        StartCoroutine(CubeUtility.setMaterialAfterDelay(neighbourNode.cube, neighbourMaterial, animationDelay * whileIncrement));
                    }

                    float tentativeWeight = bestNode.distance + arc.Value;

                    if (tentativeWeight < neighbourNode.distance) {
                        neighbourNode.distance = tentativeWeight;
                        neighbourNode.prevNode = bestNode;
                    }
                }
            }
        }

        return path;
    }

    public AbstractNode getBestNode(List<AbstractNode> list) {
        AbstractNode bestNode = list[0];

        for (int i = 1; i < list.Count; i++) {
            if (list[i].distance < bestNode.distance) {
                bestNode = list[i];
            }
        }

        return bestNode;
    }

    public void createAbstractGraph(World world) {
        AStar astar = (AStar)FindObjectOfType(typeof(AStar));

        AbstractGraph graph = new AbstractGraph(world);

        // Create each node by region w/ interedges

        foreach (Region region in world.regions) {
            List<Portal> portals = getPortals(region);

            foreach (Portal portal in portals) {
                AbstractNode entranceNode = new AbstractNode(portal.entrance);
                AbstractNode exitNode = new AbstractNode(portal.exit);

                entranceNode.arcs.Add(exitNode, 1f);
                exitNode.arcs.Add(entranceNode, 1f);

                CubeUtility.setMaterial(portal.entrance, WorldUtility.Instance.portalMaterial);
                CubeUtility.setMaterial(portal.exit, WorldUtility.Instance.portalMaterial);

                graph.regions[portal.entrance.region.zPos, portal.entrance.region.xPos].nodes.Add(entranceNode);
                graph.regions[portal.exit.region.zPos, portal.exit.region.xPos].nodes.Add(exitNode);

                graph.nodes.Add(entranceNode);
                graph.nodes.Add(exitNode);
            }
        }

        // Find intraedges

        foreach (AbstractRegion region in graph.regions) {
            for (int i = 0; i < region.nodes.Count - 1; i++) {
                foreach (AbstractNode node in region.nodes.GetRange(i + 1, region.nodes.Count - (i + 1))) {
                    CubePath path = astar.createPath(region.region, region.nodes[i].cube, node.cube);

                    if (path != null) {
                        region.nodes[i].arcs.Add(node, path.distance);
                        node.arcs.Add(region.nodes[i], path.distance);
                    }
                }
            }
        }

        AbstractNode testNode = graph.nodes[0];
        AbstractNode testNode2 = graph.nodes[52];
        print("node1 = " + testNode.cube.g_xPos + ", " + testNode.cube.g_zPos);
        print("node2 = " + testNode2.cube.g_xPos + ", " + testNode2.cube.g_zPos);

        createAbstractPath(graph, testNode, testNode2);
    }

    List<Portal> getPortals(Region region) {
        List<Portal> portals = new List<Portal>();

        checkRightColumn(region, portals);
        checkTopRow(region, portals);

        return portals;
    }

    void checkRightColumn(Region region, List<Portal> portals) {
        if (!((region.xPos + 1) <= region.world.regions.GetLength(1) - 1)) { return; }

        Region neighbour = region.world.regions[region.zPos, region.xPos + 1];
        List<Cube> adjacentCubes = new List<Cube>();
        int regionSize = WorldUtility.Instance.regionSize;

        for (int z = 0; z < regionSize; z++) {
            Cube regionCube = region.cubes[z, regionSize - 1];
            Cube neighbourCube = neighbour.cubes[z, 0];

            if (regionCube.isWalkable && neighbourCube.isWalkable) {
                adjacentCubes.Add(regionCube);
                CubeUtility.setMaterial(regionCube, WorldUtility.Instance.entranceMaterial);

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

    void checkTopRow(Region region, List<Portal> portals) {
        if (!((region.zPos + 1) <= region.world.regions.GetLength(0) - 1)) { return; }

        Region neighbour = region.world.regions[region.zPos + 1, region.xPos];
        List<Cube> adjacentCubes = new List<Cube>();
        int regionSize = WorldUtility.Instance.regionSize;

        for (int x = 0; x < regionSize; x++) {
            Cube regionCube = region.cubes[regionSize - 1, x];
            Cube neighbourCube = neighbour.cubes[0, x];

            if (regionCube.isWalkable && neighbourCube.isWalkable) {
                adjacentCubes.Add(regionCube);
                CubeUtility.setMaterial(regionCube, WorldUtility.Instance.entranceMaterial);

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