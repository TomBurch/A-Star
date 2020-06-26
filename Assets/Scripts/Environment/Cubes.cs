using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using UnityEngine;

namespace Cubes {

    public class Cubes : MonoBehaviour
    {
        public static Cubes Instance;
        public Transform grassPrefab;

        void Awake()
        {
            Instance = this;
        }
    }

    public class TerrainCube : MonoBehaviour
    {
        public bool isWalkable;
        public bool isEmpty = true;
        public GameObject worldObject;
        int xPos, zPos;

        public TerrainCube(int xPos, int zPos, bool isWalkable, Transform prefab, GameObject parent, string name)
        {
            this.isWalkable = isWalkable;
            this.xPos = xPos;
            this.zPos = zPos;

            Transform newCube = Instantiate(prefab, new Vector3(xPos * 1f, 0f, zPos * 1f), Quaternion.identity, parent.transform);
            newCube.name = name;
            this.worldObject = newCube.gameObject;
        }

        public Vector3 getPos()
        {
            return worldObject.transform.position;
        }

        public void setMaterial(Material newMaterial)
        {
            worldObject.GetComponent<MeshRenderer>().material = newMaterial;
        }
    }

    public class GrassCube : TerrainCube
    {
        public GrassCube(int xPos, int zPos, GameObject parent, string name = "GrassCube") : base(xPos, zPos, true, Cubes.Instance.grassPrefab, parent, name)
        {
        }
    }
}