using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    private static bool battleDefeat;

    [SerializeField] private GameObject restartButton;
    [SerializeField] private string runningScene = "Running";
    [SerializeField] private string preparationScene = "Preparacion";
    [SerializeField] private string startScene = "Inicio";
    [SerializeField] private bool useSpawnPoint = true;
    [SerializeField] private string spawnPointTag = "SpawnPoint";

    private bool sceneLoading;

    private void Awake()
    {
        if (battleDefeat && restartButton != null)
        {
            restartButton.SetActive(false);
        }

        battleDefeat = false;
    }

    public static void MarkBattleDefeat()
    {
        battleDefeat = true;
    }

    public void RestartRunning()
    {
        SceneAudioController.AllowMusicForRunningRestart();
        LoadScene(runningScene);
    }

    public void GoToPreparation()
    {
        LoadScene(preparationScene);
    }

    public void GoToStart()
    {
        LoadScene(startScene);
    }

    public void LoadScene(string sceneName)
    {
        if (sceneLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("No hay ninguna escena seleccionada para cargar.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"La escena '{sceneName}' no esta incluida en File > Build Settings > Scenes In Build.");
            return;
        }

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
        sceneLoading = false;

        PlayerSceneUtility.ResetPlayer(spawnPointTag);
    }
}