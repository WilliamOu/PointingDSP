using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class PlayerSpawner
{
    public static IEnumerator SpawnPlayerCoroutine(GameObject player, GameObject virtualizerPlayer, GameObject roomscalePlayer)
    {
        GameObject playerInstance;

        if (!PersistentDataManager.Instance.IsVR)
        {
            playerInstance = SpawnPCPlayer(player);
        }
        else
        {
            playerInstance = PersistentDataManager.Instance.IsRoomscale ? SpawnRoomscalePlayer(roomscalePlayer) : SpawnVirtualizerPlayer(virtualizerPlayer);
        }

        PlayerManager playerManager = playerInstance.AddComponent<PlayerManager>();
        Object.DontDestroyOnLoad(playerInstance);

        yield return new WaitUntil(() => PlayerManager.Instance != null);
    }

    public static IEnumerator SpawnPlayerCoroutine(GameObject player, GameObject virtualizerPlayer, GameObject roomscalePlayer, string stage)
    {
        yield return SpawnPlayerCoroutine(player, virtualizerPlayer, roomscalePlayer);
        LoadSelectedScene(stage);
    }

    private static GameObject SpawnPCPlayer(GameObject player)
    {
        Vector3 spawnPosition = new Vector3(0, 0, 0);
        Quaternion spawnRotation = Quaternion.Euler(0, 0, 0);

        GameObject playerInstance = Object.Instantiate(player, spawnPosition, spawnRotation);
        if (PersistentDataManager.Instance.Map == "Default Map") 
        { 
            playerInstance.GetComponent<CharacterController>().center = new Vector3(0f, 0.94f, 0f);
            playerInstance.transform.Find("Main Camera").transform.position = new Vector3(0f, 1.59f, 0f); 
            playerInstance.transform.localScale = new Vector3(1f, 2f, 1f);
        }

        return playerInstance;
    }

    private static GameObject SpawnVirtualizerPlayer(GameObject virtualizerPlayer)
    {
        Vector3 spawnPosition = new Vector3(0, 0, 0);
        Quaternion spawnRotation = Quaternion.Euler(0, 0, 0);

        GameObject vrPlayerInstance = Object.Instantiate(virtualizerPlayer, spawnPosition, spawnRotation);
        if (PersistentDataManager.Instance.Map == "Default Map") { vrPlayerInstance.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f); }
        return vrPlayerInstance;
    }

    private static GameObject SpawnRoomscalePlayer(GameObject roomscalePlayer)
    {
        Vector3 spawnPosition = new Vector3(0, 0, 0);
        Quaternion spawnRotation = Quaternion.Euler(0, 0, 0);

        GameObject vrPlayerInstance = Object.Instantiate(roomscalePlayer, spawnPosition, spawnRotation);
        if (PersistentDataManager.Instance.Map == "Default Map") { vrPlayerInstance.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f); }
        return vrPlayerInstance;
    }

    public static void LoadSelectedScene(string stage)
    {
        string selectedStageName = stage;
        selectedStageName = selectedStageName == "Full Study" ? "Training Stage" : selectedStageName;
        SceneManager.LoadScene(selectedStageName);
    }
}
