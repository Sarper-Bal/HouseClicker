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

    // Metot artık string/long yerine SoldierData alıyor.
    public void BuySoldier(SoldierData soldierData)
    {
        if (CurrencyManager.Instance.SpendGold(soldierData.cost))
        {
            // SoldierManager'a da SoldierData gönderiyoruz, bu hatayı düzeltecek.
            SoldierManager.Instance.AddSoldier(soldierData, 1);
        }
        else
        {
            Debug.Log("Altın yetersiz!");
        }
    }
}