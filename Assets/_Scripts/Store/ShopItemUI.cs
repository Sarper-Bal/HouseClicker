using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Action kullanabilmek için
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

    private SoldierData soldierData;
    private Action<SoldierData> onBuyAction;

    /// <summary>
    /// Bu UI elemanını bir SoldierData ile doldurur ve satın alma eylemini ayarlar.
    /// </summary>
    public void Setup(SoldierData data, Action<SoldierData> buyAction)
    {
        this.soldierData = data;
        this.onBuyAction = buyAction;

        // UI elemanlarını doldur
        soldierIcon.sprite = soldierData.shopIcon;
        soldierNameText.text = soldierData.soldierName;
        healthText.text = soldierData.health.ToString();
        attackText.text = soldierData.attack.ToString();
        costText.text = soldierData.cost.ToString("N0");

        // Satın al butonuna tıklama olayını ayarla
        buyButton.onClick.AddListener(HandleBuyClick);

        // Başlangıçta buton durumunu paraya göre ayarla
        UpdateButtonState();
    }

    /// <summary>
    /// Satın Al butonuna tıklandığında çalışır.
    /// </summary>
    private void HandleBuyClick()
    {
        // Ana ShopController'a hangi askerin satın alınmak istendiğini bildirir.
        onBuyAction?.Invoke(soldierData);

        // Animasyon
        buyButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
    }

    /// <summary>
    /// Oyuncunun parasına göre Satın Al butonunun tıklanabilirliğini günceller.
    /// </summary>
    public void UpdateButtonState()
    {
        if (CurrencyManager.Instance != null && soldierData != null)
        {
            buyButton.interactable = CurrencyManager.Instance.CurrentGold >= soldierData.cost;
        }
    }

    private void OnDestroy()
    {
        buyButton.onClick.RemoveAllListeners();
    }
}