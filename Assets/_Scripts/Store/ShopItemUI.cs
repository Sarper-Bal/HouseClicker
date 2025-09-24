using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI Elemanları")]
    [SerializeField] private Image soldierIcon;
    [SerializeField] private TextMeshProUGUI soldierNameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI ownedCountText; // YENİ: Sahip olunan sayıyı gösteren metin

    private SoldierData soldierData;
    private Action<SoldierData> onBuyAction;

    public void Setup(SoldierData data, Action<SoldierData> buyAction)
    {
        this.soldierData = data;
        this.onBuyAction = buyAction;

        soldierIcon.sprite = soldierData.shopIcon;
        soldierNameText.text = soldierData.soldierName;
        healthText.text = soldierData.health.ToString();
        attackText.text = soldierData.attack.ToString();
        costText.text = soldierData.cost.ToString("N0");

        buyButton.onClick.AddListener(HandleBuyClick);

        UpdateButtonState();
        UpdateOwnedCount(); // Başlangıçta sahip olunan sayıyı güncelle
    }

    private void HandleBuyClick()
    {
        onBuyAction?.Invoke(soldierData);
        buyButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
    }

    public void UpdateButtonState()
    {
        if (CurrencyManager.Instance != null && soldierData != null)
        {
            buyButton.interactable = CurrencyManager.Instance.CurrentGold >= soldierData.cost;
        }
    }

    // YENİ FONKSİYON: SoldierManager'dan alınan bilgiyle sahip olunan asker sayısını günceller.
    public void UpdateOwnedCount()
    {
        if (SoldierManager.Instance != null && soldierData != null && ownedCountText != null)
        {
            int count = SoldierManager.Instance.GetSoldierCount(soldierData.soldierName);
            ownedCountText.text = count.ToString();
        }
    }

    private void OnDestroy()
    {
        buyButton.onClick.RemoveAllListeners();
    }
}