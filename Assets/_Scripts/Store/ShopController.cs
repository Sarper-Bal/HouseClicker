using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopController : MonoBehaviour
{
    [Header("Mağaza Ayarları")]
    [SerializeField] private List<SoldierData> soldiersToSell;

    [Header("UI Elemanları")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI totalSoldiersText; // "Toplam Asker" metni

    [Header("Liste Yapısı")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject shopItemPrefab;

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
        UpdateTotalSoldierCount();
    }

    private void OnEnable()
    {
        if (SoldierManager.Instance != null)
            SoldierManager.Instance.OnSoldierDataChanged += OnSoldierDataChanged;
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged += UpdateAllButtonStates;
    }

    private void OnDisable()
    {
        if (SoldierManager.Instance != null)
            SoldierManager.Instance.OnSoldierDataChanged -= OnSoldierDataChanged;
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= UpdateAllButtonStates;
    }

    // Asker verisi değiştiğinde hem toplamı hem de bireysel sayıları günceller.
    private void OnSoldierDataChanged()
    {
        UpdateTotalSoldierCount();
        UpdateAllOwnedCounts();
    }

    private void PopulateShop()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        spawnedShopItems.Clear();

        foreach (SoldierData soldier in soldiersToSell)
        {
            GameObject itemGO = Instantiate(shopItemPrefab, contentParent);
            ShopItemUI shopItem = itemGO.GetComponent<ShopItemUI>();

            if (shopItem != null)
            {
                shopItem.Setup(soldier, BuySoldier);
                spawnedShopItems.Add(shopItem);
            }
        }
    }

    private void BuySoldier(SoldierData soldierToBuy)
    {
        if (CurrencyManager.Instance == null || SoldierManager.Instance == null) return;

        long cost = soldierToBuy.cost;
        if (CurrencyManager.Instance.CurrentGold >= cost)
        {
            CurrencyManager.Instance.SpendGold(cost);
            // Artık AddSoldiers yerine AddSoldier kullanıyoruz ve SoldierData'yı gönderiyoruz.
            SoldierManager.Instance.AddSoldier(soldierToBuy);
        }
    }

    private void UpdateTotalSoldierCount()
    {
        if (SoldierManager.Instance != null)
        {
            totalSoldiersText.text = SoldierManager.Instance.GetTotalSoldierCount().ToString();
        }
    }

    // YENİ FONKSİYON: Listedeki tüm ürünlerin sahip olunan asker sayısını günceller.
    private void UpdateAllOwnedCounts()
    {
        foreach (var item in spawnedShopItems)
        {
            item.UpdateOwnedCount();
        }
    }

    private void UpdateAllButtonStates(long newGoldAmount)
    {
        foreach (var item in spawnedShopItems)
        {
            item.UpdateButtonState();
        }
    }

    private void ClosePanel()
    {
        shopPanel.SetActive(false);
    }
}