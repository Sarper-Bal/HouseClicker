using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;

public class HouseController : MonoBehaviour, IPointerDownHandler
{
    public event Action<long> OnClickedForGold;

    [Header("Animation Settings")]
    [SerializeField] private GameObject shockwavePrefab;

    private Sequence clickSequence;
    private SpriteRenderer houseSpriteRenderer;
    private Vector3 originalScale;
    private Color originalColor;

    private void Awake()
    {
        houseSpriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        originalColor = houseSpriteRenderer.color;
    }

    private void OnDestroy()
    {
        if (clickSequence != null)
        {
            clickSequence.Kill();
        }
        transform.DOKill();

        if (houseSpriteRenderer != null)
        {
            houseSpriteRenderer.DOKill();
        }

        DOTween.KillAll();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickSequence != null && clickSequence.IsActive())
        {
            clickSequence.Complete(true);
        }

        HandleGameplayLogic();
        PlayExaggeratedClickAnimation();
    }

    private void HandleGameplayLogic()
    {
        if (UpgradeManager.Instance == null || CurrencyManager.Instance == null)
        {
            Debug.LogError("Manager'lar sahnede bulunamadı!");
            return;
        }

        LevelData currentLevelData = UpgradeManager.Instance.GetCurrentLevelData();

        if (currentLevelData != null)
        {
            long goldToAdd = currentLevelData.goldPerClick;
            CurrencyManager.Instance.AddGold(goldToAdd);
            OnClickedForGold?.Invoke(goldToAdd);
        }
    }

    private void PlayExaggeratedClickAnimation()
    {
        clickSequence = DOTween.Sequence();

        clickSequence.Append(transform.DOScale(originalScale * 0.8f, 0.07f));
        clickSequence.Join(houseSpriteRenderer.DOColor(Color.white, 0.07f));


        clickSequence.Join(transform.DOPunchPosition(Vector3.up * 0.2f, 0.4f, 1, 0.5f));

        clickSequence.Append(transform.DOScale(originalScale, 0.5f).SetEase(Ease.OutElastic));
        clickSequence.Join(transform.DOPunchRotation(new Vector3(0, 0, 8f), 0.5f, 15, 1));

        clickSequence.Join(houseSpriteRenderer.DOColor(originalColor, 0.4f));
    }



    // --- YENİ EKLENEN METOT ---
    /// <summary>
    /// UIManager tarafından çağrılarak, mevcut seviye verisine göre
    /// evin sprite'ını ve diğer görsellerini günceller.
    /// </summary>
    public void UpdateVisuals(LevelData currentLevelData)
    {
        // LevelData'da evin sprite'ını tutan bir 'houseSprite' alanı olduğunu varsayıyoruz.
        // Kendi projenizdeki alan adıyla (örneğin 'levelSprite') değiştirebilirsiniz.
        if (currentLevelData != null && houseSpriteRenderer != null && currentLevelData.houseSprite != null)
        {
            houseSpriteRenderer.sprite = currentLevelData.houseSprite;
        }
    }
}