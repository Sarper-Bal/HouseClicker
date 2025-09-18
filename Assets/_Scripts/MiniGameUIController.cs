using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MiniGameUIController : MonoBehaviour
{
    [Header("Ana Paneller ve Butonlar")]
    [SerializeField] private Button closeMiniGamesButton;

    [Header("Sayı Toplama Oyunu (Sum Game) UI")]
    [SerializeField] private SumGame sumGameLogic;
    [SerializeField] private Button playSumGameButton;
    [SerializeField] private TextMeshProUGUI numbersText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI costAndRewardText;

    // --- YENİ EKLENEN KISIM ---
    // Sonuç metninin orijinal ölçeğini saklamak için bir değişken.
    private Vector3 resultTextOriginalScale;
    // -------------------------

    private void Start()
    {
        closeMiniGamesButton.onClick.AddListener(CloseMiniGamesPanel);
        playSumGameButton.onClick.AddListener(PlaySumGame);

        // --- YENİ EKLENEN KISIM ---
        // Oyun başlarken sonuç metninin mevcut ölçeğini hafızaya al.
        if (resultText != null)
        {
            resultTextOriginalScale = resultText.transform.localScale;
        }
        // -------------------------

        resultText.text = "";
    }

    private void OnEnable()
    {
        costAndRewardText.text = $"Maliyet: {sumGameLogic.CostToPlay} Altın\nÖdül: {sumGameLogic.RewardOnWin} Altın";
        numbersText.text = "Sayılar: ?, ?";
        resultText.text = "";
    }

    private void Update()
    {
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

    private void CloseMiniGamesPanel()
    {
        gameObject.SetActive(false);
    }

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
        // --- GÜNCELLENEN KISIM ---
        resultText.transform.localScale = Vector3.zero;
        resultText.DOFade(1, 0);
        // Animasyonu sabit bir '1' değerine değil, hafızadaki 'orijinal' ölçeğe yap.
        resultText.transform.DOScale(resultTextOriginalScale, 0.5f).SetEase(Ease.OutBack);
        // -------------------------

        resultText.DOFade(0, 1f).SetDelay(2f);
    }
}