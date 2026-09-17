using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QTEBar : MonoBehaviour
{
    [Header("UI")]
    public Image barFill;
    public GameObject panelCompletado;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip musicaFondo;
    public AudioClip musicaVictoria;

    [Header("QTE")]
    public float amountPerPress = 0.08f;
    public float decreaseSpeed = 0.1f;

    [Header("Transición")]
    public float tiempoAntesDeCambiar = 3f;
    public string escenaDerrota = "Loose";

    private float progress = 0f;
    private bool completed = false;
    private bool hasDecreased = false;

    void Start()
    {
        progress = 0.7f;
        hasDecreased = false;
        barFill.fillAmount = progress;

        panelCompletado.SetActive(false);

        // Música inicial
        audioSource.clip = musicaFondo;
        audioSource.loop = true;
        audioSource.Play();
    }

    void Update()
    {
        if (completed)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            progress += amountPerPress;
        }

        progress -= decreaseSpeed * Time.deltaTime;

        if (progress < 1f)
        {
            hasDecreased = true;
        }

        progress = Mathf.Clamp01(progress);
        barFill.fillAmount = progress;

        if (progress <= 0f)
        {
            LoseQTE();
            return;
        }

        if (hasDecreased && progress >= 1f)
        {
            CompleteQTE();
        }
    }

    void CompleteQTE()
    {
        completed = true;

        // Detener música de fondo
        audioSource.Stop();

        // Cambiar a música de victoria
        audioSource.clip = musicaVictoria;
        audioSource.loop = false;
        audioSource.Play();

        // Mostrar panel de victoria
        panelCompletado.SetActive(true);
        StartCoroutine(CambiarDespuesDeEsperar());

        Debug.Log("¡QTE completado!");
    }

    private void LoseQTE()
    {
        completed = true;
        audioSource.Stop();
        SceneAudioController.StopMusicAfterDeath();

        if (!Application.CanStreamedLevelBeLoaded(escenaDerrota))
        {
            Debug.LogError($"La escena de derrota '{escenaDerrota}' no esta incluida en File > Build Settings > Scenes In Build.");
            return;
        }

        SceneNavigation.MarkBattleDefeat();
        SceneManager.LoadScene(escenaDerrota);
    }

    private System.Collections.IEnumerator CambiarDespuesDeEsperar()
    {
        yield return new WaitForSeconds(tiempoAntesDeCambiar);

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("No hay otra escena despues de Battle.");
            yield break;
        }

        string nextScenePath = SceneUtility.GetScenePathByBuildIndex(nextSceneIndex);
        string nextSceneName = System.IO.Path.GetFileNameWithoutExtension(nextScenePath);

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError($"La escena '{nextSceneName}' no esta incluida en File > Build Settings > Scenes In Build.");
            yield break;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}