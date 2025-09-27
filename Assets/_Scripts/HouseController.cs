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

        clickSequence.AppendCallback(CreateShockwave);
        clickSequence.Join(transform.DOPunchPosition(Vector3.up * 0.2f, 0.4f, 1, 0.5f));

        clickSequence.Append(transform.DOScale(originalScale, 0.5f).SetEase(Ease.OutElastic));
        clickSequence.Join(transform.DOPunchRotation(new Vector3(0, 0, 8f), 0.5f, 15, 1));

        clickSequence.Join(houseSpriteRenderer.DOColor(originalColor, 0.4f));
    }

    private void CreateShockwave()
    {
        if (shockwavePrefab == null) return;

        GameObject shockwaveInstance = Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
        Destroy(shockwaveInstance, 0.5f);

        SpriteRenderer swRenderer = shockwaveInstance.GetComponent<SpriteRenderer>();
        if (swRenderer == null) return;

        shockwaveInstance.transform.localScale = originalScale * 0.1f;

        Sequence shockwaveSequence = DOTween.Sequence();
        shockwaveSequence.Append(shockwaveInstance.transform.DOScale(originalScale * 2f, 0.5f));
        shockwaveSequence.Join(swRenderer.DOFade(0, 0.5f).SetEase(Ease.InQuad));
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