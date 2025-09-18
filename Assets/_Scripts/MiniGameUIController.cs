using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MiniGameUIController : MonoBehaviour
{
    // --- BU KISIM DEĞİŞTİ ---
    // Paneli açma/kapama sorumluluğu artık UIManager'da olduğu için
    // openMiniGamesButton referansını sildik.
    [Header("Ana Paneller ve Butonlar")]
    [SerializeField] private Button closeMiniGamesButton;
    // ---------------------------

    [Header("Sayı Toplama Oyunu (Sum Game) UI")]
    [SerializeField] private SumGame sumGameLogic;
    [SerializeField] private Button playSumGameButton;
    [SerializeField] private TextMeshProUGUI numbersText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI costAndRewardText;

    private void Start()
    {
        // --- BU KISIM DEĞİŞTİ ---
        // Sadece kapatma butonunu dinliyoruz.
        closeMiniGamesButton.onClick.AddListener(CloseMiniGamesPanel);
        // ---------------------------

        playSumGameButton.onClick.AddListener(PlaySumGame);
        resultText.text = "";
    }

    // OnEnable, obje aktif olduğunda çalışan bir Unity fonksiyonudur.
    // Panelimiz açıldığında bu fonksiyon tetiklenecek.
    private void OnEnable()
    {
        // Paneli açarken maliyet/ödül metnini güncelle
        costAndRewardText.text = $"Maliyet: {sumGameLogic.CostToPlay} Altın\nÖdül: {sumGameLogic.RewardOnWin} Altın";
        numbersText.text = "Sayılar: ?, ?";
        resultText.text = ""; // Her açıldığında sonuç metnini temizle
    }

    private void Update()
    {
        // Cooldown metni aynı şekilde çalışmaya devam ediyor.
        // Bu script artık sadece panel aktifken çalıştığı için if kontrolüne gerek yok.
        float remainingCooldown = MiniGameManager.Instance.GetRemainingCooldown(sumGameLogic.GameID);
        if (remainingCooldown > 0)
        {
            playSumGameButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Oyna ({remainingCooldown:F0}s)";
            playSumGameButton.interactable = false;
        }
        else
        {
            playSumGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Oyna";
            playSumGameButton.interactable = true;
        }
    }

    // --- BU KISIM DEĞİŞTİ ---
    // Kapatma fonksiyonu artık kendi panelini kapatıyor.
    private void CloseMiniGamesPanel()
    {
        gameObject.SetActive(false);
    }
    // ---------------------------

    // ... (PlaySumGame ve diğer fonksiyonlar aynı, değişiklik yok) ...

    private void PlaySumGame()
    {
        if (CurrencyManager.Instance.CurrentGold < sumGameLogic.CostToPlay)
        {
            resultText.text = "Yetersiz Altın!";
            AnimateResultText();
            return;
        }
        CurrencyManager.Instance.SpendGold(sumGameLogic.CostToPlay);
        MiniGameManager.Instance.RecordPlayTime(sumGameLogic.GameID);
        sumGameLogic.Play(HandleSumGameResult);
        playSumGameButton.interactable = false;
    }

    private void HandleSumGameResult(MiniGameResult result)
    {
        numbersText.text = result.NumbersGenerated;
        resultText.text = result.ResultMessage;
        if (result.IsWin)
        {
            CurrencyManager.Instance.AddGold(result.Payout);
        }
        AnimateResultText();
    }

    private void AnimateResultText()
    {
        resultText.transform.localScale = Vector3.zero;
        resultText.DOFade(1, 0);
        resultText.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
        resultText.DOFade(0, 1f).SetDelay(2f);
    }
}