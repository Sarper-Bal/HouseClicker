using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class RevealCard : MonoBehaviour
{
    [SerializeField] private Button cardButton;
    [SerializeField] private TextMeshProUGUI cardText;

    // --- YENİ EKLENEN KISIM ---
    private Vector3 originalScale; // Kartın orijinal ölçeğini saklamak için.
                                   // -------------------------

    private int numberValue;
    private bool isRevealed = false;

    public event Action OnCardRevealed;

    // Awake, Start'tan önce çalışır ve objenin temel bilgilerini almak için idealdir.
    private void Awake()
    {
        // --- YENİ EKLENEN KISIM ---
        // Oyun başlar başlamaz bu kartın orijinal ölçeğini hafızaya al.
        originalScale = transform.localScale;
        // -------------------------
    }

    private void Start()
    {
        cardButton.onClick.AddListener(FlipCard);
    }

    public void SetupCard(int number)
    {
        this.numberValue = number;
        ResetCard();
    }

    private void FlipCard()
    {
        if (isRevealed) return;

        isRevealed = true;
        cardButton.interactable = false;

        Sequence flipSequence = DOTween.Sequence();

        // --- GÜNCELLENEN KISIM ---
        flipSequence.Append(transform.DOScaleX(0, 0.2f).SetEase(Ease.InQuad));
        flipSequence.AppendCallback(() =>
        {
            cardText.text = numberValue.ToString();
        });
        // Animasyon bittiğinde X ölçeğini sabit '1' yerine, orijinal X ölçeğine döndür.
        flipSequence.Append(transform.DOScaleX(originalScale.x, 0.2f).SetEase(Ease.OutQuad));
        // -------------------------

        flipSequence.OnComplete(() =>
        {
            OnCardRevealed?.Invoke();
        });
    }

    public void ResetCard()
    {
        isRevealed = false;
        cardText.text = "?";
        cardButton.interactable = true;

        // --- GÜNCELLENEN KISIM ---
        // Ölçeği sabit 'Vector3.one' yerine hafızadaki orijinal ölçeğe sıfırla.
        transform.localScale = originalScale;
        // -------------------------
    }
}