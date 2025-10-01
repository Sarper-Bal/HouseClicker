using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

[System.Serializable]
public class MultiplierBox
{
    public Button boxButton;
    public TextMeshProUGUI multiplierText;
    [HideInInspector] public float assignedMultiplier;
}

public class MultiplierGameController : MonoBehaviour
{
    [Header("Oyun Verisi")]
    [SerializeField] private MultiplierGameData gameData;

    [Header("Ana Paneller")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject resultPanel;

    [Header("Onay Paneli UI")]
    [SerializeField] private TextMeshProUGUI gameNameText;
    [SerializeField] private TextMeshProUGUI betInfoText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button closeButton;

    [Header("Oyun Paneli UI")]
    [SerializeField] private TextMeshProUGUI currentAmountText;
    [SerializeField] private List<MultiplierBox> boxes;

    [Header("Sonuç Paneli UI")]
    [SerializeField] private TextMeshProUGUI betResultText;
    [SerializeField] private TextMeshProUGUI winningsResultText;
    [SerializeField] private Button finishAndCloseButton;

    private double currentWinnings;
    private bool isGameRunning = false;
    private int boxesOpenedCount = 0;
    private long currentBetAmount; // O anki oyunun maliyetini saklamak için

    private void Start()
    {
        startGameButton.onClick.AddListener(StartGame);
        closeButton.onClick.AddListener(CloseConfirmationPanel);
        closeButton.onClick.AddListener(CloseMainPanel);
        finishAndCloseButton.onClick.AddListener(CloseMainPanel);

        foreach (var box in boxes)
        {
            box.boxButton.onClick.AddListener(() => OnBoxClicked(box));
        }

        mainPanel.SetActive(false);
    }


    // YENİ FONKSİYON: O anki seviyeye göre bahis miktarını hesaplar.
    private long GetCurrentBetAmount()
    {
        if (UpgradeManager.Instance == null)
        {
            Debug.LogError("UpgradeManager bulunamadı!");
            return gameData.baseBetAmount;
        }
        int playerLevel = UpgradeManager.Instance.CurrentLevel;
        return gameData.baseBetAmount + (playerLevel * gameData.amountPerLevel);
    }

    public void ShowConfirmationPanel()
    {
        // Cooldown kontrolü burada kalmalı, çünkü bekleme süresindeyse panel hiç açılmamalı.
        float cooldown = MiniGameManager.Instance.GetRemainingCooldown(gameData.gameName);
        if (cooldown > 0)
        {
            Debug.Log($"Tekrar oynamak için {cooldown:F0} saniye beklemelisin.");
            return;
        }

        // DEĞİŞİKLİK: Para kontrolü buradan kaldırıldı. Panel artık her zaman açılacak.

        mainPanel.SetActive(true);
        confirmationPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        resultPanel.SetActive(false);

        gameNameText.text = gameData.gameName;

        // O anki bahis miktarını hesapla
        currentBetAmount = GetCurrentBetAmount();
        betInfoText.text = currentBetAmount.ToString("N0", CultureInfo.InvariantCulture);

        // DEĞİŞİKLİK: Para kontrolünü burada yapıp butonu ayarla.
        if (CurrencyManager.Instance.CurrentGold >= currentBetAmount)
        {
            startGameButton.interactable = true;
            betInfoText.color = Color.white; // Parası yeterliyse metin rengi normal
        }
        else
        {
            startGameButton.interactable = false;
            betInfoText.color = Color.red; // Parası yetersizse kırmızı renk ile belirt
        }
    }

    private void StartGame()
    {
        // Oyna butonuna basıldığında maliyet yeniden hesaplanıp düşülür.
        currentBetAmount = GetCurrentBetAmount();

        confirmationPanel.SetActive(false);
        gameplayPanel.SetActive(true);

        isGameRunning = true;
        boxesOpenedCount = 0;

        CurrencyManager.Instance.SpendGold(currentBetAmount);
        MiniGameManager.Instance.RecordPlayTime(gameData.gameName);

        currentWinnings = currentBetAmount;
        UpdateAmountText(currentBetAmount, true);

        AssignMultipliersToBoxes();
        ResetBoxVisuals();

        foreach (var box in boxes)
        {
            box.boxButton.interactable = true;
        }
    }

    private void OnBoxClicked(MultiplierBox clickedBox)
    {
        if (!isGameRunning) return;
        clickedBox.boxButton.interactable = false;
        boxesOpenedCount++;
        StartCoroutine(RevealBox(clickedBox));
    }

    private IEnumerator RevealBox(MultiplierBox boxToReveal)
    {
        float multiplier = boxToReveal.assignedMultiplier;

        Sequence revealAnim = DOTween.Sequence();
        revealAnim.Append(boxToReveal.boxButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f));
        revealAnim.AppendCallback(() =>
        {
            boxToReveal.multiplierText.gameObject.SetActive(true);
            if (multiplier > 0)
            {
                boxToReveal.multiplierText.text = $"x{multiplier}";
                boxToReveal.multiplierText.color = Color.green;
            }
            else
            {
                boxToReveal.multiplierText.text = $"{multiplier}x";
                boxToReveal.multiplierText.color = Color.red;
            }
            boxToReveal.multiplierText.transform.localScale = Vector3.zero;
        });
        revealAnim.Append(boxToReveal.multiplierText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));

        yield return revealAnim.WaitForCompletion();

        double previousWinnings = currentWinnings;
        bool isPositive = multiplier > 0;

        if (isPositive) currentWinnings *= multiplier;
        else currentWinnings /= -multiplier;

        UpdateAmountText(currentWinnings, isPositive);

        if (boxesOpenedCount >= boxes.Count)
        {
            yield return new WaitForSeconds(1.0f);
            FinishGame();
        }
    }

    private void FinishGame()
    {
        isGameRunning = false;
        long finalWinnings = (long)System.Math.Round(currentWinnings);
        CurrencyManager.Instance.AddGold(finalWinnings);

        ShowResultPanel(finalWinnings);
    }

    private void ShowResultPanel(long finalWinnings)
    {
        gameplayPanel.SetActive(false);
        resultPanel.SetActive(true);

        // Sonuç panelinde de o anki oyun maliyetini göster
        betResultText.text = currentBetAmount.ToString("N0", CultureInfo.InvariantCulture);
        winningsResultText.text = finalWinnings.ToString("N0", CultureInfo.InvariantCulture);

        if (finalWinnings >= currentBetAmount)
        {
            winningsResultText.color = Color.green;
        }
        else
        {
            winningsResultText.color = Color.red;
        }
    }

    private void CloseMainPanel()
    {
        if (isGameRunning) return;
        mainPanel.SetActive(false);
    }

    private void AssignMultipliersToBoxes()
    {
        List<float> allMultipliers = new List<float>();
        allMultipliers.AddRange(gameData.positiveMultipliers);
        allMultipliers.AddRange(gameData.negativeMultipliers);
        var shuffledMultipliers = allMultipliers.OrderBy(x => Random.value).ToList();
        for (int i = 0; i < boxes.Count; i++)
        {
            boxes[i].assignedMultiplier = (i < shuffledMultipliers.Count) ? shuffledMultipliers[i] : 1f;
        }
    }

    private void ResetBoxVisuals()
    {
        foreach (var box in boxes)
        {
            box.boxButton.transform.localScale = Vector3.one;
            box.multiplierText.gameObject.SetActive(false);
            box.boxButton.interactable = false;
        }
    }

    private void UpdateAmountText(double targetAmount, bool isPositiveChange = true)
    {
        double.TryParse(currentAmountText.text, NumberStyles.Any, CultureInfo.InvariantCulture, out double startAmount);

        DOTween.To(() => startAmount, x => startAmount = x, targetAmount, 0.5f)
               .OnUpdate(() => currentAmountText.text = ((long)System.Math.Round(startAmount)).ToString("N0", CultureInfo.InvariantCulture));

        if (isPositiveChange)
        {
            currentAmountText.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f);
        }
        else
        {
            currentAmountText.transform.DOShakePosition(0.4f, new Vector3(10, 0, 0));
        }
    }
    private void CloseConfirmationPanel()
    {
        confirmationPanel.SetActive(false);
    }


}