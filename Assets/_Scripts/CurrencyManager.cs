using UnityEngine;
using System;
using System.Collections; // Coroutine için gerekli

public class CurrencyManager : MonoBehaviour
{
    // --- Singleton Pattern (Değişiklik yok) ---
    public static CurrencyManager Instance { get; private set; }

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

    public long CurrentGold { get; private set; }
    public event Action<long> OnGoldChanged;

    private void Start()
    {
        LoadGold();
        // --- YENİ EKLENEN KISIM ---
        // Pasif gelir döngüsünü başlat.
        StartCoroutine(PassiveIncomeCoroutine());
        // -------------------------
    }

    // --- YENİ EKLENEN COROUTINE ---
    private IEnumerator PassiveIncomeCoroutine()
    {
        // Sonsuz döngü
        while (true)
        {
            // 1 saniye bekle
            yield return new WaitForSeconds(1f);

            // Gerekli sistemler hazırsa pasif geliri ekle
            if (UpgradeManager.Instance != null)
            {
                LevelData currentLevelData = UpgradeManager.Instance.GetCurrentLevelData();
                if (currentLevelData != null && currentLevelData.goldPerSecond > 0)
                {
                    // AddGold fonksiyonu zaten kaydı ve UI bildirimini yapıyor.
                    AddGold(currentLevelData.goldPerSecond);
                }
            }
        }
    }
    // ----------------------------

    public void AddGold(long amount)
    {
        if (amount < 0) return;
        CurrentGold += amount;
        OnGoldChanged?.Invoke(CurrentGold);
        // Sürekli kaydetmek performansı etkileyebilir, daha sonra optimize edilebilir.
        // Şimdilik test için kalabilir.
        SaveGold();
    }

    public void SpendGold(long amount)
    {
        if (amount < 0) return;
        if (CurrentGold >= amount)
        {
            CurrentGold -= amount;
            OnGoldChanged?.Invoke(CurrentGold);
            SaveGold();
        }
    }

    private void SaveGold()
    {
        PlayerPrefs.SetString("CurrentGold", CurrentGold.ToString());
    }

    private void LoadGold()
    {
        string goldStr = PlayerPrefs.GetString("CurrentGold", "0");
        long.TryParse(goldStr, out long loadedGold);
        CurrentGold = loadedGold;
        OnGoldChanged?.Invoke(CurrentGold);
    }
}