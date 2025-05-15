using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healthcontroller : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health of the entity.")]
    public float maxHealth = 100f;

    [Tooltip("Initial health; if zero, will be set to maxHealth on Awake.")]
    public float initialHealth = 0f;

    private float currentHealth;

    public AIbase aiBase;

    private void Awake()
    {
        currentHealth = (initialHealth > 0f) ? initialHealth : maxHealth;
        aiBase = GetComponent<AIbase>();
    }

    /// <summary>
    /// Applies damage to this entity, reducing current health.
    /// </summary>
    /// <param name="amount">Amount of damage to apply (must be positive).</param>
    public void ApplyDamage(float amount)
    {
        if (amount <= 0f || IsDead()) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (IsDead())
        {
            OnDeath();
        }
    }

    /// <summary>
    /// Heals this entity by the specified amount.
    /// </summary>
    /// <param name="amount">Amount to heal (must be positive).</param>
    public void Heal(float amount)
    {
        if (amount <= 0f || IsDead()) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    /// <summary>
    /// Returns true if the entity's health has reached zero.
    /// </summary>
    public bool IsDead()
    {
        return currentHealth <= 0f;
    }

    /// <summary>
    /// Event called when health reaches zero. Override in subclass for custom behavior.
    /// </summary>
    protected virtual void OnDeath()
    {
        if(aiBase is PlayerState)
        {
            aiBase.AddState(new Down());
        }
        else
        {
            aiBase.AddState(new Die());
        }
    }

    /// <summary>
    /// Gets the current health value.
    /// </summary>
    public float GetHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Sets current health explicitly (clamped between 0 and maxHealth).
    /// </summary>
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
    }
}

