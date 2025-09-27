using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingTextManager : MonoBehaviour
{
    [Header("Temel Ayarlar")]
    [Tooltip("Ekranda belirecek yazı prefab'ı (TextMeshProUGUI objesi olmalı).")]
    [SerializeField] private GameObject floatingTextPrefab;
    [Tooltip("Yaratılan tüm yazıların altında toplanacağı, Canvas'a bağlı boş bir obje.")]
    [SerializeField] private Transform textParentCanvas;

    [Header("Spawn Noktaları")]
    [Tooltip("Tıklama sonrası altın yazısının çıkacağı nokta.")]
    [SerializeField] private Transform clickSpawnPoint;
    [Tooltip("Pasif gelir yazısının çıkacağı nokta.")]
    [SerializeField] private Transform passiveSpawnPoint;

    // --- YENİ EKLENEN KISIM: Inspector'dan Ayarlanabilir Renkler ---
    [Header("Renk Ayarları")]
    [SerializeField] private Color clickTextColor = Color.yellow;
    [SerializeField] private Color passiveTextColor = Color.green;
    // ----------------------------------------------------------------

    private Camera mainCamera;
    private HouseController houseControllerRef;

    void Start()
    {
        mainCamera = Camera.main;

        if (floatingTextPrefab == null || textParentCanvas == null)
        {
            Debug.LogError("FloatingTextManager üzerinde 'Floating Text Prefab' veya 'Text Parent Canvas' atanmamış!", this.gameObject);
            return;
        }

        houseControllerRef = FindObjectOfType<HouseController>();
        if (houseControllerRef != null)
        {
            houseControllerRef.OnClickedForGold += HandleClickGold;
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnPassiveGoldAdded += HandlePassiveGold;
        }
    }

    void OnDestroy()
    {
        if (houseControllerRef != null)
        {
            houseControllerRef.OnClickedForGold -= HandleClickGold;
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnPassiveGoldAdded -= HandlePassiveGold;
        }
    }

    private void HandleClickGold(long amount)
    {
        string text = "+ " + FormatNumber(amount);
        // Tıklama için Inspector'dan ayarlanan rengi kullan.
        CreateFloatingText(text, clickTextColor, clickSpawnPoint);
    }

    private void HandlePassiveGold(long amount)
    {
        // --- İSTEĞİN ÜZERİNE "(Pasif)" YAZISI KALDIRILDI ---
        string text = "+ " + FormatNumber(amount);
        // Pasif gelir için Inspector'dan ayarlanan rengi kullan.
        CreateFloatingText(text, passiveTextColor, passiveSpawnPoint);
    }

    private void CreateFloatingText(string text, Color color, Transform spawnPoint)
    {
        if (mainCamera == null)
        {
            return;
        }

        if (floatingTextPrefab == null) return;

        Transform actualSpawnPoint = (spawnPoint != null) ? spawnPoint : this.transform;

        GameObject textGO = Instantiate(floatingTextPrefab, textParentCanvas);

        Vector3 worldPosition = actualSpawnPoint.position;
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        textGO.transform.position = screenPosition;

        TextMeshProUGUI textMesh = textGO.GetComponent<TextMeshProUGUI>();
        if (textMesh == null)
        {
            Debug.LogError("Floating Text Prefab'ında TextMeshProUGUI component'i bulunamadı!", textGO);
            return;
        }

        textMesh.text = text;
        textMesh.color = color;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(textGO.transform.DOMoveY(textGO.transform.position.y + 100f, 1.2f).SetEase(Ease.OutQuad));
        sequence.Join(textMesh.DOFade(0, 1.2f).SetEase(Ease.InQuad));
        sequence.OnComplete(() => Destroy(textGO));
    }

    private string FormatNumber(long num)
    {
        if (num >= 1000)
            return (num / 1000D).ToString("0.#") + "K";
        return num.ToString();
    }
}