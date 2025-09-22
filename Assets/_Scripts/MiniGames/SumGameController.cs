using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

// Bu script, sadece Sayı Toplama mini oyununun UI panelini ve mantığını yönetir.
public class SumGameController : MonoBehaviour
{
    [Header("Oyun Verisi")]
    [Tooltip("Bu oyunun kurallarını içeren ScriptableObject.")]
    [SerializeField] private SumGameData gameData;

    [Header("UI Elemanları")]
    [SerializeField] private GameObject gamePanel; // Bu controller'ın yönettiği ana panel (kendisi).
    [SerializeField] private Button playButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI costAndRewardText;
    [SerializeField] private RevealCard card1;
    [SerializeField] private RevealCard card2;

    private int num1, num2;
    private int cardsRevealedCount;
    private Vector3 resultTextOriginalScale;
    private CanvasGroup panelCanvasGroup;

    private void Awake()
    {
        if (gamePanel != null)
        {
            panelCanvasGroup = gamePanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = gamePanel.AddComponent<CanvasGroup>();
            }
        }
    }

    private void Start()
    {
        playButton.onClick.AddListener(PlayGame);
        closeButton.onClick.AddListener(ClosePanel);

        card1.OnCardRevealed += OnCardRevealed;
        card2.OnCardRevealed += OnCardRevealed;

        if (resultText != null)
        {
            resultTextOriginalScale = resultText.transform.localScale;
        }

        // Script başladığında, bağlı olduğu paneli kendiliğinden kapatsın.
        if (gamePanel != null)
        {
            gamePanel.SetActive(false);
        }
        else
        {
            Debug.LogError("HATA: 'Game Panel' referansı atanmamış!", this.gameObject);
        }
    }

    // Bu metod, ana UI'daki "Sayı Oyunu Oyna" butonu tarafından çağrılacak.
    public void ShowPanel()
    {
        if (CurrencyManager.Instance.CurrentGold < gameData.costToPlay)
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

        SetupPanel();

        // Paneli aktif et ve görünürlüğünü garanti altına al.
        gamePanel.SetActive(true);
        gamePanel.transform.localScale = Vector3.one;
        panelCanvasGroup.alpha = 1f;
        panelCanvasGroup.interactable = true;
        panelCanvasGroup.blocksRaycasts = true;
    }

    private void SetupPanel()
    {
        costAndRewardText.text = $"Maliyet: {gameData.costToPlay} Altın\nÖdül: {gameData.rewardOnWin} Altın";
        resultText.text = "";
        cardsRevealedCount = 0;
        card1.ResetCard();
        card2.ResetCard();
        playButton.interactable = true;
    }

    private void PlayGame()
    {
        CurrencyManager.Instance.SpendGold(gameData.costToPlay);
        MiniGameManager.Instance.RecordPlayTime(gameData.gameName);
        playButton.interactable = false;

        num1 = Random.Range(1, 11);
        num2 = Random.Range(1, 11);

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
        int total = num1 + num2;
        string message;

        if (total <= gameData.targetSum)
        {
            message = $"{num1} + {num2} = {total}\nKazandın!";
            CurrencyManager.Instance.AddGold(gameData.rewardOnWin);
        }
        else
        {
            message = $"{num1} + {num2} = {total}\nKaybettin!";
        }

        resultText.text = message;
        AnimateResultText(resultText);
        playButton.interactable = true;
    }

    public void ClosePanel()
    {
        gamePanel.SetActive(false);
    }

    private void AnimateResultText(TextMeshProUGUI text)
    {
        if (text == null) return;
        text.transform.localScale = Vector3.zero;
        text.alpha = 1;
        text.transform.DOScale(resultTextOriginalScale, 0.5f).SetEase(Ease.OutBack);
        text.DOFade(0, 1f).SetDelay(3f);
    }

    private void OnDestroy()
    {
        if (playButton != null) playButton.onClick.RemoveAllListeners();
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();
        if (card1 != null) card1.OnCardRevealed -= OnCardRevealed;
        if (card2 != null) card2.OnCardRevealed -= OnCardRevealed;
    }
}