using UnityEngine;
using UnityEngine.UI; // LayoutRebuilder için bu satır önemli
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

public class MiniGameUIController : MonoBehaviour
{
    [Header("Ana Paneller")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject gameListPanel;
    [SerializeField] private GameObject sumGamePanel;

    [Header("Liste Ayarları")]
    [SerializeField] private GameObject miniGameListItemPrefab;
    [SerializeField] private Transform listContentParent;
    [SerializeField] private TextMeshProUGUI listStatusText;

    [Header("Oyun Referansları")]
    [SerializeField] private List<SumGame> availableSumGames;

    [Header("Genel Butonlar")]
    [SerializeField] private Button closeAndBackButton;

    [Header("Sayı Oyunu UI Elemanları")]
    [SerializeField] private Button playSumGameButton;
    [SerializeField] private TextMeshProUGUI sumGameResultText;
    [SerializeField] private TextMeshProUGUI sumGameCostText;
    [SerializeField] private RevealCard sumGameCard1;
    [SerializeField] private RevealCard sumGameCard2;

    private SumGame currentActiveGame;
    private int num1, num2;
    private int cardsRevealedCount;
    private Vector3 resultTextOriginalScale;

    private void Start()
    {
        closeAndBackButton.onClick.AddListener(HandleBackAndCloseButton);
        playSumGameButton.onClick.AddListener(PlaySumGame);
        sumGameCard1.OnCardRevealed += OnCardRevealed;
        sumGameCard2.OnCardRevealed += OnCardRevealed;

        if (sumGameResultText != null)
        {
            resultTextOriginalScale = sumGameResultText.transform.localScale;
        }

        // Başlangıçta panelleri kapalı tut
        gameListPanel.SetActive(false);
        sumGamePanel.SetActive(false);
    }

    private void OnEnable()
    {
        // Panel her açıldığında listeyi yeniden oluştur ve doğru paneli göster.
        PopulateGameList();
        ShowGameListPanel();
        if (listStatusText != null) listStatusText.text = "";
    }

    private void PopulateGameList()
    {
        foreach (Transform child in listContentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (SumGame game in availableSumGames)
        {
            if (game.GameData == null)
            {
                Debug.LogError($"HATA! '{game.gameObject.name}' objesindeki SumGame'in GameData'sı atanmamış! Lütfen Inspector'ı kontrol edin.", game.gameObject);
                continue;
            }

            GameObject listItemObject = Instantiate(miniGameListItemPrefab, listContentParent);
            MiniGameListItem listItem = listItemObject.GetComponent<MiniGameListItem>();
            SumGameData data = game.GameData;
            string infoText = $"Maliyet: {data.costToPlay} / Ödül: {data.rewardOnWin}";
            listItem.Setup(data.gameIcon, data.gameName, infoText);
            listItem.AddClickListener(() => TryEnterGame(game));
        }

        // --- SORUN 1 ÇÖZÜMÜ BURADA ---
        // Liste dolduktan sonra, layout grubunu anında güncellemeye zorla.
        LayoutRebuilder.ForceRebuildLayoutImmediate(listContentParent as RectTransform);
        // -----------------------------
    }

    private void TryEnterGame(SumGame game)
    {
        if (CurrencyManager.Instance.CurrentGold >= game.GameData.costToPlay)
        {
            ShowSumGamePanel(game);
        }
        else
        {
            listStatusText.text = "Yetersiz Altın!";
            listStatusText.DOFade(1, 0.2f).OnComplete(() => listStatusText.DOFade(0, 1f).SetDelay(1f));
        }
    }

    // ... Script'in geri kalanı bir öncekiyle aynı ...
    #region Unchanged Methods
    private void ShowGameListPanel()
    {
        gameListPanel.SetActive(true);
        sumGamePanel.SetActive(false);
    }

    private void ShowSumGamePanel(SumGame game)
    {
        currentActiveGame = game;
        gameListPanel.SetActive(false);
        sumGamePanel.SetActive(true);
        SetupSumGamePanel(game);
    }

    private void SetupSumGamePanel(SumGame game)
    {
        SumGameData data = game.GameData;
        sumGameCostText.text = $"Maliyet: {data.costToPlay} Altın\nÖdül: {data.rewardOnWin} Altın";
        sumGameResultText.text = "";
        cardsRevealedCount = 0;
        sumGameCard1.ResetCard();
        sumGameCard2.ResetCard();
        playSumGameButton.interactable = true;
    }

    private void PlaySumGame()
    {
        if (currentActiveGame == null)
        {
            Debug.LogError("Oynanacak aktif bir oyun seçilmemiş!");
            return;
        }

        if (MiniGameManager.Instance.GetRemainingCooldown(currentActiveGame.GameID) > 0) return;

        SumGameData data = currentActiveGame.GameData;

        CurrencyManager.Instance.SpendGold(data.costToPlay);
        MiniGameManager.Instance.RecordPlayTime(currentActiveGame.GameID);

        num1 = Random.Range(data.minNumber, data.maxNumber + 1);
        num2 = Random.Range(data.minNumber, data.maxNumber + 1);

        sumGameCard1.SetupCard(num1);
        sumGameCard2.SetupCard(num2);
        playSumGameButton.interactable = false;
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
        SumGameData data = currentActiveGame.GameData;
        int total = num1 + num2;
        MiniGameResult result;
        if (total <= data.targetSum)
        {
            result = new MiniGameResult { IsWin = true, Payout = data.rewardOnWin, ResultMessage = $"{num1} + {num2} = {total}\nKazandın!" };
            CurrencyManager.Instance.AddGold(result.Payout);
        }
        else
        {
            result = new MiniGameResult { IsWin = false, Payout = 0, ResultMessage = $"{num1} + {num2} = {total}\nKaybettin!" };
        }
        sumGameResultText.text = result.ResultMessage;
        AnimateResultText(sumGameResultText);
        playSumGameButton.interactable = true;
    }

    private void AnimateResultText(TextMeshProUGUI text)
    {
        if (resultTextOriginalScale == Vector3.zero && text != null)
            resultTextOriginalScale = text.transform.localScale;

        text.transform.localScale = Vector3.zero;
        text.DOFade(1, 0);
        text.transform.DOScale(resultTextOriginalScale, 0.5f).SetEase(Ease.OutBack);
        text.DOFade(0, 1f).SetDelay(3f);
    }

    private void HandleBackAndCloseButton()
    {
        if (sumGamePanel.activeSelf)
        {
            ShowGameListPanel();
        }
        else
        {
            mainPanel.SetActive(false);
        }
    }
    #endregion
}