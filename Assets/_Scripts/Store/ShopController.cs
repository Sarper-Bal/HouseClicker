using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ShopController : MonoBehaviour
{
    [Header("Mağaza Ürünü")]
    [SerializeField] private SoldierData soldierToSell;

    [Header("UI Elemanları")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeButton;

    [Header("Bilgi Alanları")]
    [SerializeField] private Image soldierIcon;
    [SerializeField] private TextMeshProUGUI soldierNameText;

    // --- DEĞİŞİKLİK 1: 'statsText' yerine iki ayrı text ---
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;
    // --- DEĞİŞİKLİK SONU ---

    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI currentSoldiersText;

    private void Start()
    {
        buyButton.onClick.AddListener(BuySoldier);
        closeButton.onClick.AddListener(ClosePanel);
        shopPanel.SetActive(false);

        if (SoldierManager.Instance != null)
        {
            SoldierManager.Instance.OnSoldierDataChanged += UpdateDynamicUI;
        }
    }

    private void OnDestroy()
    {
        if (SoldierManager.Instance != null)
        {
            SoldierManager.Instance.OnSoldierDataChanged -= UpdateDynamicUI;
        }
    }

    public void ShowPanel()
    {
        if (soldierToSell == null)
        {
            Debug.LogError("Mağazada satılacak asker verisi atanmamış!");
            return;
        }

        // Paneli açmadan önce tüm UI'ı bir kez dolduralım
        UpdateStaticUI();
        UpdateDynamicUI();

        shopPanel.SetActive(true);
    }

    // Sadece bir kez, panel açılırken güncellenmesi gereken statik bilgiler
    private void UpdateStaticUI()
    {
        soldierIcon.sprite = soldierToSell.shopIcon;
        soldierNameText.text = soldierToSell.soldierName;
        costText.text = soldierToSell.cost.ToString("N0");

        // --- DEĞİŞİKLİK 2: Sağlık ve Saldırı metinlerini etiket olmadan ayarla ---
        healthText.text = soldierToSell.health.ToString();
        attackText.text = soldierToSell.attack.ToString();
    }

    // Asker veya para sayısı gibi dinamik olarak değişen bilgileri günceller
    private void UpdateDynamicUI()
    {
        if (SoldierManager.Instance != null)
        {
            // --- DEĞİŞİKLİK 3: "Mevcut Asker:" etiketini kaldır ---
            currentSoldiersText.text = SoldierManager.Instance.TotalSoldiers.ToString();
        }

        if (CurrencyManager.Instance != null && soldierToSell != null)
        {
            buyButton.interactable = CurrencyManager.Instance.CurrentGold >= soldierToSell.cost;
        }
    }

    private void BuySoldier()
    {
        if (CurrencyManager.Instance == null || SoldierManager.Instance == null) return;

        long cost = soldierToSell.cost;

        if (CurrencyManager.Instance.CurrentGold >= cost)
        {
            CurrencyManager.Instance.SpendGold(cost);
            SoldierManager.Instance.AddSoldiers(1, soldierToSell.health, soldierToSell.attack);

            buyButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);

            // Satın alımdan sonra butonun durumu ve asker sayısı anında güncellenir.
            // OnSoldierDataChanged eventi bu fonksiyonu otomatik tetikleyecektir.
        }
    }

    private void ClosePanel()
    {
        shopPanel.SetActive(false);
    }
}