using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    [Header("UI Elemanları")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI currentAmountText;
    [SerializeField] private List<MultiplierBox> boxes;

    private double currentWinnings;
    private bool isGameRunning = false;
    private int boxesOpenedCount = 0;

    private void Start()
    {
        playButton.onClick.AddListener(StartGame);
        closeButton.onClick.AddListener(ClosePanel);

        foreach (var box in boxes)
        {
            box.boxButton.onClick.AddListener(() => OnBoxClicked(box));
        }

        gamePanel.SetActive(false);
    }

    public void ShowPanel()
    {
        if (CurrencyManager.Instance.CurrentGold < gameData.betAmount)
        {
            Debug.Log("Yetersiz Altın!");
            return;
        }

        float cooldown = MiniGameManager.Instance.GetRemainingCooldown(gameData.gameName);
        if (cooldown > 0)
        {
            Debug.Log($"Tekrar oynamak için {cooldown:F0} saniye beklemelisin.");
            return;
        }

        gamePanel.SetActive(true);
        ResetGame();
    }

    private void ResetGame()
    {
        infoText.text = $"Bahis: {gameData.betAmount} Altın";
        currentAmountText.text = "0";
        boxesOpenedCount = 0;

        foreach (var box in boxes)
        {
            box.boxButton.transform.localScale = Vector3.one;
            box.multiplierText.gameObject.SetActive(false);
            box.boxButton.interactable = false;
        }

        playButton.interactable = true;
        isGameRunning = false;
    }

    private void StartGame()
    {
        if (isGameRunning) return;

        isGameRunning = true;
        playButton.interactable = false;
        infoText.text = "Kutuları Aç!";

        CurrencyManager.Instance.SpendGold(gameData.betAmount);
        MiniGameManager.Instance.RecordPlayTime(gameData.gameName);

        currentWinnings = gameData.betAmount;
        UpdateAmountText(gameData.betAmount, true); // Başlangıç animasyonu için

        AssignMultipliersToBoxes();

        foreach (var box in boxes)
        {
            box.boxButton.interactable = true;
        }
    }

    private void AssignMultipliersToBoxes()
    {
        List<float> allMultipliers = new List<float>();
        allMultipliers.AddRange(gameData.positiveMultipliers);
        allMultipliers.AddRange(gameData.negativeMultipliers);

        var shuffledMultipliers = allMultipliers.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < boxes.Count; i++)
        {
            if (i < shuffledMultipliers.Count)
            {
                boxes[i].assignedMultiplier = shuffledMultipliers[i];
            }
            else
            {
                boxes[i].assignedMultiplier = 1f;
            }
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

            // --- DEĞİŞİKLİK BURADA ---
            if (multiplier > 0)
            {
                boxToReveal.multiplierText.text = $"x{multiplier}";
                boxToReveal.multiplierText.color = Color.green;
            }
            else // Negatif ise
            {
                // Metni "-2x" formatında göster
                boxToReveal.multiplierText.text = $"{multiplier}x";
                boxToReveal.multiplierText.color = Color.red;
            }
            // --- DEĞİŞİKLİK SONU ---

            boxToReveal.multiplierText.transform.localScale = Vector3.zero;
        });
        revealAnim.Append(boxToReveal.multiplierText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));

        yield return revealAnim.WaitForCompletion();

        double previousWinnings = currentWinnings;
        bool isPositive = multiplier > 0;

        if (isPositive)
        {
            currentWinnings *= multiplier;
        }
        else
        {
            // Negatif çarpanın değeri -2 ise 2'ye böl, -3 ise 3'e böl.
            currentWinnings /= -multiplier;
        }

        UpdateAmountText(currentWinnings, isPositive);


        if (boxesOpenedCount >= boxes.Count)
        {
            // Son kutudan sonra biraz bekleyip oyunu bitir.
            yield return new WaitForSeconds(1.0f);
            FinishGame();
        }
    }

    private void FinishGame()
    {
        isGameRunning = false;
        long finalWinnings = (long)System.Math.Round(currentWinnings);
        infoText.text = $"Kazanç: {finalWinnings} Altın!";
        CurrencyManager.Instance.AddGold(finalWinnings);

        StartCoroutine(ResetAfterDelay(3f));
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetGame();
    }

    // Metin güncelleme fonksiyonunu animasyonlu hale getirelim.
    private void UpdateAmountText(double targetAmount, bool isPositiveChange = true)
    {
        double startAmount = double.Parse(currentAmountText.text, System.Globalization.NumberStyles.Any);

        DOTween.To(() => startAmount, x => startAmount = x, targetAmount, 0.5f)
               .OnUpdate(() => currentAmountText.text = ((long)System.Math.Round(startAmount)).ToString("N0"));

        // Animasyonlu geri bildirim
        if (isPositiveChange)
        {
            currentAmountText.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f);
        }
        else
        {
            currentAmountText.transform.DOShakePosition(0.4f, new Vector3(10, 0, 0));
        }
    }

    private void ClosePanel()
    {
        if (isGameRunning) return;
        gamePanel.SetActive(false);
    }
}