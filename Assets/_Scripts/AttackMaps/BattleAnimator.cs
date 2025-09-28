using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleAnimator : MonoBehaviour
{
    [SerializeField] private Image characterImage;

    [Header("Animasyon Yönleri")]
    [Tooltip("Saldırı animasyonunda karakterin hangi yöne atılacağını belirler. Oyuncu için (50, 0), Düşman için (-50, 0) idealdir.")]
    [SerializeField] private Vector2 attackPunchDirection = new Vector2(50f, 0);

    // --- ÖLÜM ANİMASYONU İÇİN YENİ AYARLAR ---
    [Header("Ölüm Animasyonu Ayarları")]
    [Tooltip("Karakterin geriye doğru ne kadar uzağa savrulacağını belirler.")]
    [SerializeField] private float deathEjectDistance = 400f;
    [Tooltip("Savrulma animasyonunun yayının ne kadar yüksek olacağını belirler.")]
    [SerializeField] private float deathJumpPower = 150f;
    [Tooltip("Savrulma animasyonunun ne kadar süreceğini belirler.")]
    [SerializeField] private float deathAnimationDuration = 0.7f;
    [Tooltip("Savrulurken kaç tur döneceğini belirler.")]
    [SerializeField] private int deathRotateCount = 2;
    // ------------------------------------------

    private Vector3 initialPosition;
    private Color initialColor;

    private void Awake()
    {
        if (characterImage == null)
        {
            characterImage = GetComponentInChildren<Image>();
        }
        initialPosition = GetComponent<RectTransform>().anchoredPosition;
        initialColor = characterImage.color;
    }

    public Tween PlaySpawn()
    {
        transform.localScale = Vector3.zero;
        characterImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        sequence.Join(characterImage.DOFade(1, 0.3f));
        return sequence;
    }

    public Tween PlayAttack()
    {
        return GetComponent<RectTransform>().DOPunchAnchorPos(attackPunchDirection, 0.4f, 5, 0.5f);
    }

    public Tween PlayTakeDamage()
    {
        return transform.DOShakePosition(0.3f, new Vector3(8, 8, 0), 15, 90);
    }

    // --- YENİ VE DAHA EĞLENCELİ ÖLÜM ANİMASYONU ---
    public Tween PlayDeath()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Sequence sequence = DOTween.Sequence();

        // Karakterin pozisyonuna göre geriye doğru bir hedef belirle
        // attackPunchDirection pozitif ise (oyuncu), geriye doğru negatif hareket etmeli
        float ejectDirection = Mathf.Sign(attackPunchDirection.x) * -1;
        Vector2 targetPos = rectTransform.anchoredPosition + new Vector2(deathEjectDistance * ejectDirection, 0);

        // Yay çizerek hedefe doğru zıpla
        sequence.Append(rectTransform.DOJumpAnchorPos(targetPos, deathJumpPower, 1, deathAnimationDuration).SetEase(Ease.InQuad));
        // Zıplarken aynı anda kendi etrafında dön
        sequence.Join(rectTransform.DORotate(new Vector3(0, 0, 360 * deathRotateCount * ejectDirection), deathAnimationDuration, RotateMode.FastBeyond360));
        // Zıplarken aynı anda küçülerek kaybol
        sequence.Join(transform.DOScale(0f, deathAnimationDuration * 0.8f).SetDelay(deathAnimationDuration * 0.2f));

        return sequence;
    }

    public void ResetAnimator()
    {
        transform.DOKill();
        GetComponent<RectTransform>().anchoredPosition = initialPosition;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        characterImage.color = initialColor;
        characterImage.gameObject.SetActive(true); // Ölüm sonrası tekrar görünür yap
    }
}