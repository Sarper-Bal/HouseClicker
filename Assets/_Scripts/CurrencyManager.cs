using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    // --- Singleton Pattern (Sahneye Özel) ---
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
        }
    }
    // --- Singleton Pattern Sonu ---

    public long CurrentGold { get; private set; }

    // Genel altın değişiklikleri için (UI güncellemeleri vb.)
    public event Action<long> OnGoldChanged;

    // --- HATA DÜZELTMESİ ---
    // FloatingTextManager'ın ihtiyaç duyduğu, sadece pasif gelir eklendiğinde tetiklenecek olan olay.
    public event Action<long> OnPassiveGoldAdded;
    // --- HATA DÜZELTMESİ SONU ---

    private const string GoldSaveKey = "CurrentGold";

    private void Start()
    {
        LoadGold();
        // Başlangıçta UI'ın doğru değeri göstermesi için event'i tetikle.
        OnGoldChanged?.Invoke(CurrentGold);
    }

    // Tıklama, bonus gibi anlık ve aktif kazançlar için bu metodu kullan.
    public void AddGold(long amount)
    {
        if (amount <= 0) return;
        CurrentGold += amount;
        OnGoldChanged?.Invoke(CurrentGold);
        SaveGold();
    }

    // --- YENİ EKLENEN METOD ---
    // Sadece pasif gelir ekleyen script'lerin bu metodu çağırması gerekiyor.
    public void AddPassiveGold(long amount)
    {
        if (amount <= 0) return;
        CurrentGold += amount;
        OnGoldChanged?.Invoke(CurrentGold); // Genel UI'ın da güncellenmesi için bu event'i tetikliyoruz.
        OnPassiveGoldAdded?.Invoke(amount); // FloatingTextManager için özel event'i tetikliyoruz.
        SaveGold();
    }
    // --- YENİ METOD SONU ---

    public void SpendGold(long amount)
    {
        if (amount <= 0 || amount > CurrentGold) return;
        CurrentGold -= amount;
        OnGoldChanged?.Invoke(CurrentGold);
        SaveGold();
    }

    private void SaveGold()
    {
        PlayerPrefs.SetString(GoldSaveKey, CurrentGold.ToString());
        PlayerPrefs.Save();
    }

    private void LoadGold()
    {
        string savedGold = PlayerPrefs.GetString(GoldSaveKey, "0");
        long.TryParse(savedGold, out long loadedGold);
        CurrentGold = loadedGold;
    }
}