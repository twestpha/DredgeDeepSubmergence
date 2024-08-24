using UnityEngine;
using System.Collections.Generic;
using Winch.Core;

public static class PooledGameObjectManager {
    private static Dictionary<string, Queue<GameObject>> poolLookup = new Dictionary<string, Queue<GameObject>>();

    public static bool HasPoolForIdentifier(string poolIdentifier){
        return poolLookup.ContainsKey(poolIdentifier);
    }

    public static void SetupPool(string poolIdentifier, int poolSize, GameObject gameObject, GameObject parent){
        if(!HasPoolForIdentifier(poolIdentifier)){
            Queue<GameObject> poolQueue = new Queue<GameObject>();

            for(int i = 0; i < poolSize; ++i){
                GameObject gameObjectInstance = GameObject.Instantiate(gameObject);
                gameObjectInstance.SetActive(false);

                poolQueue.Enqueue(gameObjectInstance);
                gameObjectInstance.transform.parent = parent.transform;
                gameObjectInstance.name = poolIdentifier + " (" + i + ")";
            }

            poolLookup.Add(poolIdentifier, poolQueue);
        } else {
            WinchCore.Log.Warn("The pool identifier '" + poolIdentifier + "' already has a queue setup.");
        }
    }

    public static GameObject GetInstanceFromPool(string poolIdentifier){
        if(HasPoolForIdentifier(poolIdentifier)){
            Queue<GameObject> poolQueue = poolLookup[poolIdentifier];

            if(poolQueue.Count > 0){
                GameObject instance = poolLookup[poolIdentifier].Dequeue();
                instance.SetActive(true);

                return instance;
            } else {
                WinchCore.Log.Error("The GameObject pool with identifier '" + poolIdentifier + "' ran out of instances. Consider bumping the maximum it was set up with.");
                return null;
            }
        } else {
            WinchCore.Log.Warn("A pool with the identifier '" + poolIdentifier + "' doesn't exist.");
            return null;
        }
    }

    public static void FreeInstanceToPool(string poolIdentifier, GameObject instance){
        if(HasPoolForIdentifier(poolIdentifier)){
            instance.SetActive(false);
            poolLookup[poolIdentifier].Enqueue(instance);
        } else {
            WinchCore.Log.Warn("A pool with the identifier '" + poolIdentifier + "' doesn't exist.");
        }
    }
}
