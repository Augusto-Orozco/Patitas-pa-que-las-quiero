using UnityEngine;

public static class PlayerSceneUtility
{
    public static PlayerMovement FindPlayer()
    {
        PlayerMovement player = Object.FindObjectOfType<PlayerMovement>();

        if (player != null)
        {
            return player;
        }

        PlayerMovement[] players = Resources.FindObjectsOfTypeAll<PlayerMovement>();

        foreach (PlayerMovement candidate in players)
        {
            if (candidate.gameObject.scene.IsValid() && candidate.gameObject.scene.isLoaded)
            {
                return candidate;
            }
        }

        return null;
    }

    public static void ResetPlayer(string spawnPointTag)
    {
        PlayerMovement player = FindPlayer();

        if (player == null)
        {
            return;
        }

        player.transform.root.gameObject.SetActive(true);

        GameObject spawnPoint = FindSpawnPoint(spawnPointTag);

        if (spawnPoint != null)
        {
            player.transform.SetPositionAndRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
        }

        player.ResetAfterSceneLoad();
        Physics2D.SyncTransforms();
    }

    private static GameObject FindSpawnPoint(string spawnPointTag)
    {
        if (string.IsNullOrWhiteSpace(spawnPointTag))
        {
            return null;
        }

        try
        {
            return GameObject.FindGameObjectWithTag(spawnPointTag);
        }
        catch (UnityException)
        {
            return null;
        }
    }
}