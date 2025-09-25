using UnityEngine;
using System.Collections.Generic;
using System;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public List<LevelData> levelConfigs;
    public int CurrentLevel { get; private set; }
    public event Action<LevelData> OnLevelUp;

    // --- GERİ GETİRİLEN SİSTEM BAŞLANGICI ---
    // Seviyeye bağlı pasif gelir için zamanlayıcı
    private float passiveIncomeTimer = 0f;
    // --- GERİ GETİRİLEN SİSTEM SONU ---

    private void Start()
    {
        LoadLevel();
        OnLevelUp?.Invoke(GetCurrentLevelData());
    }

    // --- GERİ GETİRİLEN SİSTEM BAŞLANGICI ---
    // Bu fonksiyon, seviyeye bağlı pasif geliri (goldPerSecond) hesaplar ve ekler.
    private void Update()
    {
        // Zamanlayıcıyı her saniye artır.
        passiveIncomeTimer += Time.deltaTime;

        // 1 saniye geçtiyse geliri ekle ve zamanlayıcıyı sıfırla.
        if (passiveIncomeTimer >= 1f)
        {
            LevelData currentLevelData = GetCurrentLevelData();
            if (currentLevelData != null && currentLevelData.goldPerSecond > 0)
            {
                // CurrencyManager'daki pasif altın ekleme fonksiyonunu kullanıyoruz.
                // Bu fonksiyon, hem altını ekler hem de FloatingTextManager'ın duyması gereken event'i tetikler.
                CurrencyManager.Instance.AddPassiveGold(currentLevelData.goldPerSecond);
            }

            // Zamanlayıcıyı sıfırlarken artan kısmı koru (örn: 1.1s olduysa 0.1s olarak başla).
            passiveIncomeTimer -= 1f;
        }
    }
    // --- GERİ GETİRİLEN SİSTEM SONU ---

    public void AttemptUpgrade()
    {
        if (CurrentLevel >= levelConfigs.Count - 1)
        {
            Debug.Log("Maksimum seviyeye ulaşıldı!");
            return;
        }

        LevelData nextLevelData = levelConfigs[CurrentLevel + 1];
        if (CurrencyManager.Instance.CurrentGold >= nextLevelData.upgradeCost)
        {
            CurrencyManager.Instance.SpendGold(nextLevelData.upgradeCost);
            CurrentLevel++;
            SaveLevel();

            Debug.Log("Seviye atlandı! Yeni seviye: " + CurrentLevel);
            OnLevelUp?.Invoke(GetCurrentLevelData());
        }
        else
        {
            Debug.Log("Yetersiz altın!");
        }
    }

    public LevelData GetCurrentLevelData()
    {
        if (CurrentLevel < levelConfigs.Count)
        {
            return levelConfigs[CurrentLevel];
        }
        return null;
    }

    private void SaveLevel()
    {
        PlayerPrefs.SetInt("CurrentLevel", CurrentLevel);
        PlayerPrefs.Save();
    }

    private void LoadLevel()
    {
        CurrentLevel = PlayerPrefs.GetInt("CurrentLevel", 0);
    }
}