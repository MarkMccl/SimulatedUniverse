using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolManager
{
    public GameObject prefab;

    public int ActiveObjectsCount { get { return activeObjects.Count; } }
    public int InactiveObjectsCount { get { return inactiveObjects.Count; } }
    public int TotalObjectsCount { get { return ActiveObjectsCount + InactiveObjectsCount; } }

    private List<GameObject> activeObjects = new List<GameObject>();
    private List<GameObject> inactiveObjects = new List<GameObject>();

    public PoolManager(GameObject prefab, int startingSize = 0)
    {
        this.prefab = prefab;
        for (int i = 0; i < startingSize; i++){
            CreateForPool();
        }
    }

    public GameObject CreateOrPool(){
        return CreateOrPool(Vector3.zero);
    }

    public GameObject CreateOrPool(Vector3 position){
        return CreateOrPool(position, Quaternion.identity);
    }

    public GameObject CreateOrPool(Vector3 position, Quaternion rotation){
        GameObject gameObject = null;
        if (inactiveObjects.Count > 0){
            gameObject = inactiveObjects[0];
            inactiveObjects.Remove(gameObject);
        }
        if (gameObject == null){
            gameObject = Create();
        }
        gameObject.SetActive(true);
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;
        return gameObject;
    }

    private GameObject Create(){
        return Create(Vector3.zero);
    }

    private GameObject Create(Vector3 position){
        return Create(position, Quaternion.identity);
    }

    private GameObject Create(Vector3 position, Quaternion rotation){
        GameObject gameObject = GameObject.Instantiate(prefab, position, rotation) as GameObject;
        if (gameObject != null){
            activeObjects.Add(gameObject);
        }
        return gameObject;
    }

    private GameObject CreateForPool(){
        return CreateForPool(Vector3.zero);
    }

    private GameObject CreateForPool(Vector3 position){
        return CreateForPool(position, Quaternion.identity);
    }

    private GameObject CreateForPool(Vector3 position, Quaternion rotation){
        GameObject gameObject = GameObject.Instantiate(prefab, position, rotation) as GameObject;
        if (gameObject != null){
            inactiveObjects.Add(gameObject);
            gameObject.SetActive(false);
        }
        return gameObject;
    }

    public void Deactivate(GameObject gameObject){
        if (activeObjects.Contains(gameObject))
        {
            activeObjects.Remove(gameObject);
            gameObject.SetActive(false);
            inactiveObjects.Add(gameObject);
        }
    }
}