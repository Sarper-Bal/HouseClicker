using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // Hatanın çözümü için eklenen satır

public class ShopController : MonoBehaviour
{
    [Header("Mağaza Ürünü")]
    [SerializeField] private SoldierData soldierToSell; // Mağazada satılacak askerin verisi

    [Header("UI Elemanları")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeButton;

    [Header("Bilgi Alanları")]
    [SerializeField] private Image soldierIcon;
    [SerializeField] private TextMeshProUGUI soldierNameText;
    [SerializeField] private TextMeshProUGUI statsText; // "Sağlık: 10, Saldırı: 2"
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI currentSoldiersText; // "Mevcut Asker: 5"

    private void Start()
    {
        buyButton.onClick.AddListener(BuySoldier);
        closeButton.onClick.AddListener(ClosePanel);
        shopPanel.SetActive(false);
    }

    // Bu fonksiyon, ana menüdeki "Mağaza" butonu tarafından çağrılacak
    public void ShowPanel()
    {
        if (soldierToSell == null)
        {
            Debug.LogError("Mağazada satılacak asker verisi atanmamış!");
            return;
        }

        shopPanel.SetActive(true);
        UpdateUI();
    }

    private void OnEnable()
    {
        // Panel her açıldığında mevcut asker sayısını güncel tutmak için
        if (SoldierManager.Instance != null)
        {
            SoldierManager.Instance.OnSoldierDataChanged += UpdateUI;
        }
    }

    private void OnDisable()
    {
        // Paneli kapatırken event aboneliğini iptal et, bu önemli!
        if (SoldierManager.Instance != null)
        {
            SoldierManager.Instance.OnSoldierDataChanged -= UpdateUI;
        }
    }

    private void UpdateUI()
    {
        // ScriptableObject'ten gelen verilerle UI'ı doldur
        soldierIcon.sprite = soldierToSell.shopIcon;
        soldierNameText.text = soldierToSell.soldierName;
        statsText.text = $"Sağlık: {soldierToSell.health}\nSaldırı: {soldierToSell.attack}";
        costText.text = soldierToSell.cost.ToString("N0");

        // Mevcut asker sayısını SoldierManager'dan al
        if (SoldierManager.Instance != null)
        {
            currentSoldiersText.text = $"Mevcut Asker: {SoldierManager.Instance.TotalSoldiers}";
        }

        // Paranın yetip yetmediğini kontrol edip butonu ayarla
        if (CurrencyManager.Instance != null)
        {
            buyButton.interactable = CurrencyManager.Instance.CurrentGold >= soldierToSell.cost;
        }
    }

    private void BuySoldier()
    {
        // Gerekli kontroller
        if (CurrencyManager.Instance == null || SoldierManager.Instance == null) return;

        long cost = soldierToSell.cost;

        // Paranın yettiğinden emin ol
        if (CurrencyManager.Instance.CurrentGold >= cost)
        {
            // Parayı düş
            CurrencyManager.Instance.SpendGold(cost);
            // Askeri ekle
            SoldierManager.Instance.AddSoldiers(1, soldierToSell.health, soldierToSell.attack);

            // Başarılı satın alım animasyonu veya sesi oynatılabilir
            buyButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
        }
    }

    private void ClosePanel()
    {
        shopPanel.SetActive(false);
    }
}