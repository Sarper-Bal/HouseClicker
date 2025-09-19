using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class MiniGameUIController : MonoBehaviour
{
    [Header("Ana Paneller ve Butonlar")]
    [SerializeField] private Button closeMiniGamesButton;

    [Header("Sayı Toplama Oyunu (Sum Game) UI")]
    [SerializeField] private SumGame sumGameLogic;
    [SerializeField] private Button playSumGameButton;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI costAndRewardText;

    [Header("İnteraktif Kartlar")]
    [SerializeField] private RevealCard card1;
    [SerializeField] private RevealCard card2;

    private int num1, num2;
    private int cardsRevealedCount;
    private Vector3 resultTextOriginalScale;

    private void Start()
    {
        closeMiniGamesButton.onClick.AddListener(CloseMiniGamesPanel);
        playSumGameButton.onClick.AddListener(StartNewGame);

        card1.OnCardRevealed += OnCardRevealed;
        card2.OnCardRevealed += OnCardRevealed;

        if (resultText != null)
        {
            resultTextOriginalScale = resultText.transform.localScale;
        }
    }

    private void OnEnable()
    {
        costAndRewardText.text = $"Maliyet: {sumGameLogic.CostToPlay} Altın\nÖdül: {sumGameLogic.RewardOnWin} Altın";
        ResetUIForNewGame();
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

    private void StartNewGame()
    {
        if (CurrencyManager.Instance.CurrentGold < sumGameLogic.CostToPlay)
        {
            resultText.text = "Yetersiz Altın!";
            AnimateResultText();
            return;
        }
        CurrencyManager.Instance.SpendGold(sumGameLogic.CostToPlay);
        MiniGameManager.Instance.RecordPlayTime(sumGameLogic.GameID);

        SumGameData data = sumGameLogic.GameData;

        num1 = Random.Range(data.minNumber, data.maxNumber + 1);

        // --- KOPYALA-YAPIŞTIR HATASI BURADA DÜZELTİLDİ ---
        // Eskiden: Random.Range(data.maxNumber, data.maxNumber + 1) yazıyordu.
        num2 = Random.Range(data.minNumber, data.maxNumber + 1);
        // -------------------------------------------------

        ResetUIForNewGame();
        card1.SetupCard(num1);
        card2.SetupCard(num2);
    }

    private void OnCardRevealed()
    {
        cardsRevealedCount++;
        if (cardsRevealedCount >= 2)
        {
            StartCoroutine(ShowResultAfterDelay(0.5f));
        }
    }

    private IEnumerator ShowResultAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SumGameData data = sumGameLogic.GameData;
        int total = num1 + num2;
        MiniGameResult result;
        if (total <= data.targetSum)
        {
            result = new MiniGameResult { IsWin = true, Payout = sumGameLogic.RewardOnWin, ResultMessage = $"{num1} + {num2} = {total}\nKazandın!" };
            CurrencyManager.Instance.AddGold(result.Payout);
        }
        else
        {
            result = new MiniGameResult { IsWin = false, Payout = 0, ResultMessage = $"{num1} + {num2} = {total}\nKaybettin!" };
        }
        resultText.text = result.ResultMessage;
        AnimateResultText();
    }

    private void ResetUIForNewGame()
    {
        resultText.text = "";
        cardsRevealedCount = 0;
        card1.ResetCard();
        card2.ResetCard();
    }

    private void AnimateResultText()
    {
        resultText.transform.localScale = Vector3.zero;
        resultText.DOFade(1, 0);
        resultText.transform.DOScale(resultTextOriginalScale, 0.5f).SetEase(Ease.OutBack);
        resultText.DOFade(0, 1f).SetDelay(3f);
    }
}