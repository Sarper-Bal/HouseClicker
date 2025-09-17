using UnityEngine;
using System; // Event'ler için gerekli

public class CurrencyManager : MonoBehaviour
{
    // --- Singleton Pattern Başlangıcı ---
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
            DontDestroyOnLoad(gameObject); // Sahne değişse bile bu obje yok olmasın.
        }
    }
    // --- Singleton Pattern Sonu ---

    public long CurrentGold { get; private set; }

    // Bu event, altın miktarı değiştiğinde diğer script'lere haber vermek için kullanılır (Örn: UI'ın güncellenmesi).
    public event Action<long> OnGoldChanged;

    private void Start()
    {
        LoadGold();
    }

    public void AddGold(long amount)
    {
        if (amount < 0) return; // Negatif altın eklenmesini engelle.
        CurrentGold += amount;
        OnGoldChanged?.Invoke(CurrentGold); // Event'i tetikle.
        SaveGold(); // Her altın kazandığında otomatik kaydet.
    }

    public void SpendGold(long amount)
    {
        if (amount < 0) return; // Negatif harcama olmasın.
        if (CurrentGold >= amount)
        {
            CurrentGold -= amount;
            OnGoldChanged?.Invoke(CurrentGold); // Event'i tetikle.
            SaveGold();
        }
    }

    // Basit PlayerPrefs ile kayıt sistemi
    private void SaveGold()
    {
        // long tipini doğrudan kaydedemediğimiz için string'e çeviriyoruz.
        PlayerPrefs.SetString("CurrentGold", CurrentGold.ToString());
    }

    private void LoadGold()
    {
        string goldStr = PlayerPrefs.GetString("CurrentGold", "0");
        long.TryParse(goldStr, out long loadedGold);
        CurrentGold = loadedGold;
        OnGoldChanged?.Invoke(CurrentGold); // UI'ın başlangıçta doğru değeri göstermesi için tetikle.
    }
}