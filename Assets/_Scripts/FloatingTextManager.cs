using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingTextManager : MonoBehaviour
{
    [Header("Genel Ayarlar")]
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Canvas mainCanvas;

    [Header("Tıklama Metni Ayarları")]
    [SerializeField] private Transform clickSpawnPoint;
    [SerializeField] private Color clickTextColor = Color.yellow;

    [Header("Pasif Gelir Metni Ayarları")]
    [SerializeField] private Transform passiveSpawnPoint;
    [SerializeField] private Color passiveTextColor = Color.white;

    [Header("Animasyon Ayarları")]
    [SerializeField] private float moveDistance = 150f;
    [SerializeField] private float duration = 1.5f;

    void Start()
    {
        // Bu script (ve MainScene) yüklendiğinde event'leri dinlemeye başla
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
        // Bu script (ve MainScene) yok edildiğinde event'leri dinlemeyi bırak
        if (HouseController.FindObjectOfType<HouseController>() != null)
        {
            HouseController.FindObjectOfType<HouseController>().OnClickedForGold -= HandleClickIncome;
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnPassiveGoldAdded -= HandlePassiveIncome;
            // CurrencyManager.Instance.StopPassiveIncome(); satırı buradan kaldırıldı.
        }
    }

    private void HandleClickIncome(long amount)
    {
        if (clickSpawnPoint == null) return;
        ShowFloatingText($"+{amount}", clickSpawnPoint.position, clickTextColor);
    }

    private void HandlePassiveIncome(long amount)
    {
        if (passiveSpawnPoint == null) return;
        ShowFloatingText($"+{amount}", passiveSpawnPoint.position, passiveTextColor);
    }

    private void ShowFloatingText(string message, Vector3 worldPosition, Color color)
    {
        if (mainCanvas == null || floatingTextPrefab == null) return;

        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        GameObject textObject = Instantiate(floatingTextPrefab, mainCanvas.transform);
        textObject.transform.position = screenPosition;

        TextMeshProUGUI tmpText = textObject.GetComponent<TextMeshProUGUI>();
        tmpText.text = message;
        tmpText.color = color;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(textObject.transform.DOMoveY(screenPosition.y + moveDistance, duration).SetEase(Ease.OutQuad));
        sequence.Join(tmpText.DOFade(0, duration).SetEase(Ease.InQuad));
        sequence.SetLink(textObject);
        sequence.OnComplete(() =>
        {
            if (textObject != null)
                Destroy(textObject);
        });
    }
}