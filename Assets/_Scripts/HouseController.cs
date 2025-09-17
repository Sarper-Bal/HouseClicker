using UnityEngine;
using DG.Tweening; // DOTween kütüphanesini kullanacağımızı belirtiyoruz.
using UnityEngine.EventSystems; // Modern input sistemi için bu satırı ekliyoruz.

// IPointerDownHandler arayüzünü ekleyerek bu script'in tıklama/dokunma olaylarını dinleyebileceğini belirtiyoruz.
public class HouseController : MonoBehaviour, IPointerDownHandler
{
    // Animasyon ayarları
    [SerializeField] private float punchScaleAmount = 0.1f;
    [SerializeField] private float punchScaleDuration = 0.15f;

    // OnMouseDown yerine bu fonksiyonu kullanacağız.
    // Bu fonksiyon, Event System tarafından objeye bir tıklama veya dokunma geldiğinde otomatik olarak çağrılır.
    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. Gerekli yöneticilere ulaşılabiliyor mu diye kontrol et.
        if (UpgradeManager.Instance == null || CurrencyManager.Instance == null)
        {
            Debug.LogError("Manager'lar sahnede bulunamadı!");
            return;
        }

        // 2. Mevcut seviye verisini al.
        LevelData currentLevelData = UpgradeManager.Instance.GetCurrentLevelData();

        if (currentLevelData != null)
        {
            // 3. Altını ekle.
            CurrencyManager.Instance.AddGold(currentLevelData.goldPerClick);

            // 4. DOTween ile basit bir tıklama animasyonu (geri bildirim).
            transform.DOKill(); // Önceki animasyonları bitir.
            transform.DOPunchScale(Vector3.one * punchScaleAmount, punchScaleDuration, 1, 0.5f);
        }
    }
}