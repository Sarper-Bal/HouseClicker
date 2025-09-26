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
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI currentCountText;


    private SoldierData soldierData;
    private ShopController shopController;

    private void Start()
    {
        if (SoldierManager.Instance != null)
        {
            SoldierManager.Instance.OnSoldierCountChanged += UpdateCurrentCountText;
        }
    }

    private void OnDestroy()
    {
        if (SoldierManager.Instance != null)
        {
            SoldierManager.Instance.OnSoldierCountChanged -= UpdateCurrentCountText;
        }
    }

    public void Setup(SoldierData data, ShopController controller)
    {
        soldierData = data;
        shopController = controller;

        soldierIcon.sprite = soldierData.shopIcon;
        soldierNameText.text = soldierData.soldierName;
        costText.text = soldierData.cost.ToString();

        // --- DEĞİŞTİRİLEN SATIRLAR ---
        attackText.text = soldierData.attack.ToString();
        healthText.text = soldierData.health.ToString();
        // -------------------------

        if (SoldierManager.Instance != null)
        {
            int currentCount = SoldierManager.Instance.GetSoldierCount(soldierData.soldierName);
            // --- DEĞİŞTİRİLEN SATIR ---
            currentCountText.text = currentCount.ToString();
            // -------------------------
        }

        buyButton.onClick.AddListener(OnBuyButtonClicked);
    }

    private void OnBuyButtonClicked()
    {
        if (shopController != null && soldierData != null)
        {
            shopController.BuySoldier(soldierData);
        }
    }

    private void UpdateCurrentCountText(string name, int newCount)
    {
        if (soldierData != null && soldierData.soldierName == name)
        {
            // --- DEĞİŞTİRİLEN SATIR ---
            currentCountText.text = newCount.ToString();
            // -------------------------
        }
    }
}