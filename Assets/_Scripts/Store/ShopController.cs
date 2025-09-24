using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // List kullanmak için
using DG.Tweening; // Sonradan eklenen satır

public class ShopController : MonoBehaviour
{
    [Header("Mağaza Ayarları")]
    [Tooltip("Mağazada satılacak tüm askerlerin ScriptableObject listesi.")]
    [SerializeField] private List<SoldierData> soldiersToSell;

    [Header("UI Elemanları")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI currentSoldiersText;

    [Header("Liste Yapısı")]
    [Tooltip("Mağaza ürünlerinin (ShopItem_Prefab) içine oluşturulacağı parent obje.")]
    [SerializeField] private Transform contentParent;
    [Tooltip("Tek bir mağaza ürününü temsil eden prefab.")]
    [SerializeField] private GameObject shopItemPrefab;

    // Oluşturulan tüm ShopItemUI'ları takip etmek için bir liste
    private List<ShopItemUI> spawnedShopItems = new List<ShopItemUI>();

    private void Start()
    {
        closeButton.onClick.AddListener(ClosePanel);
        shopPanel.SetActive(false);
    }

    public void ShowPanel()
    {
        shopPanel.SetActive(true);
        PopulateShop();
        UpdateCurrentSoldierCount();
    }

    private void OnEnable()
    {
        if (SoldierManager.Instance != null)
            SoldierManager.Instance.OnSoldierDataChanged += UpdateCurrentSoldierCount;
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged += UpdateAllButtonStates; // Bu satır hataya neden oluyordu
    }

    private void OnDisable()
    {
        if (SoldierManager.Instance != null)
            SoldierManager.Instance.OnSoldierDataChanged -= UpdateCurrentSoldierCount;
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= UpdateAllButtonStates;
    }

    /// <summary>
    /// Mağazayı, listedeki askerlerle dinamik olarak doldurur.
    /// </summary>
    private void PopulateShop()
    {
        // Önce eski listeyi temizle
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        spawnedShopItems.Clear();

        // soldiersToSell listesindeki her bir asker için yeni bir UI elemanı oluştur
        foreach (SoldierData soldier in soldiersToSell)
        {
            GameObject itemGO = Instantiate(shopItemPrefab, contentParent);
            ShopItemUI shopItem = itemGO.GetComponent<ShopItemUI>();

            if (shopItem != null)
            {
                // ShopItemUI'ı ayarla ve satın alma eylemini (BuySoldier) ona bağla
                shopItem.Setup(soldier, BuySoldier);
                spawnedShopItems.Add(shopItem);
            }
        }
    }

    /// <summary>
    /// Bir ShopItemUI tarafından çağrıldığında ilgili askeri satın alır.
    /// </summary>
    private void BuySoldier(SoldierData soldierToBuy)
    {
        if (CurrencyManager.Instance == null || SoldierManager.Instance == null) return;

        long cost = soldierToBuy.cost;
        if (CurrencyManager.Instance.CurrentGold >= cost)
        {
            CurrencyManager.Instance.SpendGold(cost);
            SoldierManager.Instance.AddSoldiers(1, soldierToBuy.health, soldierToBuy.attack);
        }
    }

    /// <summary>
    /// Mevcut asker sayısını günceller.
    /// </summary>
    private void UpdateCurrentSoldierCount()
    {
        if (SoldierManager.Instance != null)
        {
            currentSoldiersText.text = SoldierManager.Instance.TotalSoldiers.ToString();
        }
    }

    // --- HATA DÜZELTMESİ BURADA ---
    // Fonksiyon artık OnGoldChanged event'inden gelen 'long' parametresini kabul ediyor.
    /// <summary>
    /// Listedeki tüm ürünlerin buton durumunu günceller.
    /// </summary>
    private void UpdateAllButtonStates(long newGoldAmount)
    {
        foreach (var item in spawnedShopItems)
        {
            item.UpdateButtonState();
        }
    }
    // --- DÜZELTME SONU ---

    private void ClosePanel()
    {
        shopPanel.SetActive(false);
    }
}