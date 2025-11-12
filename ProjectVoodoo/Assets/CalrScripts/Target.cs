using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 50f;
    public GameObject deathEffect;
    public GameObject hitEffect;

    public void TakeDamage(float amount)
    {
        GameObject hitEffectGO = Instantiate(hitEffect, transform.position, Quaternion.Euler(-90f, 0f, 0f));
        Destroy(hitEffectGO, 10f);

        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        GameObject deathEffectGO = Instantiate(deathEffect, transform.position, Quaternion.Euler(-90f, 0f, 0f));
        Destroy(deathEffectGO, 10f);
        Destroy(gameObject);
    }
}
