using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 2f;
    [SerializeField] private float fastFallSpeed = 4f;
    [SerializeField] private float landingDuration = 0.25f;
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Image healthBar;
    [SerializeField] private string deathScene = "Loose";
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject shieldPanel;
    [SerializeField] private TMP_Text shieldTimerText;

    private Rigidbody2D body;
    private Collider2D playerCollider;
    private float shieldRemaining;
    private float horizontalInput;
    private bool jumpRequested;
    private int jumpsUsed;
    private bool isGrounded;
    private bool hasBeenAirborne;
    private bool isLanding;
    private bool isDoubleJumping;
    private bool isDead;
    private int currentHealth;
    private float landingTimer;
    private readonly HashSet<Collider2D> groundContacts = new HashSet<Collider2D>();

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
        UpdateHealthBar();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (shieldPanel == null || shieldTimerText == null)
        {
            FindShieldPanel();
        }

        SetShieldPanelVisible(false);
    }

    private void Update()
    {
        if (isDead)
        {
            horizontalInput = 0f;
            UpdateAnimation();
            return;
        }

        shieldRemaining = Mathf.Max(0f, shieldRemaining - Time.deltaTime);

        if (shieldPanel == null || shieldTimerText == null)
        {
            FindShieldPanel();
        }

        UpdateShieldPanel();

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
        }

        if (spriteRenderer != null && Mathf.Abs(horizontalInput) > 0.01f)
        {
            spriteRenderer.flipX = horizontalInput < 0f;
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

        bool wasGrounded = isGrounded;
        isGrounded = CheckGrounded();

        if (!wasGrounded && isGrounded && hasBeenAirborne && body.velocity.y <= 0f)
        {
            StartLandingAnimation();
            hasBeenAirborne = false;
            jumpsUsed = 0;
            isDoubleJumping = false;
        }

        if (isGrounded && !hasBeenAirborne)
        {
            jumpsUsed = 0;
        }

        body.velocity = new Vector2(horizontalInput * moveSpeed, body.velocity.y);

        if (jumpRequested && (isGrounded || jumpsUsed < 2))
        {
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            groundContacts.Clear();
            isGrounded = false;
            hasBeenAirborne = true;
            jumpsUsed++;
            isDoubleJumping = jumpsUsed == 2;
            GameplayLogger.Instance?.LogJump();
        }

        if (!isGrounded && Input.GetKey(KeyCode.S))
        {
            body.velocity = new Vector2(body.velocity.x, -fastFallSpeed);
        }

        jumpRequested = false;

        if (isLanding)
        {
            landingTimer -= Time.fixedDeltaTime;

            if (landingTimer <= 0f)
            {
                isLanding = false;
            }
        }

        UpdateAnimation();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(groundTag))
        {
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                groundContacts.Add(collision.collider);
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionStay2D(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        groundContacts.Remove(collision.collider);
        isGrounded = groundContacts.Count > 0;
    }

    private bool CheckGrounded()
    {
        if (playerCollider == null)
        {
            return false;
        }

        Bounds bounds = playerCollider.bounds;
        Vector2 checkCenter = new Vector2(bounds.center.x, bounds.min.y - 0.02f);
        Vector2 checkSize = new Vector2(bounds.size.x * 0.8f, 0.08f);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(checkCenter, checkSize, 0f);

        foreach (Collider2D collider in colliders)
        {
            if (collider != playerCollider && collider.CompareTag(groundTag))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D collider = playerCollider != null ? playerCollider : GetComponent<Collider2D>();

        if (collider == null)
        {
            return;
        }

        Bounds bounds = collider.bounds;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector2(bounds.center.x, bounds.min.y - 0.02f),
            new Vector2(bounds.size.x * 0.8f, 0.08f));
    }

    private void UpdateAnimation()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsLanding", isLanding);
        animator.SetBool("IsDoubleJumping", isDoubleJumping);
    }

    private void StartLandingAnimation()
    {
        isLanding = true;
        landingTimer = landingDuration;
    }

    public void ResetAfterSceneLoad()
    {
        isDead = false;
        currentHealth = maxHealth;
        horizontalInput = 0f;
        jumpRequested = false;
        isGrounded = false;
        hasBeenAirborne = false;
        isLanding = false;
        landingTimer = 0f;
        jumpsUsed = 0;
        isDoubleJumping = false;
        shieldRemaining = 0f;
        SetShieldPanelVisible(false);
        groundContacts.Clear();

        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.simulated = true;
        body.position = transform.position;
        SetVisualsActive(true);
        UpdateHealthBar();
        UpdateAnimation();
    }

    public void ConnectHealthBarFromScene()
    {
        healthBar = FindHealthBar();
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        if (shieldRemaining > 0f)
        {
            GameplayLogger.Instance?.LogBlockedDamage();
            return;
        }

        GameplayLogger.Instance?.LogDamage();

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthBar();

        if (currentHealth > 0)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(deathScene))
        {
            Debug.LogError($"La escena de muerte '{deathScene}' no esta incluida en File > Build Settings > Scenes In Build.");
            return;
        }

        PrepareForDeath();
        SceneAudioController.StopMusicAfterDeath();
        SceneManager.LoadScene(deathScene);
    }

    public void ActivateShield(float duration)
    {
        shieldRemaining = Mathf.Max(shieldRemaining, duration);
        SetShieldPanelVisible(true);
        UpdateShieldPanel();
    }

    private void FindShieldPanel()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);

        foreach (Canvas canvas in canvases)
        {
            Transform[] children = canvas.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (child.name != "Bendicion")
                {
                    continue;
                }

                TMP_Text text = child.GetComponentInChildren<TMP_Text>(true);

                if (text == null)
                {
                    continue;
                }

                shieldPanel = child.gameObject;
                shieldTimerText = text;
                return;
            }
        }

    }

    private void UpdateShieldPanel()
    {
        if (shieldRemaining <= 0f)
        {
            SetShieldPanelVisible(false);
            return;
        }

        SetShieldPanelVisible(true);

        if (shieldTimerText != null)
        {
            shieldTimerText.text = $"{shieldRemaining:0.0}s";
        }
    }

    private void SetShieldPanelVisible(bool isVisible)
    {
        if (shieldPanel != null)
        {
            shieldPanel.SetActive(isVisible);
        }
    }

    public void PrepareForDeath()
    {
        isDead = true;
        horizontalInput = 0f;
        jumpRequested = false;
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.simulated = false;
        SetVisualsActive(false);
    }

    private void SetVisualsActive(bool isActive)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = isActive;
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    private Image FindHealthBar()
    {
        Image fallback = null;
        Image panelFill = null;
        Image[] images = FindObjectsOfType<Image>(true);

        foreach (Image image in images)
        {
            string objectName = image.gameObject.name;

            if (image.type == Image.Type.Filled &&
                IsNamed(objectName, "vida") && IsHealthBarChild(image.transform))
            {
                return image;
            }

            if (image.type == Image.Type.Filled && IsHealthBarChild(image.transform))
            {
                panelFill = image;
            }

            if (fallback == null && image.type == Image.Type.Filled)
            {
                fallback = image;
            }
        }

        return panelFill != null ? panelFill : fallback;
    }

    private bool IsHealthBarChild(Transform imageTransform)
    {
        Transform current = imageTransform;

        while (current != null)
        {
            string objectName = current.gameObject.name;

            if (IsNamed(objectName, "HealthBar") || IsNamed(objectName, "BarraVida") ||
                IsNamed(objectName, "FondoVida"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private bool IsNamed(string objectName, string expectedName)
    {
        return string.Equals(objectName, expectedName, System.StringComparison.OrdinalIgnoreCase);
    }
}
