using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PersistentObject : MonoBehaviour
{
    private static readonly Dictionary<string, GameObject> persistentRoots =
        new Dictionary<string, GameObject>();
    private static PlayerMovement persistentPlayer;
    private static Vector3 initialPlayerPosition;
    private static Quaternion initialPlayerRotation;
    private static bool hasInitialPlayerPosition;

    private void Awake()
    {
        GameObject root = transform.root.gameObject;
        string key = root.name;

        if (persistentRoots.TryGetValue(key, out GameObject existingRoot) &&
            existingRoot != null && existingRoot != root)
        {
            Destroy(root);
            return;
        }

        persistentRoots[key] = root;
        DontDestroyOnLoad(root);

        PlayerMovement player = root.GetComponentInChildren<PlayerMovement>(true);

        if (player != null)
        {
            persistentPlayer = player;

            if (!hasInitialPlayerPosition)
            {
                initialPlayerPosition = player.transform.position;
                initialPlayerRotation = player.transform.rotation;
                hasInitialPlayerPosition = true;
            }
        }

        SceneManager.sceneLoaded += ResetPlayerAfterSceneLoad;

        Debug.Log($"Objeto persistente: {root.name} desde la escena {gameObject.scene.name}.");
    }

    public static void DestroyPersistentRoot(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        string key = root.name;

        if (persistentRoots.TryGetValue(key, out GameObject existingRoot) && existingRoot == root)
        {
            persistentRoots.Remove(key);
        }

        if (persistentPlayer != null && persistentPlayer.transform.root.gameObject == root)
        {
            persistentPlayer = null;
        }

        root.SetActive(false);
        Destroy(root);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= ResetPlayerAfterSceneLoad;

        GameObject root = transform.root.gameObject;
        string key = root.name;

        if (persistentRoots.TryGetValue(key, out GameObject existingRoot) && existingRoot == root)
        {
            persistentRoots.Remove(key);
        }
    }

    private void ResetPlayerAfterSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Loose")
        {
            return;
        }

        PlayerMovement player = persistentPlayer != null ? persistentPlayer : PlayerSceneUtility.FindPlayer();

        if (player == null)
        {
            return;
        }

        player.transform.root.gameObject.SetActive(true);
        player.ConnectHealthBarFromScene();

        if (scene.name == "Inicio")
        {
            player.transform.SetPositionAndRotation(initialPlayerPosition, initialPlayerRotation);
            player.ResetAfterSceneLoad();
            Physics2D.SyncTransforms();
            return;
        }

        GameObject spawnPoint = null;

        try
        {
            spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        }
        catch (UnityException)
        {
            spawnPoint = null;
        }

        if (spawnPoint != null)
        {
            player.transform.SetPositionAndRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
        }

        player.ResetAfterSceneLoad();
        Physics2D.SyncTransforms();
    }
}