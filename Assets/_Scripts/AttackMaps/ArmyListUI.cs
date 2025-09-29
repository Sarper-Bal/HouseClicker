// Dosya Adı: ArmyListUI.cs
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class ArmyListUI : MonoBehaviour
{
    [SerializeField] private GameObject armyUnitEntryPrefab;

    // --- YENİ EKLENEN METOT ---
    private void OnDestroy()
    {
        // Bu obje (ve içindeki tüm asker satırları) yok edilirken,
        // onlara bağlı tüm DOTween animasyonlarını anında durdur.
        // Bu, sahne geçişlerindeki "Missing Target" hatasını tamamen çözer.
        transform.DOKill();
    }
    // -------------------------

    private Dictionary<string, ArmyUnitEntryUI> listItems = new Dictionary<string, ArmyUnitEntryUI>();

    public void UpdateList(Dictionary<string, int> armyComposition, Dictionary<string, Sprite> iconDict)
    {
        // Önce listeden kaldırılacakları tespit et
        List<string> soldiersToRemove = new List<string>();
        foreach (var listItem in listItems)
        {
            if (!armyComposition.ContainsKey(listItem.Key) || armyComposition[listItem.Key] <= 0)
            {
                soldiersToRemove.Add(listItem.Key);
            }
        }

        // Tespit edilenleri listeden kaldır ve animasyonla yok et
        foreach (string soldierName in soldiersToRemove)
        {
            if (listItems.ContainsKey(soldierName))
            {
                ArmyUnitEntryUI itemToDestroy = listItems[soldierName];
                listItems.Remove(soldierName);

                // Animasyonla yok etme
                itemToDestroy.transform.DOScale(0, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    Destroy(itemToDestroy.gameObject);
                });
            }
        }

        // Şimdi mevcut orduya göre listeyi güncelle veya yeni eleman ekle
        foreach (var soldier in armyComposition)
        {
            string soldierName = soldier.Key;
            int soldierCount = soldier.Value;

            if (soldierCount <= 0) continue;

            if (listItems.ContainsKey(soldierName))
            {
                // Zaten varsa, sayısını güncelle
                listItems[soldierName].Setup(soldierName, iconDict[soldierName], soldierCount);
            }
            else
            {
                // Yoksa, yeni bir satır oluştur
                GameObject itemGO = Instantiate(armyUnitEntryPrefab, transform);
                ArmyUnitEntryUI itemUI = itemGO.GetComponent<ArmyUnitEntryUI>();
                itemUI.Setup(soldierName, iconDict[soldierName], soldierCount);
                listItems[soldierName] = itemUI;

                // Animasyonla belirme
                itemGO.transform.localScale = Vector3.zero;
                itemGO.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
            }
        }
    }
}