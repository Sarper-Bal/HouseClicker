using UnityEngine;
using TMPro; // TextMeshPro kullanacağımız için
using DG.Tweening; // DOTween animasyonları için

public class FloatingTextManager : MonoBehaviour
{
    [Header("Genel Ayarlar")]
    [SerializeField] private GameObject floatingTextPrefab; // Metin prefab'ı
    [SerializeField] private Canvas mainCanvas; // Bütün UI'ın bulunduğu ana Canvas

    [Header("Tıklama Metni Ayarları")]
    [SerializeField] private Transform clickSpawnPoint; // Tıklama metinlerinin doğacağı dünya konumu
    [SerializeField] private Color clickTextColor = Color.yellow;

    [Header("Pasif Gelir Metni Ayarları")]
    [SerializeField] private Transform passiveSpawnPoint; // Pasif gelir metinlerinin doğacağı dünya konumu
    [SerializeField] private Color passiveTextColor = Color.white;

    [Header("Animasyon Ayarları")]
    [SerializeField] private float moveDistance = 150f; // Metnin yukarı doğru gideceği mesafe (piksel cinsinden)
    [SerializeField] private float duration = 1.5f; // Animasyonun süresi

    void Start()
    {
        // Event'lere abone oluyoruz
        if (HouseController.FindObjectOfType<HouseController>() != null)
        {
            HouseController.FindObjectOfType<HouseController>().OnClickedForGold += HandleClickIncome;
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnPassiveGoldAdded += HandlePassiveIncome;
        }
    }

    void OnDestroy()
    {
        // Event aboneliklerini iptal ediyoruz
        if (HouseController.FindObjectOfType<HouseController>() != null)
        {
            HouseController.FindObjectOfType<HouseController>().OnClickedForGold -= HandleClickIncome;
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnPassiveGoldAdded -= HandlePassiveIncome;
        }
    }

    private void HandleClickIncome(long amount)
    {
        ShowFloatingText($"+{amount}", clickSpawnPoint.position, clickTextColor);
    }

    private void HandlePassiveIncome(long amount)
    {
        ShowFloatingText($"+{amount}", passiveSpawnPoint.position, passiveTextColor);
    }

    private void ShowFloatingText(string message, Vector3 worldPosition, Color color)
    {
        // 1. Dünya konumunu ekran konumuna çevir
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // 2. Prefab'dan yeni bir metin objesi yarat (Instantiate)
        GameObject textObject = Instantiate(floatingTextPrefab, mainCanvas.transform);
        textObject.transform.position = screenPosition;

        // 3. Metin ve renk ayarlarını yap
        TextMeshProUGUI tmpText = textObject.GetComponent<TextMeshProUGUI>();
        tmpText.text = message;
        tmpText.color = color;

        // 4. Animasyonu başlat ve bitince objeyi yok et (Destroy)
        tmpText.alpha = 1;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(textObject.transform.DOMoveY(screenPosition.y + moveDistance, duration).SetEase(Ease.OutQuad));
        sequence.Join(tmpText.DOFade(0, duration).SetEase(Ease.InQuad));
        sequence.OnComplete(() =>
        {
            Destroy(textObject);
        });
    }
}