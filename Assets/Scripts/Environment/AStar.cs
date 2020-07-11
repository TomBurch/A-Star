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

        CubeUtility.setMaterial(start, startMaterial);
        CubeUtility.setMaterial(target, targetMaterial);

        // Wrap each Cube into a Node (giving it a weight variable)
        // Assign all nodes as 0 (start) or infinity
        for (int z = 0; z < region.size; z++) {
            List<Node> row = new List<Node>();

            for (int x = 0; x < region.size; x++) {
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
                if ((neighbour.cube.worldObject != start.worldObject) && neighbour.cube.worldObject != target.worldObject) {
                    //StartCoroutine(CubeUtility.setMaterialAfterDelay(neighbour.cube, neighbourMaterial, animationDelay * whileIncrement));
                }

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

            if ((currentNode.cube.worldObject != start.worldObject) && currentNode.cube.worldObject != target.worldObject) {
                //StartCoroutine(CubeUtility.setMaterialAfterDelay(currentNode.cube, visitedMaterial, animationDelay * whileIncrement));
            }

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
                //StartCoroutine(CubeUtility.setMaterialAfterDelay(tailNode.previousNode.cube, pathMaterial, (animationDelay * whileIncrement) + (animationDelay * pathIncrement)));
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
        return (Mathf.Abs(target.xPos - node.cube.xPos) + Mathf.Abs(target.zPos - node.cube.zPos));
    }

    private List<Node> getNeighbours(Node centerNode, List<List<Node>> grid) {
        List<Node> neighbours = new List<Node>();

        int centerX = centerNode.cube.xPos;
        int centerZ = centerNode.cube.zPos;

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

    class RegionNode {
        public Region region;

        public RegionNode(Region region) {
            this.region = region;
        }
    }

    class Portal {
        public Cube entrance;
        public Cube exit;
        public Dictionary<Portal, float> arcs;

        public Portal(Cube entrance, Cube exit) {
            this.entrance = entrance;
            this.exit = exit;
            this.arcs = new Dictionary<Portal, float>();
        }
    }

    public void createAbstractGraph(World world) {
        RegionNode[,] grid = new RegionNode[world.size, world.size];

        for (int z = 0; z < world.size; z++) {
            for (int x = 0; x < world.size; x++) {
                grid[z, x] = new RegionNode(world.regions[z, x]);
            }
        }

        AStar astar = (AStar)FindObjectOfType(typeof(AStar));

        List<Portal> allPortals = new List<Portal>();

        foreach (RegionNode node in grid) {
            List<Portal> portals = getPortals(grid, node.region);

            for (int i = 0; i < portals.Count - 1; i++) {
                foreach (Portal portal in portals.GetRange(i + 1, portals.Count - (i + 1))) {
                    if (portal == portals[i]) { continue; }

                    CubePath path = astar.createPath(node.region, portals[i].entrance, portal.entrance);
                    if (path != null) {
                        portals[i].arcs.Add(portal, path.distance);
                        portal.arcs.Add(portals[i], path.distance);
                    }
                }
                allPortals.Add(portals[i]);
            }
        }

        foreach (Portal portal in allPortals) {
            CubeUtility.setMaterial(portal.entrance, WorldUtility.Instance.portalMaterial);
            CubeUtility.setMaterial(portal.exit, WorldUtility.Instance.portalMaterial);
            print(portal.entrance.region.xPos + "," + portal.entrance.region.zPos + " - " + portal.entrance.xPos + "," + portal.entrance.zPos + " : " + portal.arcs.Count);
        }
    }

    List<Portal> getPortals(RegionNode[,] grid, Region region) {
        List<Portal> portals = new List<Portal>();

        checkLeftColumn(grid, region, portals);
        checkBottomRow(grid, region, portals);
        checkRightColumn(grid, region, portals);
        checkTopRow(grid, region, portals);

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