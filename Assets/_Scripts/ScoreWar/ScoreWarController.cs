using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

[System.Serializable]
public class ScoreBox
{
    public GameObject boxObject;
    public TextMeshProUGUI modifierText;
    [HideInInspector] public float modifierValue;
    [HideInInspector] public bool isMultiplier;
}

public class ScoreWarController : MonoBehaviour
{
    [Header("Oyun Verisi")]
    [SerializeField] private ScoreWarData gameData;

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
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI enemyScoreText;
    [SerializeField] private List<ScoreBox> playerBoxes;
    [SerializeField] private List<ScoreBox> enemyBoxes;
    [SerializeField] private TextMeshProUGUI turnIndicatorText;

    [Header("Görsel Efektler")]
    [SerializeField] private Transform playerSpriteTransform;
    [SerializeField] private Transform enemySpriteTransform;
    [SerializeField] private float minSpriteScale = 0.6f;
    [SerializeField] private float maxSpriteScale = 2.0f;

    [Header("Sonuç Paneli UI")]
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI resultInfoText;
    [SerializeField] private Button finishAndCloseButton;

    private float playerScore;
    private float enemyScore;
    private long currentBetAmount;
    private Vector3 initialPlayerScale;
    private Vector3 initialEnemyScale;

    // --- YENİ EKLENEN KISIM ---
    // Kutuların orijinal boyutlarını saklamak için bir Dictionary
    private Dictionary<GameObject, Vector3> initialBoxScales = new Dictionary<GameObject, Vector3>();
    // --- YENİ KISIM SONU ---

    private void Awake()
    {
        if (playerSpriteTransform != null) initialPlayerScale = playerSpriteTransform.localScale;
        if (enemySpriteTransform != null) initialEnemyScale = enemySpriteTransform.localScale;

        // --- YENİ EKLENEN KISIM ---
        // Oyuna başlamadan önce TÜM kutuların orijinal boyutlarını hafızaya al
        foreach (var box in playerBoxes.Concat(enemyBoxes))
        {
            if (box.boxObject != null && !initialBoxScales.ContainsKey(box.boxObject))
            {
                initialBoxScales.Add(box.boxObject, box.boxObject.transform.localScale);
            }
        }
        // --- YENİ KISIM SONU ---
    }

    private void Start()
    {
        startGameButton.onClick.AddListener(StartGame);
        closeButton.onClick.AddListener(CloseMainPanel);
        finishAndCloseButton.onClick.AddListener(CloseMainPanel);
        mainPanel.SetActive(false);
    }

    private long GetCurrentBetAmount()
    {
        if (UpgradeManager.Instance == null) return gameData.baseBetAmount;
        int playerLevel = UpgradeManager.Instance.CurrentLevel;
        return gameData.baseBetAmount + (playerLevel * gameData.amountPerLevel);
    }

    public void ShowConfirmationPanel()
    {
        float cooldown = MiniGameManager.Instance.GetRemainingCooldown(gameData.gameName);
        if (cooldown > 0)
        {
            Debug.Log($"Tekrar oynamak için {cooldown:F0} saniye beklemelisin.");
            return;
        }

        mainPanel.SetActive(true);
        confirmationPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        resultPanel.SetActive(false);

        gameNameText.text = gameData.gameName;
        currentBetAmount = GetCurrentBetAmount();
        betInfoText.text = currentBetAmount.ToString("N0", CultureInfo.InvariantCulture);

        if (CurrencyManager.Instance.CurrentGold >= currentBetAmount)
        {
            startGameButton.interactable = true;
            betInfoText.color = Color.white;
        }
        else
        {
            startGameButton.interactable = false;
            betInfoText.color = Color.red;
        }
    }

    private void StartGame()
    {
        confirmationPanel.SetActive(false);
        gameplayPanel.SetActive(true);

        currentBetAmount = GetCurrentBetAmount();
        CurrencyManager.Instance.SpendGold(currentBetAmount);
        MiniGameManager.Instance.RecordPlayTime(gameData.gameName);

        playerScore = gameData.startScore;
        enemyScore = gameData.startScore;
        UpdateScoreText(playerScoreText, playerScore, false);
        UpdateScoreText(enemyScoreText, enemyScore, false);

        UpdateSpriteScale(playerSpriteTransform, playerScore, initialPlayerScale);
        UpdateSpriteScale(enemySpriteTransform, enemyScore, initialEnemyScale);

        AssignModifiers();
        ResetBoxVisuals(); // Bu fonksiyon artık doğru boyutları kullanacak

        StartCoroutine(DuelSequence());
    }

    private void AssignModifiers()
    {
        List<float> availableModifiers = new List<float>();
        availableModifiers.AddRange(gameData.additiveModifiers.Select(v => (float)v));
        availableModifiers.AddRange(gameData.multiplicativeModifiers);
        var shuffled = availableModifiers.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < 2; i++)
        {
            AssignRandomModifier(playerBoxes[i], shuffled);
            AssignRandomModifier(enemyBoxes[i], shuffled);
        }
    }

    private void AssignRandomModifier(ScoreBox box, List<float> shuffledModifiers)
    {
        if (shuffledModifiers.Count == 0) return;
        float modifier = shuffledModifiers[0];
        shuffledModifiers.RemoveAt(0);
        box.modifierValue = modifier;
        box.isMultiplier = Mathf.Abs(modifier) > 1 && Mathf.Abs(modifier) < 10;
    }

    private IEnumerator DuelSequence()
    {
        yield return new WaitForSeconds(1f);

        turnIndicatorText.text = "Senin Sıran";
        yield return RevealBox(playerBoxes[0], playerScoreText, playerSpriteTransform, initialPlayerScale, (newScore) => playerScore = newScore);
        yield return new WaitForSeconds(1.5f);

        turnIndicatorText.text = "Rakip Sırası";
        yield return RevealBox(enemyBoxes[0], enemyScoreText, enemySpriteTransform, initialEnemyScale, (newScore) => enemyScore = newScore);
        yield return new WaitForSeconds(1.5f);

        turnIndicatorText.text = "Senin Sıran";
        yield return RevealBox(playerBoxes[1], playerScoreText, playerSpriteTransform, initialPlayerScale, (newScore) => playerScore = newScore);
        yield return new WaitForSeconds(1.5f);

        turnIndicatorText.text = "Rakip Sırası";
        yield return RevealBox(enemyBoxes[1], enemyScoreText, enemySpriteTransform, initialEnemyScale, (newScore) => enemyScore = newScore);
        yield return new WaitForSeconds(2f);

        FinishGame();
    }

    private IEnumerator RevealBox(ScoreBox box, TextMeshProUGUI scoreText, Transform spriteTransform, Vector3 initialScale, System.Action<float> updateScoreAction)
    {
        float currentScore = (scoreText == playerScoreText) ? playerScore : enemyScore;

        Sequence revealAnim = DOTween.Sequence();
        revealAnim.Append(box.boxObject.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f));
        revealAnim.AppendCallback(() =>
        {
            box.modifierText.gameObject.SetActive(true);
            string text = "";
            if (box.isMultiplier) text = box.modifierValue > 0 ? $"x{box.modifierValue}" : $"÷{-box.modifierValue}";
            else text = box.modifierValue > 0 ? $"+{box.modifierValue}" : $"{box.modifierValue}";
            box.modifierText.text = text;
            box.modifierText.color = box.modifierValue > 0 ? Color.green : Color.red;
            box.modifierText.transform.localScale = Vector3.zero;
        });
        revealAnim.Append(box.modifierText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));

        yield return revealAnim.WaitForCompletion();

        float newScore;
        if (box.isMultiplier) newScore = box.modifierValue > 0 ? currentScore * box.modifierValue : currentScore / -box.modifierValue;
        else newScore = currentScore + box.modifierValue;

        updateScoreAction(newScore);
        UpdateScoreText(scoreText, newScore, true);
        UpdateSpriteScale(spriteTransform, newScore, initialScale);
    }

    private void FinishGame()
    {
        string title;
        string info;

        if (playerScore > enemyScore)
        {
            title = "Kazandın!";
            info = $"Ödül: {gameData.rewardAmount}";
            CurrencyManager.Instance.AddGold(gameData.rewardAmount);
        }
        else if (enemyScore > playerScore)
        {
            title = "Kaybettin!";
            info = "Bir Sonraki Sefere";
        }
        else
        {
            title = "Berabere!";
            info = "Bahis İade Edildi";
            if (gameData.refundOnDraw) CurrencyManager.Instance.AddGold(currentBetAmount);
        }

        ShowResultPanel(title, info);
    }

    private void ShowResultPanel(string title, string info)
    {
        gameplayPanel.SetActive(false);
        resultPanel.SetActive(true);
        resultTitleText.text = title;
        resultInfoText.text = info;
    }

    private void CloseMainPanel()
    {
        mainPanel.SetActive(false);
    }

    private void ResetBoxVisuals()
    {
        foreach (var box in playerBoxes.Concat(enemyBoxes))
        {
            // --- DEĞİŞİKLİK BURADA ---
            // Artık boyutu (1,1,1)'e değil, hafızaya aldığımız orijinal boyutuna sıfırlıyoruz.
            if (initialBoxScales.ContainsKey(box.boxObject))
            {
                box.boxObject.transform.localScale = initialBoxScales[box.boxObject];
            }
            // --- DEĞİŞİKLİK SONU ---
            box.modifierText.gameObject.SetActive(false);
        }
    }

    private void UpdateScoreText(TextMeshProUGUI textElement, float targetScore, bool animate)
    {
        float startScore = animate ? float.Parse(textElement.text, CultureInfo.InvariantCulture) : targetScore;

        DOTween.To(() => startScore, x => startScore = x, targetScore, 0.5f)
               .OnUpdate(() => textElement.text = Mathf.RoundToInt(startScore).ToString());

        if (animate) textElement.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f);
    }

    private void UpdateSpriteScale(Transform spriteTransform, float score, Vector3 initialScale)
    {
        if (spriteTransform == null) return;

        float scaleFactor = score / gameData.startScore;
        scaleFactor = Mathf.Clamp(scaleFactor, minSpriteScale, maxSpriteScale);
        Vector3 newScale = initialScale * scaleFactor;
        spriteTransform.DOScale(newScale, 0.5f).SetEase(Ease.OutQuad);
    }
}