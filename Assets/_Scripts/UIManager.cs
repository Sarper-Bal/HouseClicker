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

    [Header("Panel References")]
    [SerializeField] private GameObject miniGamesMainPanel;
    [SerializeField] private Button openMiniGamesButton;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button openShopButton;
    [SerializeField] private Button closeShopButton;

    [Header("Sahne Geçiş")]
    [SerializeField] private Button goToMapButton;

    private long displayedGold = 0;
    private Tween buttonPulseAnimation;
    private Vector3 upgradeButtonOriginalScale;

    private void Start()
    {
        if (upgradeButton != null)
        {
            upgradeButtonOriginalScale = upgradeButton.transform.localScale;
        }

        // --- YENİ EKLENEN SATIR ---
        // Sahne her yüklendiğinde SoldierManager'ın verilerini tazelemesini sağla.
        if (SoldierManager.Instance != null)
        {
            SoldierManager.Instance.RefreshDataFromPrefs();
        }
        // -------------------------

        // Event Listeners
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged += UpdateGoldText;
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnLevelUp += UpdateLevelUI;

        // Button Clicks
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(() => UpgradeManager.Instance.AttemptUpgrade());

        if (openMiniGamesButton != null)
            openMiniGamesButton.onClick.AddListener(OpenMiniGamesPanel);

        if (goToMapButton != null)
            goToMapButton.onClick.AddListener(GoToMapScene);

        if (openShopButton != null)
            openShopButton.onClick.AddListener(OpenShopPanel);
        if (closeShopButton != null)
            closeShopButton.onClick.AddListener(CloseShopPanel);

        // Panelleri başlangıçta gizle
        if (miniGamesMainPanel != null)
            miniGamesMainPanel.SetActive(false);
        if (shopPanel != null)
            shopPanel.SetActive(false);

        InitializeUI();
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= UpdateGoldText;
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnLevelUp -= UpdateLevelUI;

        if (buttonPulseAnimation != null)
        {
            buttonPulseAnimation.Kill();
        }
    }

    private void OpenShopPanel()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
    }

    private void CloseShopPanel()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    private void GoToMapScene()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadMapScene();
        }
        else
        {
            Debug.LogError("SceneLoader sahnede bulunamadı!");
        }
    }

    private void OpenMiniGamesPanel()
    {
        if (miniGamesMainPanel != null)
        {
            miniGamesMainPanel.SetActive(true);
        }
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
                if (goldText != null) goldText.text = FormatNumber(displayedGold);
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
        if (UpgradeManager.Instance == null || CurrencyManager.Instance == null || upgradeButton == null) return;

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