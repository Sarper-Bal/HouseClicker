using UnityEngine;

public class ShopController : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Transform shopItemContainer;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private SoldierData[] availableSoldiers;

    private void Start()
    {
        PopulateShop();
    }

    private void PopulateShop()
    {
        foreach (var soldierData in availableSoldiers)
        {
            GameObject itemGO = Instantiate(shopItemPrefab, shopItemContainer);
            ShopItemUI shopItem = itemGO.GetComponent<ShopItemUI>();
            shopItem.Setup(soldierData, this);
        }
    }

    /// <summary>
    /// Bir asker satın alma işlemini gerçekleştirir.
    /// </summary>
    public void BuySoldier(SoldierData soldierData)
    {
        if (CurrencyManager.Instance.SpendGold(soldierData.cost))
        {
            // --- DEĞİŞTİRİLEN SATIR ---
            // Artık string adı yerine doğrudan SoldierData objesini gönderiyoruz.
            SoldierManager.Instance.AddSoldier(soldierData, 1);
            // -------------------------
        }
        else
        {
            Debug.Log("Altın yetersiz!");
        }
    }
}