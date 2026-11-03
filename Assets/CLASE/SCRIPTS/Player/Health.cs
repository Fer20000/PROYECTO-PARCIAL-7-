using Fusion;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;


    [Networked]
    public int ActualHealth { get; private set; }


    [Networked]
    public NetworkBool IsDead { get; private set; }


    // ==========================================
    // INICIAR VIDA
    // ==========================================

    public override void Spawned()
    {
        if (!HasStateAuthority)
            return;

        ActualHealth = maxHealth;
        IsDead = false;
    }


    // ==========================================
    // RECIBIR DAÑO
    // ==========================================

    public void TakeDamage(
        int damage,
        PlayerRef attacker)
    {
        if (!HasStateAuthority)
            return;

        if (IsDead)
            return;

        if (damage <= 0)
            return;


        ActualHealth -= damage;


        Debug.Log(
            gameObject.name +
            " recibió " +
            damage +
            " de daño. Vida: " +
            ActualHealth
        );


        if (ActualHealth <= 0)
        {
            Die(attacker);
        }
    }


    // ==========================================
    // MORIR
    // ==========================================

    private void Die(PlayerRef attacker)
    {
        if (!HasStateAuthority)
            return;

        if (IsDead)
            return;


        IsDead = true;
        ActualHealth = 0;


        Debug.Log(
            gameObject.name +
            " murió. Lo mató: " +
            attacker
        );


        GameManager gameManager =
            FindFirstObjectByType<GameManager>();


        if (gameManager != null)
        {
            gameManager.PlayerKilled(attacker);
        }
    }


    // ==========================================
    // CURAR
    // ==========================================

    public void Heal(int amount)
    {
        if (!HasStateAuthority)
            return;

        if (IsDead)
            return;

        if (amount <= 0)
            return;


        ActualHealth += amount;


        if (ActualHealth > maxHealth)
        {
            ActualHealth = maxHealth;
        }
    }


    // ==========================================
    // REINICIAR VIDA
    // ==========================================

    public void ResetHealth()
    {
        if (!HasStateAuthority)
            return;


        ActualHealth = maxHealth;
        IsDead = false;
    }
}