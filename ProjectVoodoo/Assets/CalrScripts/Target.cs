using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 50f;
    public GameObject deathEffect;
    public ParticleSystem hitEffect;

    public void TakeDamage(float amount)
    {
        hitEffect.Play();

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
