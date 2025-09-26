using UnityEngine;
using TMPro;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public long CurrentGold { get; private set; }

    // Para miktarı değiştiğinde UI'ı güncellemek için bir event
    public event Action<long> OnGoldChanged;
    public event Action<long> OnPassiveGoldAdded;


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

    private void Start()
    {
        LoadGold();
    }

    /// <summary>
    /// Oyuncunun altınını artırır.
    /// </summary>
    public void AddGold(long amount)
    {
        if (amount <= 0) return;
        CurrentGold += amount;
        OnGoldChanged?.Invoke(CurrentGold); // Event'i tetikle
        SaveGold();
    }
    public void AddPassiveGold(long amount)
    {
        CurrentGold += amount;
        OnPassiveGoldAdded?.Invoke(amount);
        OnGoldChanged?.Invoke(CurrentGold);
        SaveGold();
    }
    /// <summary>
    /// Belirtilen miktarda altın harcamaya çalışır.
    /// </summary>
    /// <returns>Harcama başarılıysa true, bakiye yetersizse false döner.</returns>
    public bool SpendGold(long amount)
    {
        if (amount <= 0) return false;

        if (CurrentGold >= amount)
        {
            CurrentGold -= amount;
            OnGoldChanged?.Invoke(CurrentGold); // Event'i tetikle
            SaveGold();
            return true; // Harcama başarılı
        }
        else
        {
            Debug.Log("Yetersiz altın!");
            return false; // Harcama başarısız
        }
    }

    private void SaveGold()
    {
        PlayerPrefs.SetString("CurrentGold", CurrentGold.ToString());
        PlayerPrefs.Save();
    }

    private void LoadGold()
    {
        string goldStr = PlayerPrefs.GetString("CurrentGold", "0");
        if (long.TryParse(goldStr, out long savedGold))
        {
            CurrentGold = savedGold;
        }
        else
        {
            CurrentGold = 0;
        }
        OnGoldChanged?.Invoke(CurrentGold); // UI'ı başlangıçta güncelle
    }
}