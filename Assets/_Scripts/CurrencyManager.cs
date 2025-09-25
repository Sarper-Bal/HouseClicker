using UnityEngine;
using System;
using System.Collections;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    // Coroutine'i durdurma ile ilgili tüm kodlar kaldırıldı.
    // private Coroutine passiveIncomeCoroutine;

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
    public event Action<long> OnPassiveGoldAdded;

    private void Start()
    {
        LoadGold();
        // Pasif gelir görevini sadece bir kez, oyunun en başında başlatıyoruz.
        // Artık bir değişkene atamaya veya durdurmaya gerek yok.
        StartCoroutine(PassiveIncomeCoroutine());
    }

    // StopPassiveIncome() fonksiyonu buradan kaldırıldı.

    private IEnumerator PassiveIncomeCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            // Oyuncunun hangi sahnede olduğunu kontrol etmemize gerek yok.
            // Bu coroutine her zaman çalışacak ve altın ekleyecek.
            if (UpgradeManager.Instance != null)
            {
                LevelData currentLevelData = UpgradeManager.Instance.GetCurrentLevelData();
                if (currentLevelData != null && currentLevelData.goldPerSecond > 0)
                {
                    long passiveAmount = currentLevelData.goldPerSecond;
                    // Sinyali gönder. Eğer dinleyen (FloatingTextManager) varsa çalışır, yoksa görmezden gelinir.
                    OnPassiveGoldAdded?.Invoke(passiveAmount);
                    AddGold(passiveAmount);
                }
            }
        }
    }

    public void AddGold(long amount)
    {
        if (amount < 0) return;
        CurrentGold += amount;
        OnGoldChanged?.Invoke(CurrentGold);
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