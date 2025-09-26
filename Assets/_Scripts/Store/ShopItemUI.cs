using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField] private Image soldierIcon;
    [SerializeField] private TextMeshProUGUI soldierNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button buyButton;

    private SoldierData soldierData;
    private ShopController shopController;

    public void Setup(SoldierData data, ShopController controller)
    {
        soldierData = data;
        shopController = controller;

        soldierIcon.sprite = soldierData.shopIcon;
        soldierNameText.text = soldierData.soldierName;
        costText.text = soldierData.cost.ToString();

        buyButton.onClick.AddListener(OnBuyButtonClicked);
    }

    private void OnBuyButtonClicked()
    {
        if (shopController != null && soldierData != null)
        {
            // DEĞİŞİKLİK: Artık string ve cost yerine direkt SoldierData'yı gönderiyoruz.
            shopController.BuySoldier(soldierData);
        }
    }
}