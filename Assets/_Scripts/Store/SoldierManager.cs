using UnityEngine;
using System;

public class SoldierManager : MonoBehaviour
{
    public static SoldierManager Instance { get; private set; }

    public int TotalSoldiers { get; private set; }
    public long TotalHealth { get; private set; }
    public long TotalAttack { get; private set; }

    // Asker sayısı değiştiğinde UI gibi diğer sistemleri bilgilendirmek için bir event.
    public event Action OnSoldierDataChanged;

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
            LoadSoldiers(); // Oyun açıldığında verileri yükle
        }
    }

    /// <summary>
    /// Orduya yeni askerler ve istatistiklerini ekler.
    /// </summary>
    /// <param name="amount">Eklenecek asker sayısı.</param>
    /// <param name="healthPerSoldier">Her bir askerin sağladığı sağlık.</param>
    /// <param name="attackPerSoldier">Her bir askerin sağladığı saldırı.</param>
    public void AddSoldiers(int amount, int healthPerSoldier, int attackPerSoldier)
    {
        if (amount <= 0) return;

        TotalSoldiers += amount;
        TotalHealth += (long)amount * healthPerSoldier;
        TotalAttack += (long)amount * attackPerSoldier;

        Debug.Log($"{amount} asker orduya katıldı. Yeni Toplam Asker: {TotalSoldiers}, Sağlık: {TotalHealth}, Saldırı: {TotalAttack}");

        SaveSoldiers();
        OnSoldierDataChanged?.Invoke(); // Değişikliği dinleyenlere haber ver.
    }

    private void SaveSoldiers()
    {
        PlayerPrefs.SetInt("TotalSoldiers", TotalSoldiers);
        PlayerPrefs.SetString("TotalHealth", TotalHealth.ToString());
        PlayerPrefs.SetString("TotalAttack", TotalAttack.ToString());
    }

    private void LoadSoldiers()
    {
        TotalSoldiers = PlayerPrefs.GetInt("TotalSoldiers", 0);
        long.TryParse(PlayerPrefs.GetString("TotalHealth", "0"), out long loadedHealth);
        long.TryParse(PlayerPrefs.GetString("TotalAttack", "0"), out long loadedAttack);
        TotalHealth = loadedHealth;
        TotalAttack = loadedAttack;

        Debug.Log($"Asker verileri yüklendi. Toplam Asker: {TotalSoldiers}, Sağlık: {TotalHealth}, Saldırı: {TotalAttack}");
    }
}