using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneAudioController
{
    private const string MusicSourceName = "ML";
    private static bool musicAllowed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneLoading()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartInitialMusic()
    {
        UpdateMusic(SceneManager.GetActiveScene().name);
    }

    public static void StopMusicAfterDeath()
    {
        musicAllowed = false;
        StopMusicSources();
    }

    public static void AllowMusicForRunningRestart()
    {
        musicAllowed = true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMusic(scene.name);
    }

    private static void UpdateMusic(string sceneName)
    {
        if (sceneName == "Inicio")
        {
            musicAllowed = true;
            PlayMusicSource();
            return;
        }

        if (sceneName == "Running" && musicAllowed)
        {
            PlayMusicSource();
            return;
        }

        StopMusicSources();
    }

    private static void PlayMusicSource()
    {
        AudioSource[] sources = Resources.FindObjectsOfTypeAll<AudioSource>();
        AudioSource selectedSource = null;

        foreach (AudioSource source in sources)
        {
            if (source == null || source.clip == null || source.name != MusicSourceName ||
                !IsLoadedSceneOrPersistent(source))
            {
                continue;
            }

            selectedSource = source;
            break;
        }

        StopMusicSources(selectedSource);

        if (selectedSource == null)
        {
            Debug.LogWarning("No se encontro el AudioSource 'ML'.");
            return;
        }

        ActivateHierarchy(selectedSource.transform);
        selectedSource.enabled = true;
        selectedSource.loop = true;
        selectedSource.mute = false;

        if (selectedSource.volume <= 0f)
        {
            selectedSource.volume = 1f;
        }

        if (!selectedSource.isPlaying)
        {
            selectedSource.Play();
        }
    }

    private static void ActivateHierarchy(Transform target)
    {
        Transform current = target;

        while (current != null)
        {
            current.gameObject.SetActive(true);
            current = current.parent;
        }
    }

    private static bool IsLoadedSceneOrPersistent(AudioSource source)
    {
        Scene sourceScene = source.gameObject.scene;

        return sourceScene.IsValid() &&
            (sourceScene.isLoaded || sourceScene.name == "DontDestroyOnLoad");
    }

    private static void StopMusicSources(AudioSource sourceToKeep = null)
    {
        AudioSource[] sources = Resources.FindObjectsOfTypeAll<AudioSource>();

        foreach (AudioSource source in sources)
        {
            if (source != null && source.name == MusicSourceName && source != sourceToKeep)
            {
                source.Stop();
            }
        }
    }
}