using UnityEngine;
using TMPro;
using System; // Action kullanabilmek için

public class BattleSoldier : MonoBehaviour
{
    [SerializeField] private SpriteRenderer soldierSpriteRenderer;
    [SerializeField] private TextMeshProUGUI healthText;

    private SoldierData soldierData;
    private long currentHealth;

    // Bir asker öldüğünde BattleManager'a haber vermek için kullanılacak olay (event)
    public event Action<BattleSoldier> OnSoldierDied;

    /// <summary>
    /// Savaşçı asker objesini gerekli verilerle kurar.
    /// </summary>
    public void Setup(SoldierData data)
    {
        soldierData = data;
        currentHealth = soldierData.health;

        soldierSpriteRenderer.sprite = soldierData.shopIcon;
        UpdateHealthText();
    }

    /// <summary>
    /// Bu askerin saldırı gücünü döndürür.
    /// </summary>
    public long GetAttackPower()
    {
        return soldierData.attack;
    }

    /// <summary>
    /// Askerin hasar almasını sağlar.
    /// </summary>
    public void TakeDamage(long damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            UpdateHealthText();
            Die();
        }
        else
        {
            UpdateHealthText();
        }
    }

    /// <summary>
    /// Can metnini günceller.
    /// </summary>
    private void UpdateHealthText()
    {
        healthText.text = currentHealth.ToString();
    }

    /// <summary>
    /// Askerin ölme işlemini gerçekleştirir.
    /// </summary>
    private void Die()
    {
        // Ölüm olayını tetikle ve bu objeyi yok et
        OnSoldierDied?.Invoke(this);
        Destroy(gameObject);
    }
}