using UnityEngine;

// Savaş sonucunu paket halinde taşımak için bir veri yapısı (struct)
public struct BattleResult
{
    public bool isPlayerWinner;
    public long playerPower;
    public long enemyPower;
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// İki ordu arasında otomatik bir savaş yapar ve sonucu döndürür.
    /// </summary>
    public BattleResult DoBattle(long playerHealth, long playerAttack, long enemyHealth, long enemyAttack)
    {
        // Savaş gücünü hesaplamak için basit bir formül.
        // Bu formülü ileride daha karmaşık ve ilginç hale getirebiliriz.
        long playerPower = playerHealth + (playerAttack * 5); // Saldırı puanını daha değerli yapalım
        long enemyPower = enemyHealth + (enemyAttack * 5);

        Debug.Log($"Savaş Başladı! Oyuncu Gücü: {playerPower} vs Düşman Gücü: {enemyPower}");

        // Sonucu bir pakete doldur
        BattleResult result = new BattleResult
        {
            playerPower = playerPower,
            enemyPower = enemyPower,
            isPlayerWinner = playerPower >= enemyPower // Beraberlik durumunda oyuncu kazanır
        };

        return result;
    }
}