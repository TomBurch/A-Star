﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cubes;
using Regions;

public class AStar : MonoBehaviour {
    public Material startMaterial;
    public Material targetMaterial;
    public Material visitedMaterial;
    public Material neighbourMaterial;
    public Material pathMaterial;

    public float animationDelay;

    public List<Cube> createPath(Region region, Cube start, Cube target) {
        List<List<Node>> grid = new List<List<Node>>();
        List<Node> unvisited = new List<Node>();
        List<Node> visited = new List<Node>();
        List<Cube> path = new List<Cube>();
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
                    StartCoroutine(CubeUtility.setMaterialAfterDelay(neighbour.cube, neighbourMaterial, animationDelay * whileIncrement));
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
                StartCoroutine(CubeUtility.setMaterialAfterDelay(currentNode.cube, visitedMaterial, animationDelay * whileIncrement));
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

        Node tailNode = currentNode;
        int pathIncrement = 1;
        path.Add(currentNode.cube);

        while (tailNode.previousNode != null) {
            if (tailNode.previousNode.cube.worldObject != start.worldObject) {
                StartCoroutine(CubeUtility.setMaterialAfterDelay(tailNode.previousNode.cube, pathMaterial, (animationDelay * whileIncrement) + (animationDelay * pathIncrement)));
                path.Add(tailNode.previousNode.cube);
            }
            tailNode = tailNode.previousNode;
            pathIncrement++;
        }

        path.Reverse();
        return path;
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
}