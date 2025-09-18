using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System; // Event için bu satır gerekli.

public class HouseController : MonoBehaviour, IPointerDownHandler
{
    // --- YENİ EKLENEN EVENT ---
    // FloatingTextManager'ın dinleyeceği tıklama sinyali.
    public event Action<long> OnClickedForGold;
    // -------------------------

    [SerializeField] private float punchScaleAmount = 0.1f;
    [SerializeField] private float punchScaleDuration = 0.15f;

    public void OnPointerDown(PointerEventData eventData)
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

            // Altını ekle
            CurrencyManager.Instance.AddGold(goldToAdd);

            // --- EVENT'İ TETİKLE ---
            // Tıkladığımızı ve ne kadar kazandığımızı haber veriyoruz.
            OnClickedForGold?.Invoke(goldToAdd);
            // -------------------------

            // Animasyon
            transform.DOKill();
            transform.DOPunchScale(Vector3.one * punchScaleAmount, punchScaleDuration, 1, 0.5f);
        }
    }
}