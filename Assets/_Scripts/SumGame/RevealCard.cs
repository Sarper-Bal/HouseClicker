using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class RevealCard : MonoBehaviour
{
    [SerializeField] private Button cardButton;
    [SerializeField] private TextMeshProUGUI cardText;

    private Vector3 originalScale;
    private int numberValue;
    private bool isRevealed = false;

    // --- YENİ EKLENEN KISIM ---
    // Script'in boyutunu hafızaya alıp almadığını kontrol etmek için bir bayrak.
    private bool isInitialized = false;
    // -------------------------

    public event Action OnCardRevealed;

    // Awake, obje aktif olur olmaz ilk çağrılan fonksiyonlardan biridir.
    private void Awake()
    {
        // Başlangıçta direkt hafızaya almayı deneriz.
        CheckForInitialization();
    }

    private void Start()
    {
        cardButton.onClick.AddListener(FlipCard);
    }

    // --- YENİ EKLENEN FONKSİYON ---
    // Bu fonksiyon, orijinal ölçeğin hafızaya alınıp alınmadığını kontrol eder.
    // Eğer alınmamışsa, o anki ölçeği hafızaya alır.
    private void CheckForInitialization()
    {
        if (isInitialized) return; // Zaten hafızaya alınmışsa bir şey yapma.

        originalScale = transform.localScale;
        isInitialized = true;
    }
    // -------------------------

    public void SetupCard(int number)
    {
        this.numberValue = number;
        ResetCard();
    }

    private void FlipCard()
    {
        if (isRevealed) return;

        CheckForInitialization(); // Her ihtimale karşı burada da kontrol et.

        isRevealed = true;
        cardButton.interactable = false;

        Sequence flipSequence = DOTween.Sequence();

        flipSequence.Append(transform.DOScaleX(0, 0.2f).SetEase(Ease.InQuad));
        flipSequence.AppendCallback(() =>
        {
            cardText.text = numberValue.ToString();
        });
        flipSequence.Append(transform.DOScaleX(originalScale.x, 0.2f).SetEase(Ease.OutQuad));

        flipSequence.OnComplete(() =>
        {
            OnCardRevealed?.Invoke();
        });
    }

    public void ResetCard()
    {
        CheckForInitialization(); // Sıfırlama yapmadan önce hafızaya aldığından emin ol!

        isRevealed = false;
        cardText.text = "?";
        cardButton.interactable = true;

        transform.localScale = originalScale;
    }
}