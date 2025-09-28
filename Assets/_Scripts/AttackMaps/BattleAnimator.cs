using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleAnimator : MonoBehaviour
{
    [SerializeField] private Image characterImage;

    [Header("Animasyon Yönleri")]
    [Tooltip("Saldırı animasyonunda karakterin hangi yöne atılacağını belirler.")]
    [SerializeField] private Vector2 attackPunchDirection = new Vector2(50f, 0);
    [Tooltip("Ölüm animasyonunda karakterin hangi yöne savrulacağını belirler.")]
    [SerializeField] private Vector2 deathEjectDirection = new Vector2(-100f, 50f);

    private Vector3 initialPosition;
    private Color initialColor;

    private void Awake()
    {
        if (characterImage == null) characterImage = GetComponentInChildren<Image>();
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
        // Vuruş ve geri çekilme hissini artırmak için "Punch" animasyonunu kullanıyoruz.
        // Bu, ileri atılıp hemen geri dönmesini sağlar.
        return GetComponent<RectTransform>().DOPunchAnchorPos(attackPunchDirection, 0.4f, 5, 0.5f);
    }

    public Tween PlayTakeDamage()
    {
        // Sadece sarsılma efekti.
        return transform.DOShakePosition(0.3f, new Vector3(8, 8, 0), 15, 90);
    }

    public Tween PlayDeath()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rectTransform.DOAnchorPos(rectTransform.anchoredPosition + deathEjectDirection, 0.5f).SetEase(Ease.OutQuad));
        sequence.Join(rectTransform.DORotate(new Vector3(0, 0, 45f), 0.5f));
        sequence.Join(characterImage.DOFade(0, 0.4f));
        return sequence;
    }

    public void ResetAnimator()
    {
        transform.DOKill();
        GetComponent<RectTransform>().anchoredPosition = initialPosition;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        characterImage.color = initialColor;
    }
}