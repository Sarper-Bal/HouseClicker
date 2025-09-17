using UnityEngine;
using System.Collections.Generic;
using System;

public class UpgradeManager : MonoBehaviour
{
    // --- Singleton Pattern Başlangıcı ---
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
            DontDestroyOnLoad(gameObject);
        }
    }
    // --- Singleton Pattern Sonu ---

    // Buraya Unity Editöründen oluşturduğumuz tüm LevelData dosyalarını sürükleyeceğiz.
    public List<LevelData> levelConfigs;

    public int CurrentLevel { get; private set; }

    // Bu event, seviye atlandığında diğer script'lere haber vermek için (Örn: Görsel güncellemeleri)
    public event Action<LevelData> OnLevelUp;

    private void Start()
    {
        LoadLevel();
    }

    // Dışarıdan çağrılacak ana geliştirme fonksiyonu (UI Butonu tarafından).
    public void AttemptUpgrade()
    {
        // Son seviyede miyiz kontrolü
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
            OnLevelUp?.Invoke(GetCurrentLevelData()); // Seviye atlama event'ini tetikle.
        }
        else
        {
            Debug.Log("Yetersiz altın!");
        }
    }

    // Mevcut seviyenin tüm verilerini kolayca almak için bir yardımcı fonksiyon.
    public LevelData GetCurrentLevelData()
    {
        if (CurrentLevel < levelConfigs.Count)
        {
            return levelConfigs[CurrentLevel];
        }
        return null; // Hata durumu
    }

    private void SaveLevel()
    {
        PlayerPrefs.SetInt("CurrentLevel", CurrentLevel);
    }

    private void LoadLevel()
    {
        CurrentLevel = PlayerPrefs.GetInt("CurrentLevel", 0); // Oyuna seviye 0'dan başla.
        // Oyun başladığında görsellerin vs. doğru yüklenmesi için event'i tetikle.
        OnLevelUp?.Invoke(GetCurrentLevelData());
    }
}