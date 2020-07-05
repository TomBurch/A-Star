using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using UnityEngine;

namespace Cubes {

    public class Cubes : MonoBehaviour {
        public static Cubes Instance;
        public Transform grassPrefab;
        public Transform riverPrefab;

        void Awake() {
            Instance = this;
        }
    }

    public class TerrainCube : MonoBehaviour {
        public bool isWalkable;
        public GameObject containedObject;
        public GameObject worldObject;
        public int xPos, zPos;

        public TerrainCube(int xPos, int zPos, bool isWalkable, Transform prefab, GameObject parent, string name) {
            this.containedObject = null;
            this.isWalkable = isWalkable;
            this.xPos = xPos;
            this.zPos = zPos;

            Transform newCube = Instantiate(prefab, new Vector3(xPos * 1f, 0f, zPos * 1f), Quaternion.identity, parent.transform);
            newCube.name = name;
            this.worldObject = newCube.gameObject;
        }

        public Vector3 getPos() {
            return worldObject.transform.position;
        }

        public void setMaterial(Material newMaterial) {
            worldObject.GetComponent<MeshRenderer>().material = newMaterial;
        }

        public IEnumerator setMaterialAfterDelay(Material newMaterial, float delay) {
            yield return new WaitForSeconds(delay);
            worldObject.GetComponent<MeshRenderer>().material = newMaterial;
        }

        public void clearCube() {
            if (containedObject != null) {
                Destroy(containedObject);
                containedObject = null;
                isWalkable = true;
            }
        }

        public void destroyCube() {
            clearCube();
            Destroy(worldObject);
        }
    }

    public class GrassCube : TerrainCube {
        public GrassCube(int xPos, int zPos, GameObject parent, string name = "GrassCube") : base(xPos, zPos, true, Cubes.Instance.grassPrefab, parent, name) { }
    }

    public class RiverCube : TerrainCube {
        public RiverCube(int xPos, int zPos, GameObject parent, string name = "RiverCube") : base(xPos, zPos, true, Cubes.Instance.riverPrefab, parent, name) { }
    }
}