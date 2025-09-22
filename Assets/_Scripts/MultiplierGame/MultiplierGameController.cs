using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Struct yapısını Button referansını da içerecek şekilde güncelledik.
[System.Serializable]
public class MultiplierBox
{
    public Button boxButton; // Artık doğrudan Button bileşenini referans alıyoruz.
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
    private int boxesOpenedCount = 0; // Kaç kutunun açıldığını saymak için yeni değişken

    private void Start()
    {
        playButton.onClick.AddListener(StartGame);
        closeButton.onClick.AddListener(ClosePanel);

        // Her bir kutu butonuna tıklandığında OnBoxClicked fonksiyonunu çağıracak şekilde ayarla
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
            // Kutuları başlangıç görünümüne getir
            box.boxButton.transform.localScale = Vector3.one;
            box.multiplierText.gameObject.SetActive(false);
            box.boxButton.interactable = false; // Oyun başlayana kadar tıklanmasın
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
        UpdateAmountText(gameData.betAmount);

        AssignMultipliersToBoxes();

        // Kutuları tıklanabilir hale getir
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

    // Bir kutuya tıklandığında bu fonksiyon çalışır
    private void OnBoxClicked(MultiplierBox clickedBox)
    {
        if (!isGameRunning) return;

        clickedBox.boxButton.interactable = false; // Tıklanan kutuyu tekrar tıklanmaz yap
        boxesOpenedCount++;

        // Animasyon ve hesaplama işlemleri için yeni coroutine başlat
        StartCoroutine(RevealBox(clickedBox));
    }

    private IEnumerator RevealBox(MultiplierBox boxToReveal)
    {
        float multiplier = boxToReveal.assignedMultiplier;

        // Kutu açılma animasyonu
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
                boxToReveal.multiplierText.text = $"÷{-multiplier}";
                boxToReveal.multiplierText.color = Color.red;
            }
            boxToReveal.multiplierText.transform.localScale = Vector3.zero;
        });
        revealAnim.Append(boxToReveal.multiplierText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));

        // Animasyonun bitmesini bekle
        yield return revealAnim.WaitForCompletion();

        // Parayı güncelle
        double previousWinnings = currentWinnings;
        if (multiplier > 0)
        {
            currentWinnings *= multiplier;
        }
        else
        {
            currentWinnings /= -multiplier;
        }

        // Para metninin animasyonlu değişimi
        DOTween.To(() => previousWinnings, x => previousWinnings = x, currentWinnings, 0.5f)
               .OnUpdate(() => UpdateAmountText(previousWinnings));

        // Eğer tüm kutular açıldıysa oyunu bitir
        if (boxesOpenedCount >= boxes.Count)
        {
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

    private void UpdateAmountText(double amount)
    {
        currentAmountText.text = ((long)System.Math.Round(amount)).ToString("N0");
    }

    private void ClosePanel()
    {
        if (isGameRunning) return;
        gamePanel.SetActive(false);
    }
}