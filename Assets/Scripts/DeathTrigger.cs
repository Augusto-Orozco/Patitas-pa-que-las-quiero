using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string deathScene = "Loose";

    private bool hasTriggered;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryKill(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKill(other);
    }

    private void TryKill(Collider2D other)
    {
        bool isPlayer = other != null &&
            (other.CompareTag(playerTag) || other.GetComponentInParent<PlayerMovement>() != null);

        if (hasTriggered || !isPlayer)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(deathScene))
        {
            Debug.LogError($"La escena de muerte '{deathScene}' no esta incluida en File > Build Settings > Scenes In Build.");
            return;
        }

        hasTriggered = true;
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player != null)
        {
            player.PrepareForDeath();
        }

        SceneAudioController.StopMusicAfterDeath();
        SceneManager.LoadScene(deathScene);
    }
}