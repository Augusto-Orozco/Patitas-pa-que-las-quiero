using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool useSpawnPoint = true;
    [SerializeField] private string spawnPointTag = "SpawnPoint";

    private bool sceneLoading;

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isPlayer = other != null &&
            (other.CompareTag(playerTag) || other.GetComponentInParent<PlayerMovement>() != null);

        if (!isPlayer)
        {
            return;
        }

        LoadSelectedScene();
    }

    public void LoadSelectedScene()
    {
        if (sceneLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogWarning("No hay ninguna escena seleccionada en SceneManager.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError($"La escena '{sceneToLoad}' no esta incluida en File > Build Settings > Scenes In Build.");
            return;
        }

        LoadScene(sceneToLoad);
    }

    private void LoadScene(string sceneName)
    {
        sceneLoading = true;
        if (useSpawnPoint)
        {
            SceneManager.sceneLoaded += MovePlayerToSpawnPoint;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void MovePlayerToSpawnPoint(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= MovePlayerToSpawnPoint;

        PlayerSceneUtility.ResetPlayer(spawnPointTag);
    }

    public void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("No hay otra escena despues de la escena actual.");
            return;
        }

        string nextSceneName = SceneUtility.GetScenePathByBuildIndex(nextSceneIndex);
        LoadScene(System.IO.Path.GetFileNameWithoutExtension(nextSceneName));
    }
}
