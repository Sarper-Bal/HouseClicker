using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;

    // --- YENİ EKLENEN KISIM ---
    [Header("Panel References")]
    [SerializeField] private GameObject miniGamesMainPanel;
    [SerializeField] private Button openMiniGamesButton;
    // -------------------------

    private long displayedGold = 0;
    private Tween buttonPulseAnimation;
    private Vector3 upgradeButtonOriginalScale;

    private void Start()
    {
        if (upgradeButton != null)
        {
            upgradeButtonOriginalScale = upgradeButton.transform.localScale;
        }

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged += UpdateGoldText;
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnLevelUp += UpdateLevelUI;

        upgradeButton.onClick.AddListener(() => UpgradeManager.Instance.AttemptUpgrade());

        // --- YENİ EKLENEN KISIM ---
        // Mini oyun butonuna paneli açma fonksiyonunu bağla.
        openMiniGamesButton.onClick.AddListener(OpenMiniGamesPanel);
        // Panelin kendisini başlangıçta kapat (bu satır önemli).
        miniGamesMainPanel.SetActive(false);
        // -------------------------

        InitializeUI();
    }

    // --- YENİ EKLENEN FONKSİYON ---
    private void OpenMiniGamesPanel()
    {
        miniGamesMainPanel.SetActive(true);
    }
    // ----------------------------

    // ... (script'in geri kalanı aynı, değişiklik yok) ...

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= UpdateGoldText;
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnLevelUp -= UpdateLevelUI;

        if (buttonPulseAnimation != null) buttonPulseAnimation.Kill();
    }

    private void InitializeUI()
    {
        levelText.text = "Seviye: " + (UpgradeManager.Instance.GetCurrentLevelData().levelIndex + 1);
        displayedGold = CurrencyManager.Instance.CurrentGold;
        goldText.text = FormatNumber(displayedGold);
        UpdateUpgradeButton();
    }

    private void UpdateGoldText(long newGoldAmount)
    {
        DOTween.To(() => displayedGold, x => displayedGold = x, newGoldAmount, 0.5f)
            .OnUpdate(() =>
            {
                goldText.text = FormatNumber(displayedGold);
            });

        UpdateUpgradeButton();
    }

    private void UpdateLevelUI(LevelData newLevelData)
    {
        if (newLevelData == null) return;
        levelText.text = "Seviye: " + (newLevelData.levelIndex + 1);
        UpdateUpgradeButton();
    }

    private void UpdateUpgradeButton()
    {
        int nextLevelIndex = UpgradeManager.Instance.CurrentLevel + 1;
        if (nextLevelIndex >= UpgradeManager.Instance.levelConfigs.Count)
        {
            upgradeButton.interactable = false;
            upgradeButtonText.text = "Maksimum Seviye";
            if (buttonPulseAnimation != null) buttonPulseAnimation.Kill();
            upgradeButton.transform.localScale = upgradeButtonOriginalScale;
            return;
        }

        long currentGold = CurrencyManager.Instance.CurrentGold;
        long requiredGold = UpgradeManager.Instance.levelConfigs[nextLevelIndex].upgradeCost;
        upgradeButtonText.text = $"{FormatNumber(currentGold)} / {FormatNumber(requiredGold)}";
        bool canAfford = currentGold >= requiredGold;
        upgradeButton.interactable = canAfford;

        if (canAfford)
        {
            if (buttonPulseAnimation == null || !buttonPulseAnimation.IsActive())
            {
                buttonPulseAnimation = DOTween.Sequence()
                    .Append(upgradeButton.transform.DOScale(upgradeButtonOriginalScale * 1.1f, 0.4f).SetEase(Ease.OutQuad))
                    .Append(upgradeButton.transform.DOScale(upgradeButtonOriginalScale * 0.98f, 0.8f).SetEase(Ease.InOutQuad))
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }
        else
        {
            if (buttonPulseAnimation != null)
            {
                buttonPulseAnimation.Kill();
                buttonPulseAnimation = null;
                upgradeButton.transform.localScale = upgradeButtonOriginalScale;
            }
        }
    }

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