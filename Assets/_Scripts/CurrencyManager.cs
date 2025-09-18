using UnityEngine;
using System;
using System.Collections;

public class CurrencyManager : MonoBehaviour
{
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

    // --- YENİ EKLENEN EVENT ---
    // FloatingTextManager'ın dinleyeceği sinyal.
    public event Action<long> OnPassiveGoldAdded;
    // -------------------------

    private void Start()
    {
        LoadGold();
        StartCoroutine(PassiveIncomeCoroutine());
    }

    private IEnumerator PassiveIncomeCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (UpgradeManager.Instance != null)
            {
                LevelData currentLevelData = UpgradeManager.Instance.GetCurrentLevelData();
                if (currentLevelData != null && currentLevelData.goldPerSecond > 0)
                {
                    long passiveAmount = currentLevelData.goldPerSecond;

                    // --- EVENT'İ TETİKLE ---
                    // Pasif geliri ekleyeceğimizi haber veriyoruz.
                    OnPassiveGoldAdded?.Invoke(passiveAmount);
                    // -------------------------

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