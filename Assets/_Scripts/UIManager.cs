using UnityEngine;
using UnityEngine.UI; // Button için gerekli
using TMPro; // TextMeshPro için gerekli
using System.Globalization; // Sayıları formatlamak için
using DG.Tweening; // DOTween için

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;

    private long displayedGold = 0; // Animasyon için mevcut gösterilen altın

    private void Start()
    {
        // İlgili event'lere abone ol. Bu sayede CurrencyManager veya UpgradeManager'da
        // bir değişiklik olduğunda bizim fonksiyonlarımız otomatik olarak çağrılır.
        CurrencyManager.Instance.OnGoldChanged += UpdateGoldText;
        UpgradeManager.Instance.OnLevelUp += UpdateLevelUI;

        // Butonun tıklama olayına AttemptUpgrade fonksiyonunu bağla.
        upgradeButton.onClick.AddListener(() => UpgradeManager.Instance.AttemptUpgrade());

        // Başlangıç değerlerini set et
        InitializeUI();
    }

    private void OnDestroy()
    {
        // Obje yok olduğunda event aboneliklerini iptal et, bu memory leak'leri önler.
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= UpdateGoldText;
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnLevelUp -= UpdateLevelUI;
    }

    private void InitializeUI()
    {
        // Oyuncu oyunu açtığında UI'ın doğru değerlerle başlamasını sağla.
        UpdateLevelUI(UpgradeManager.Instance.GetCurrentLevelData());
        UpdateGoldText(CurrencyManager.Instance.CurrentGold);
    }

    private void UpdateGoldText(long newGoldAmount)
    {
        // DOTween ile sayıyı akıcı bir şekilde artır/azalt
        DOTween.To(() => displayedGold, x => displayedGold = x, newGoldAmount, 0.5f)
            .OnUpdate(() =>
            {
                // Sayıyı daha okunaklı formatla (örn: 1.234 K, 5.678 M)
                goldText.text = FormatNumber(displayedGold);
            });

        // Altın miktarı her değiştiğinde butonun durumunu kontrol et.
        CheckUpgradeButtonState();
    }

    private void UpdateLevelUI(LevelData newLevelData)
    {
        if (newLevelData == null) return;

        levelText.text = "Seviye: " + (newLevelData.levelIndex + 1);

        // Son seviyeye ulaşıldı mı?
        int nextLevelIndex = UpgradeManager.Instance.CurrentLevel + 1;
        if (nextLevelIndex < UpgradeManager.Instance.levelConfigs.Count)
        {
            LevelData nextLevelData = UpgradeManager.Instance.levelConfigs[nextLevelIndex];
            upgradeButtonText.text = $"Geliştir\n({FormatNumber(nextLevelData.upgradeCost)})";
            upgradeButton.gameObject.SetActive(true);
        }
        else
        {
            // Son seviyedeyse butonu gizle.
            upgradeButtonText.text = "Maksimum Seviye";
            upgradeButton.gameObject.SetActive(false);
        }

        CheckUpgradeButtonState();
    }

    private void CheckUpgradeButtonState()
    {
        int nextLevelIndex = UpgradeManager.Instance.CurrentLevel + 1;
        if (nextLevelIndex < UpgradeManager.Instance.levelConfigs.Count)
        {
            LevelData nextLevelData = UpgradeManager.Instance.levelConfigs[nextLevelIndex];
            // Yeterli altın varsa butonu tıklanabilir yap, yoksa yapma.
            upgradeButton.interactable = CurrencyManager.Instance.CurrentGold >= nextLevelData.upgradeCost;
        }
    }

    // Sayıları kısaltan (1000 -> 1K, 1000000 -> 1M) yardımcı fonksiyon
    private string FormatNumber(long num)
    {
        if (num >= 1000000000)
            return (num / 1000000000D).ToString("0.#") + "B";
        if (num >= 1000000)
            return (num / 1000000D).ToString("0.#") + "M";
        if (num >= 1000)
            return (num / 1000D).ToString("0.#") + "K";

        return num.ToString("#,0", CultureInfo.InvariantCulture);
    }
}