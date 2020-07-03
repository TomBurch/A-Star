﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Terrain;
using Cubes;

public class Environment : MonoBehaviour
{
    Terrain.TerrainData terrainData;
    public Transform moverPrefab;

    public Material startMaterial;
    public Material endMaterial;
    public Material visitedMaterial;
    public Material neighbourMaterial;
    public Material pathMaterial;

    public float animationDelay;

    void Start() {
        var grassTerrain = FindObjectOfType<GrassTerrain>();
        terrainData = grassTerrain.Generate(10);

        TerrainCube start = terrainData.terrainCubes[UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(0, 10)];
        TerrainCube end = terrainData.terrainCubes[UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(0, 10)];

        while (start.isWalkable == false) {
            start = terrainData.terrainCubes[UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(0, 10)];
        }

        while (end.isWalkable == false) {
            end = terrainData.terrainCubes[UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(0, 10)];
        }

        start.setMaterial(startMaterial);
        end.setMaterial(endMaterial);

        GameObject moverObject = Instantiate(moverPrefab, start.getPos() + new Vector3(0f, 0.5f, 0f), Quaternion.identity).gameObject;
        Mover moverScript = moverObject.GetComponent<Mover>();

        moverScript.currentPath = createPath(start, end);
    }

    public Terrain.TerrainData getTerrainData() {
        return terrainData;
    }

    public List<Cubes.TerrainCube> createPath(Cubes.TerrainCube start, Cubes.TerrainCube target) {
        List<List<Node>> grid = new List<List<Node>>();
        List<Node> unvisited = new List<Node>();
        List<Node> visited = new List<Node>();
        List<Cubes.TerrainCube> path = new List<Cubes.TerrainCube>();
        Node currentNode = null;

        // Wrap each TerrainCube into a Node (giving it a weight variable)
        // Assign all nodes as 0 (start) or infinity
        for (int z = 0; z < terrainData.size; z++) {
            List<Node> row = new List<Node>();

            for (int x = 0; x < terrainData.size; x++) {
                Cubes.TerrainCube cube = terrainData.terrainCubes[z, x];

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
                if ((neighbour.terrainCube.worldObject != start.worldObject) && neighbour.terrainCube.worldObject != target.worldObject) {
                    StartCoroutine(neighbour.terrainCube.setMaterialAfterDelay(neighbourMaterial, animationDelay * whileIncrement));
                }
                float newWeight = getWeight(start, target, neighbour);

                if (newWeight < neighbour.weight) {
                    neighbour.weight = newWeight;
                    neighbour.previousNode = currentNode;
                }
            }

            unvisited.Remove(currentNode);
            visited.Add(currentNode);
            currentNode.visited = true;

            if ((currentNode.terrainCube.worldObject != start.worldObject) && currentNode.terrainCube.worldObject != target.worldObject) {
                StartCoroutine(currentNode.terrainCube.setMaterialAfterDelay(visitedMaterial, animationDelay * whileIncrement));
            }

            if (currentNode.terrainCube.worldObject == target.worldObject) {
                print("Target reached");
                break;
            }

            currentNode = getClosestNode(unvisited);
            whileIncrement++;
        }

        Node tailNode = currentNode;
        int pathIncrement = 1;
        path.Add(currentNode.terrainCube);

        while (tailNode.previousNode != null) {
            if (tailNode.previousNode.terrainCube.worldObject != start.worldObject) {
                StartCoroutine(tailNode.previousNode.terrainCube.setMaterialAfterDelay(pathMaterial, (animationDelay * whileIncrement) + (animationDelay * pathIncrement)));
                path.Add(tailNode.previousNode.terrainCube);
            }
            tailNode = tailNode.previousNode;
            pathIncrement++;
        }

        path.Reverse();
        return path;
    }

    private float getWeight(Cubes.TerrainCube start, Cubes.TerrainCube target, Node node) {
        //Dijkstra's manhattan
        //return (Mathf.Abs(start.xPos - node.terrainCube.xPos) + Mathf.Abs(start.zPos - node.terrainCube.zPos));

        //A* manhattan/manhattan
        return (Mathf.Abs(start.xPos - node.terrainCube.xPos) + Mathf.Abs(start.zPos - node.terrainCube.zPos)) + (Mathf.Abs(target.xPos - node.terrainCube.xPos) + Mathf.Abs(target.zPos - node.terrainCube.zPos));
    }

    private List<Node> getNeighbours(Node centerNode, List<List<Node>> grid) {
        List<Node> neighbours = new List<Node>();

        int centerX = centerNode.terrainCube.xPos;
        int centerZ = centerNode.terrainCube.zPos;

        if ((centerX - 1) >= 0) {
            Node neighbourNode = grid[centerZ][centerX - 1];

            if (!neighbourNode.visited) {
                neighbours.Add(neighbourNode);
            }
        }

        if ((centerZ - 1) >= 0) {
            Node neighbourNode = grid[centerZ - 1][centerX];

            if (!neighbourNode.visited) {
                neighbours.Add(neighbourNode);
            }
        }

        if ((centerX + 1) <= terrainData.size - 1) {
            Node neighbourNode = grid[centerZ][centerX + 1];

            if (!neighbourNode.visited) {
                neighbours.Add(neighbourNode);
            }
        }

        if ((centerZ + 1) <= terrainData.size - 1) {
            Node neighbourNode = grid[centerZ + 1][centerX];

            if (!neighbourNode.visited) {
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

        return bestNode;
    }

    private class Node {
        public float weight;
        public bool visited;
        public Node previousNode;
        public Cubes.TerrainCube terrainCube;

        public Node(Cubes.TerrainCube terrainCube, float weight) {
            this.weight = weight;
            this.terrainCube = terrainCube;
            this.visited = false;
            this.previousNode = null;
        }
    }
}