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

    // --- YENİDEN EKLENEN KISIM ---
    [Header("Panel References")]
    [Tooltip("İçinde tüm mini oyunların bulunduğu ana panel.")]
    [SerializeField] private GameObject miniGamesMainPanel;
    [Tooltip("Yukarıdaki paneli açacak olan buton.")]
    [SerializeField] private Button openMiniGamesButton;
    // --- EKLENEN KISIM SONU ---

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

        // --- YENİDEN EKLENEN KISIM ---
        // Mini oyun butonuna paneli açma fonksiyonunu bağla.
        if (openMiniGamesButton != null)
        {
            openMiniGamesButton.onClick.AddListener(OpenMiniGamesPanel);
        }
        // Panelin kendisini başlangıçta kapat.
        if (miniGamesMainPanel != null)
        {
            miniGamesMainPanel.SetActive(false);
        }
        // --- EKLENEN KISIM SONU ---

        InitializeUI();
    }

    // --- YENİDEN EKLENEN FONKSİYON ---
    private void OpenMiniGamesPanel()
    {
        if (miniGamesMainPanel != null)
        {
            miniGamesMainPanel.SetActive(true);
        }
    }
    // --- EKLENEN FONKSİYON SONU ---

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
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.GetCurrentLevelData() != null)
        {
            levelText.text = "Seviye: " + (UpgradeManager.Instance.GetCurrentLevelData().levelIndex + 1);
        }
        if (CurrencyManager.Instance != null)
        {
            displayedGold = CurrencyManager.Instance.CurrentGold;
            goldText.text = FormatNumber(displayedGold);
        }
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
        if (UpgradeManager.Instance == null || CurrencyManager.Instance == null) return;

        int nextLevelIndex = UpgradeManager.Instance.CurrentLevel + 1;
        if (nextLevelIndex >= UpgradeManager.Instance.levelConfigs.Count)
        {
            upgradeButton.interactable = false;
            upgradeButtonText.text = "Maksimum Seviye";
            if (buttonPulseAnimation != null) buttonPulseAnimation.Kill();
            if (upgradeButton != null) upgradeButton.transform.localScale = upgradeButtonOriginalScale;
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
                if (upgradeButton != null) upgradeButton.transform.localScale = upgradeButtonOriginalScale;
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