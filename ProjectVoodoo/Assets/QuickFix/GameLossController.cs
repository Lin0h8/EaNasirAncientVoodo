using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLossController : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [SerializeField] private float currentHealth;
    [SerializeField] private float damagePerCollision = 10f;
    [SerializeField] private float invincibilityDuration = 1f;

    [Header("UI References")]
    [SerializeField] private Image healthBarFillImage;

    [Header("Scene Settings")]
    [SerializeField] private string gameOverSceneName = "GameOver";

    private float lastDamageTime;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        lastDamageTime = -invincibilityDuration;
        UpdateHealthBar();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Enemy") ||
            hit.gameObject.GetComponent<EnemyAIFSM>() != null ||
            hit.gameObject.GetComponent<EnemyOneBehaviour>() != null)
        {
            if (Time.time - lastDamageTime >= invincibilityDuration)
            {
                TakeDamage(damagePerCollision);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.GetComponent<EnemyAIFSM>() != null ||
            collision.gameObject.GetComponent<EnemyOneBehaviour>() != null)
        {
            if (Time.time - lastDamageTime >= invincibilityDuration)
            {
                TakeDamage(damagePerCollision);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") ||
            other.GetComponent<EnemyAIFSM>() != null ||
            other.GetComponent<EnemyOneBehaviour>() != null)
        {
            if (Time.time - lastDamageTime >= invincibilityDuration)
            {
                TakeDamage(damagePerCollision);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        lastDamageTime = Time.time;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFillImage != null)
        {
            healthBarFillImage.fillAmount = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        LoadGameOverScene();
    }

    private void LoadGameOverScene()
    {
        SceneManager.LoadScene(gameOverSceneName);
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthBar();
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}