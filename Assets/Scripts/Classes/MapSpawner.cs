/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class MapSpawner
{
    public static IEnumerator SpawnMapCoroutine(GameObject map)
    {
        if (PersistentDataManager.Instance.Map != "Default Map")
        {
            Vector3 spawnPosition = new Vector3(0, 0, 0);
            Quaternion spawnRotation = Quaternion.Euler(0, 0, 0);

            GameObject mapInstance = Object.Instantiate(map, spawnPosition, spawnRotation);
            mapInstance.transform.localScale = new Vector3(1f, 1f, 1f);

            MapManager mapManager = mapInstance.AddComponent<MapManager>();
            Object.DontDestroyOnLoad(mapInstance);

            yield return new WaitUntil(() => MapManager.Instance != null);
        }
        else
        {
            yield return null;
        }
    }
}*/
